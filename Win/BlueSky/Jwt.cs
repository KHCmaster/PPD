using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Text;

namespace BlueSky
{
    class Jwt
    {
        [JsonProperty("sub")]
        public string Sub
        {
            get;
            private set;
        }

        [JsonProperty("iat")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime Iat
        {
            get;
            private set;
        }


        [JsonProperty("exp")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime Exp
        {
            get;
            private set;
        }


        [JsonProperty("aud")]
        public string Aud
        {
            get;
            private set;
        }

        public static Jwt Parse(string s)
        {
            var splits = s.Split('.');
            if (splits.Length < 2)
            {
                throw new Exception("Invalid JWT. lack segments");
            }
            var claim = splits[1];
            if (claim.Length % 4 != 0)
            {
                claim += new string('=', 4 - claim.Length % 4);
            }
            var bytes = Convert.FromBase64String(claim);
            return JsonConvert.DeserializeObject<Jwt>(Encoding.UTF8.GetString(bytes));
        }
    }
}
