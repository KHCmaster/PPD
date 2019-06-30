using FlowScriptEngine;
using System.Collections.Generic;

namespace FlowScriptEngineBasic.FlowSourceObjects.ArrayList
{
    [ToolTipText("ArrayList_Repeat_Summary")]
    public partial class RepeatFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "ArrayList.Repeat"; }
        }

        [ToolTipText("ArrayList_Repeat_Value")]
        public List<object> Value
        {
            get;
            private set;
        }

        [ToolTipText("ArrayList_Repeat_Object")]
        public object Object
        {
            private get;
            set;
        }

        [ToolTipText("ArrayList_Repeat_Count")]
        public int Count
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Object));
            SetValue(nameof(Count));
            Value = new List<object>(Count);
            for (int i = 0; i < Count; i++)
            {
                Value.Add(Object);
            }
            OnSuccess();
        }
    }
}
