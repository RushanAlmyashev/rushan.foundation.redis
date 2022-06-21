#if NET5_0_OR_GREATER
using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rushan.Foundation.Redis.Serialization.Settings
{
    internal class TimeSpanConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
        {
            if (TimeSpan.TryParse(reader.GetString(), CultureInfo.InvariantCulture, out TimeSpan result))
            {
                return result;
            }

            return default;
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, System.Text.Json.JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(format: null, CultureInfo.InvariantCulture));
        }
    }
}
#endif
