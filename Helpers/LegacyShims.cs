using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Models
{
    // Legacy MVC5/OWIN compatibility shims so the controllers port with minimal edits.

    // Legacy `new JsonResult { Data = x }` -> Core JsonResult (Value).
    public class LegacyJsonResult : JsonResult
    {
        public LegacyJsonResult() : base(null) { }
        public object Data { get => Value; set => Value = value; }
    }

    // Legacy MVC5 HttpStatusCodeResult(HttpStatusCode) -> Core StatusCodeResult(int).
    public class HttpStatusCodeResult : StatusCodeResult
    {
        public HttpStatusCodeResult(HttpStatusCode code) : base((int)code) { }
        public HttpStatusCodeResult(int code) : base(code) { }
    }

    // Legacy Session["key"] (object) -> Core ISession (string-backed).
    public class LegacySession
    {
        private readonly ISession _s;
        public LegacySession(ISession s) { _s = s; }
        public object this[string key]
        {
            get => _s == null ? null : _s.GetString(key);
            set { if (_s != null) _s.SetString(key, value?.ToString() ?? string.Empty); }
        }
    }

    // Legacy System.Web.Script.Serialization.JavaScriptSerializer -> Newtonsoft.
    public class JavaScriptSerializer
    {
        public int MaxJsonLength { get; set; }
        public string Serialize(object obj) => Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        public T Deserialize<T>(string input) => Newtonsoft.Json.JsonConvert.DeserializeObject<T>(input);
        public object DeserializeObject(string input) => Newtonsoft.Json.JsonConvert.DeserializeObject(input);
    }

    // Legacy MVC5 Controller.Server (HttpServerUtility) -> shim over LegacyWeb.
    public class LegacyServer
    {
        public string MapPath(string path) => LegacyWeb.MapPath(path);
        public string HtmlEncode(string s) => System.Net.WebUtility.HtmlEncode(s);
        public string UrlEncode(string s) => System.Net.WebUtility.UrlEncode(s);
    }

    // OWIN DefaultAuthenticationTypes constants.
    public static class DefaultAuthenticationTypes
    {
        public const string ApplicationCookie = "ApplicationCookie";
        public const string ExternalCookie = "ExternalCookie";
        public const string TwoFactorCookie = "TwoFactorCookie";
        public const string ExternalBearer = "ExternalBearer";
    }

    // EF6 System.Data.Entity.Validation.* shim (EF Core validates differently; these types are
    // never thrown now but keep the legacy try/catch + error-enumeration code compiling unchanged).
    public class DbValidationError
    {
        public string PropertyName { get; set; }
        public string ErrorMessage { get; set; }
    }
    public class DbEntityEntryShim
    {
        public object Entity { get; set; }
        public object State { get; set; }
    }
    public class DbEntityValidationResult
    {
        public DbEntityEntryShim Entry { get; set; } = new DbEntityEntryShim();
        public System.Collections.Generic.IEnumerable<DbValidationError> ValidationErrors { get; set; }
            = new System.Collections.Generic.List<DbValidationError>();
    }

    // EF6 System.Data.Entity.SqlServer.SqlFunctions shim (client-side equivalents so report queries
    // compile; correctness for any that ran server-side is revisited in the views/runtime wave).
    public static class SqlFunctions
    {
        public static string DateName(string datePartArg, System.DateTime? date)
        {
            if (date == null) return null;
            var d = date.Value;
            switch ((datePartArg ?? string.Empty).ToLowerInvariant())
            {
                case "weekday": case "dw": return d.DayOfWeek.ToString();
                case "month": case "mm": case "m": return d.ToString("MMMM");
                case "year": case "yy": case "yyyy": return d.Year.ToString();
                case "day": case "dd": case "d": return d.Day.ToString();
                case "hour": case "hh": return d.Hour.ToString();
                case "minute": case "mi": case "n": return d.Minute.ToString();
                default: return d.ToString();
            }
        }
        public static string StringConvert(decimal? number) => number?.ToString();
        public static string StringConvert(double? number) => number?.ToString();
        public static string StringConvert(decimal? number, int? length) => number?.ToString();
        public static string StringConvert(double? number, int? length) => number?.ToString();
    }
    public class DbEntityValidationException : System.Exception
    {
        public System.Collections.Generic.IEnumerable<DbEntityValidationResult> EntityValidationErrors { get; set; }
            = new System.Collections.Generic.List<DbEntityValidationResult>();
        public DbEntityValidationException() { }
        public DbEntityValidationException(string message) : base(message) { }
    }
}
