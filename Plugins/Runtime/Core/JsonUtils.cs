// OmniscapeAPI/Plugins/Runtime/Core/JsonUtil.cs
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Omniscape.API.Core
{
    /// <summary>
    /// Centralized JSON helper using Newtonsoft.Json.
    /// Used for robust (de)serialization of Omniscape API responses.
    /// </summary>
    public static class JsonUtils
    {
        // Preconfigured serializer settings shared across the SDK
        private static readonly JsonSerializerSettings _settings = new()
        {
            NullValueHandling = NullValueHandling.Ignore,
            DateParseHandling = DateParseHandling.DateTime, // handles ISO8601/Z timestamps
            Formatting = Formatting.None,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        /// <summary>
        /// Deserialize a JSON string into a C# object of type T.
        /// </summary>
        public static T FromJson<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
                return default;
            return JsonConvert.DeserializeObject<T>(json, _settings);
        }

        /// <summary>
        /// Serialize a C# object into a JSON string.
        /// </summary>
        public static string ToJson(object o)
        {
            if (o == null)
                return "{}";
            return JsonConvert.SerializeObject(o, _settings);
        }
    }

    /// <summary>
    /// Backward-compat shim for older callers that referenced JsonUtil.
    /// </summary>
    public static class JsonUtil
    {
        public static T FromJson<T>(string json) => JsonUtils.FromJson<T>(json);
        public static string ToJson(object o) => JsonUtils.ToJson(o);
    }
}