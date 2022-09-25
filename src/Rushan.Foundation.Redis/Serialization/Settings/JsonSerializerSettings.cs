namespace Rushan.Foundation.Redis.Serialization.Settings
{
    internal static class JsonSerializerSettings
    {

#if NET5_0_OR_GREATER
        internal static System.Text.Json.JsonSerializerOptions GetTextJsonSerializerSettings()
        {
            var result = new System.Text.Json.JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.BasicLatin, System.Text.Unicode.UnicodeRanges.Cyrillic),
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            };

            result.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase, true));
            result.Converters.Add(new TimeSpanConverter());

            return result;
        }

#else
        internal static Newtonsoft.Json.JsonSerializerSettings GetJsonSerializerSettings()
        {
            Newtonsoft.Json.Serialization.CamelCaseNamingStrategy namingStrategy = new Newtonsoft.Json.Serialization.CamelCaseNamingStrategy(true, true);
            Newtonsoft.Json.Converters.StringEnumConverter stringEnumConverter = new Newtonsoft.Json.Converters.StringEnumConverter(namingStrategy, true);

            var jsonSerializerSettings = new Newtonsoft.Json.JsonSerializerSettings()
            {
                TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                Formatting = Newtonsoft.Json.Formatting.None,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver
                {
                    NamingStrategy = new Newtonsoft.Json.Serialization.CamelCaseNamingStrategy(true, true)
                }
            };

            jsonSerializerSettings.Converters.Add(stringEnumConverter);

            return jsonSerializerSettings;
        }
#endif
    }
}
