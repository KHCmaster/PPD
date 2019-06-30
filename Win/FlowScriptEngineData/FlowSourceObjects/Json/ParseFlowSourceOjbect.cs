using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace FlowScriptEngineData.FlowSourceObjects.Json
{
    [ToolTipText("Json_Parse_Summary")]
    public partial class ParseFlowSourceOjbect : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "Json.Parse"; }
        }

        [ToolTipText("Json_Parse_Json")]
        public string Json
        {
            private get;
            set;
        }

        [ToolTipText("Json_Parse_Data")]
        public object Data
        {
            get;
            private set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            try
            {
                SetValue(nameof(Json));
                Data = Convert(JsonConvert.DeserializeObject(Json));
                OnSuccess();
            }
            catch
            {
                OnFailed();
            }
        }

        private object Convert(object obj)
        {
            if (obj is JObject)
            {
                var ret = new Dictionary<object, object>();
                foreach (var p in (JObject)obj)
                {
                    ret[p.Key] = Convert(p.Value);
                }
                return ret;
            }
            else if (obj is JArray)
            {
                var ret = new List<object>();
                foreach (var o in (JArray)obj)
                {
                    ret.Add(Convert(o));
                }
                return ret;
            }
            else if (obj is JValue)
            {
                var v = (JValue)obj;
                if (v.Value is long)
                {
                    var vv = (long)v.Value;
                    if (int.MinValue <= vv && vv <= int.MaxValue)
                    {
                        return (int)vv;
                    }
                }
                else if (v.Value is ulong)
                {
                    var vv = (ulong)v.Value;
                    if (vv <= int.MaxValue)
                    {
                        return (int)vv;
                    }
                }
                else if (v.Value is double)
                {
                    var vv = (double)v.Value;
                    if ((float)vv == vv)
                    {
                        return (float)vv;
                    }
                }
                return v.Value;
            }
            return obj;
        }
    }
}
