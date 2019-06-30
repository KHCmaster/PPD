using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace FlowScriptEngineData.FlowSourceObjects.Json
{
    [ToolTipText("Json_Dump_Summary")]
    public partial class DumpFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "Json.Dump"; }
        }

        [ToolTipText("Json_Dump_Data")]
        public object Data
        {
            private get;
            set;
        }

        [ToolTipText("Json_Dump_Json")]
        public string Json
        {
            get;
            private set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            try
            {
                SetValue(nameof(Data));
                Json = JsonConvert.SerializeObject(Convert(Data));
                OnSuccess();
            }
            catch
            {
                OnFailed();
            }
        }

        public JToken Convert(object obj)
        {
            if (obj is IDictionary<object, object>)
            {
                var ret = new JObject();
                foreach (var p in (IDictionary<object, object>)obj)
                {
                    ret.Add(p.Key.ToString(), Convert(p.Value));
                }
                return ret;
            }
            else if (obj is IEnumerable<object>)
            {
                var ret = new JArray();
                foreach (var o in (IEnumerable<object>)obj)
                {
                    ret.Add(Convert(o));
                }
                return ret;
            }
            else if (obj is bool)
            {
                return new JValue((bool)obj);
            }
            else if (obj is char)
            {
                return new JValue((char)obj);
            }
            else if (obj is DateTime)
            {
                return new JValue((DateTime)obj);
            }
            else if (obj is DateTimeOffset)
            {
                return new JValue((DateTimeOffset)obj);
            }
            else if (obj is decimal)
            {
                return new JValue((decimal)obj);
            }
            else if (obj is double)
            {
                return new JValue((double)obj);
            }
            else if (obj is float)
            {
                return new JValue((float)obj);
            }
            else if (obj is Guid)
            {
                return new JValue((Guid)obj);
            }
            else if (obj is long)
            {
                return new JValue((long)obj);
            }
            else if (obj is int)
            {
                return new JValue((int)obj);
            }
            else if (obj is string)
            {
                return new JValue((string)obj);
            }
            else if (obj is TimeSpan)
            {
                return new JValue((TimeSpan)obj);
            }
            else if (obj is ulong)
            {
                return new JValue((ulong)obj);
            }
            else if (obj is uint)
            {
                return new JValue((uint)obj);
            }
            else if (obj is Uri)
            {
                return new JValue((Uri)obj);
            }
            else
            {
                return new JValue(obj.ToString());
            }
        }
    }
}
