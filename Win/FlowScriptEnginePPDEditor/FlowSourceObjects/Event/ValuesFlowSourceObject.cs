using FlowScriptEngine;
using PPDEditorCommon;
using System.Collections.Generic;

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Event
{
    [ToolTipText("Event_Values_Summary")]
    public partial class ValuesFlowSourceObject : FlowSourceObjectBase
    {
        KeyValuePair<float, EventData>[] events;

        public override string Name
        {
            get { return "PPDEditor.Event.Values"; }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            if (Manager.Items.ContainsKey("Events"))
            {
                events = Manager.Items["Events"] as KeyValuePair<float, EventData>[];
            }
        }

        [ToolTipText("Event_Values_Values")]
        public object[] Values
        {
            get
            {
                if (events != null)
                {
                    var ret = new object[events.Length];
                    for (int i = 0; i < ret.Length; i++)
                    {
                        ret[i] = new KeyValuePair<object, object>(events[i].Key, events[i].Value);
                    }
                    return ret;
                }
                return null;
            }
        }
    }
}
