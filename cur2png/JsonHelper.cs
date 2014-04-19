// developed by Rui Lopes (ruilopes.com). licensed under GPLv3.

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace cur2png
{
    public static class JsonHelper
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        public static string ToJson(this object o)
        {
            return JsonConvert.SerializeObject(o, JsonSerializerSettings);
        }
    }
}
