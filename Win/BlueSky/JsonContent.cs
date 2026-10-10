using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace BlueSky
{
    internal class JsonContent<T>(T value, JsonSerializerSettings serializerSettings) : StringContent(JsonConvert.SerializeObject(value, serializerSettings), Encoding.UTF8, "application/json")
    {
    }
}
