using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace QuickSoft.Helpers
{
    // PORT SHIM — restores ASP.NET MVC5's JsonValueProviderFactory, which ASP.NET Core dropped.
    //
    // Dozens of legacy save/print actions POST a SINGLE json body holding SEVERAL named action parameters,
    // e.g.  $.ajax({ contentType:"application/json",
    //                data: JSON.stringify({ array:[[...]], saledata:[...], bsmodel:{...}, BalanceAmount:0 }) })
    // hitting  UpdateSale(string[][] array, string[] saledata, SEBillSundryViewModel bsmodel, ... ).
    //
    // In MVC5 the framework's JsonValueProviderFactory flattened that json object into model-binder keys
    // (`saledata[0]`, `array[0][1]`, `bsmodel.Prop`, `BalanceAmount`) so every parameter bound BY NAME.
    // ASP.NET Core has no such factory: by default a json body only feeds a single [FromBody] parameter,
    // so in the port `array`/`saledata`/... silently bound to EMPTY arrays and the first `saledata[i]`
    // access threw IndexOutOfRange (surfaced to the user as a generic "Error !" toast on Save / Save+Print).
    //
    // This factory reproduces the MVC5 behaviour verbatim — same key shapes, same primitive rendering, and
    // it preserves json `null` as a bound-null element (so legacy `Convert.ToInt32(saledata[i])` still yields
    // 0 rather than throwing on ""). It is a no-op for non-json requests (the server-side DataTables grids
    // POST form-encoded), and it rewinds the buffered body so an explicit [FromBody]/JsonConvert read still works.
    public sealed class JsonBodyValueProviderFactory : IValueProviderFactory
    {
        public async Task CreateValueProviderAsync(ValueProviderFactoryContext context)
        {
            var request = context.ActionContext.HttpContext.Request;
            if (request == null || !IsJsonRequest(request))
                return;

            // Buffer so we (and any later [FromBody]/JsonConvert reader) can read the body more than once.
            request.EnableBuffering();
            long origPos = request.Body.CanSeek ? request.Body.Position : 0;
            string body;
            if (request.Body.CanSeek)
                request.Body.Position = 0;
            using (var reader = new StreamReader(request.Body, System.Text.Encoding.UTF8,
                                                 detectEncodingFromByteOrderMarks: false,
                                                 bufferSize: 1024, leaveOpen: true))
            {
                body = await reader.ReadToEndAsync();
            }
            if (request.Body.CanSeek)
                request.Body.Position = origPos;

            if (string.IsNullOrWhiteSpace(body))
                return;

            JToken root;
            try
            {
                // DateParseHandling.None keeps every json string EXACTLY as sent (legacy date strings like
                // "dd/MM/yyyy" or "/Date(ms)/" must reach the controller untouched — the simple-type binder
                // converts them where a parameter is actually typed DateTime).
                using var sr = new StringReader(body);
                using var jr = new JsonTextReader(sr) { DateParseHandling = DateParseHandling.None };
                root = JToken.ReadFrom(jr);
            }
            catch
            {
                return; // not parseable json — leave it to the other value providers / [FromBody]
            }

            // Only a top-level json OBJECT maps onto named action parameters (MVC5 parity). A bare array or
            // scalar body is left for an explicit [FromBody] parameter.
            if (root.Type != JTokenType.Object)
                return;

            var store = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            Flatten(string.Empty, root, store);

            // Insert first so the json body wins over route/query values, matching MVC5's provider order.
            context.ValueProviders.Insert(0, new JsonBodyValueProvider(store));
        }

        private static bool IsJsonRequest(HttpRequest request)
        {
            var ct = request.ContentType;
            return ct != null && ct.IndexOf("application/json", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void Flatten(string prefix, JToken token, IDictionary<string, string?> store)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    foreach (var prop in ((JObject)token).Properties())
                        Flatten(MakePropertyKey(prefix, prop.Name), prop.Value, store);
                    break;
                case JTokenType.Array:
                    int i = 0;
                    foreach (var item in (JArray)token)
                        Flatten(MakeArrayKey(prefix, i++), item, store);
                    break;
                case JTokenType.Null:
                case JTokenType.Undefined:
                    // Keep the key (null value) so a fixed-length string[] keeps every slot AND so the element
                    // binds to null (legacy Convert.ToXxx(null) == 0/default, not a "" FormatException).
                    store[prefix] = null;
                    break;
                case JTokenType.Boolean:
                    // MVC5 stored the .NET bool, which the binder stringified as "True"/"False".
                    store[prefix] = (bool)((JValue)token).Value! ? "True" : "False";
                    break;
                default:
                    var val = ((JValue)token).Value;
                    store[prefix] = val == null ? null : Convert.ToString(val, CultureInfo.InvariantCulture);
                    break;
            }
        }

        private static string MakeArrayKey(string prefix, int index)
            => prefix.Length == 0 ? "[" + index.ToString(CultureInfo.InvariantCulture) + "]"
                                  : prefix + "[" + index.ToString(CultureInfo.InvariantCulture) + "]";

        private static string MakePropertyKey(string prefix, string propertyName)
            => prefix.Length == 0 ? propertyName : prefix + "." + propertyName;
    }

    // Backs the flattened json keys. ContainsPrefix mirrors the framework PrefixContainer semantics so the
    // array/collection/complex model binders enumerate `name[i]` / `name.Prop` exactly as for form data.
    internal sealed class JsonBodyValueProvider : IValueProvider
    {
        private readonly IDictionary<string, string?> _store;

        public JsonBodyValueProvider(IDictionary<string, string?> store) => _store = store;

        public bool ContainsPrefix(string prefix)
        {
            if (prefix == null) return false;
            if (prefix.Length == 0) return _store.Count > 0;
            foreach (var key in _store.Keys)
            {
                if (key.Length == prefix.Length)
                {
                    if (string.Equals(key, prefix, StringComparison.OrdinalIgnoreCase)) return true;
                }
                else if (key.Length > prefix.Length &&
                         key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    char c = key[prefix.Length];
                    if (c == '.' || c == '[') return true;
                }
            }
            return false;
        }

        public ValueProviderResult GetValue(string key)
        {
            if (key != null && _store.TryGetValue(key, out var v))
            {
                // A 1-element array holding null => bound element is null (NOT "") and is distinct from
                // ValueProviderResult.None, so the slot is still counted (array length preserved).
                if (v == null)
                    return new ValueProviderResult(new StringValues(new string[1]));
                return new ValueProviderResult(v);
            }
            return ValueProviderResult.None;
        }
    }
}
