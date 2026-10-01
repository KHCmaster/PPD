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
            if (obj is IDictionary<object, object> dictionary)
            {
                var ret = new JObject();
                foreach (var p in dictionary)
                {
                    ret.Add(p.Key.ToString(), Convert(p.Value));
                }
                return ret;
            }
            else if (obj is IEnumerable<object> enumerable)
            {
                var ret = new JArray();
                foreach (var o in enumerable)
                {
                    ret.Add(Convert(o));
                }
                return ret;
            }
            else if (obj is bool v)
            {
                return new JValue(v);
            }
            else if (obj is char v1)
            {
                return new JValue(v1);
            }
            else if (obj is DateTime time)
            {
                return new JValue(time);
            }
            else if (obj is DateTimeOffset offset)
            {
                return new JValue(offset);
            }
            else if (obj is decimal v2)
            {
                return new JValue(v2);
            }
            else if (obj is double v3)
            {
                return new JValue(v3);
            }
            else if (obj is float v4)
            {
                return new JValue(v4);
            }
            else if (obj is Guid guid)
            {
                return new JValue(guid);
            }
            else if (obj is long v5)
            {
                return new JValue(v5);
            }
            else if (obj is int v6)
            {
                return new JValue(v6);
            }
            else if (obj is string v7)
            {
                return new JValue(v7);
            }
            else if (obj is TimeSpan span)
            {
                return new JValue(span);
            }
            else if (obj is ulong v8)
            {
                return new JValue(v8);
            }
            else if (obj is uint v9)
            {
                return new JValue(v9);
            }
            else if (obj is Uri uri)
            {
                return new JValue(uri);
            }
            else
            {
                return new JValue(obj.ToString());
            }
        }
    }
}
