using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace BlueSky
{
    internal class JsonContent<T> : StringContent
    {
        public JsonContent(T value, JsonSerializerSettings serializerSettings) : base(JsonConvert.SerializeObject(value, serializerSettings), Encoding.UTF8, "application/json")
        {
        }
    }
}
