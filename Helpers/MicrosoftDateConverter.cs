using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuickSoft.Web
{
    // MVC5/Newtonsoft serialized DateTime as the Microsoft JSON date format "/Date(ms)/". The legacy
    // client JS (custom.js convertToDate) parses exactly that: new Date(parseInt(data.substr(6))) skips
    // "/Date(" and reads the epoch-ms. System.Text.Json emits ISO ("2026-06-10T00:00:00") -> parseInt
    // fails -> every date shows 01-01-1970. These converters restore "/Date(ms)/" globally so all the
    // legacy date columns render correctly without touching the views.
    public class MicrosoftDateTimeConverter : JsonConverter<DateTime>
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var s = reader.GetString();
            if (string.IsNullOrEmpty(s)) return default;
            if (s.StartsWith("/Date("))
            {
                var inner = s.Substring(6);
                int end = inner.IndexOfAny(new[] { '+', '-', ')' });
                if (end > 0 && long.TryParse(inner.Substring(0, end), out var ms))
                    return Epoch.AddMilliseconds(ms).ToLocalTime();
            }
            return DateTime.TryParse(s, out var dt) ? dt : default;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            var ms = (long)(value.ToUniversalTime() - Epoch).TotalMilliseconds;
            writer.WriteStringValue("/Date(" + ms + ")/");
        }
    }

    public class MicrosoftNullableDateTimeConverter : JsonConverter<DateTime?>
    {
        private static readonly MicrosoftDateTimeConverter Inner = new MicrosoftDateTimeConverter();

        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null) return null;
            return Inner.Read(ref reader, typeof(DateTime), options);
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue) Inner.Write(writer, value.Value, options);
            else writer.WriteNullValue();
        }
    }
}
