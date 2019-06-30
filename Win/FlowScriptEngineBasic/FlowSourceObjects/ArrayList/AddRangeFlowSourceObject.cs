using FlowScriptEngine;
using System.Collections.Generic;

namespace FlowScriptEngineBasic.FlowSourceObjects.ArrayList
{
    [ToolTipText("ArrayList_AddRange_Summary")]
    public partial class AddRangeFlowSourceObject : ArrayListFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "ArrayList.AddRange"; }
        }

        [ToolTipText("ArrayList_AddRange_Value")]
        public IEnumerable<object> Value
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetArrayList();
            if (ArrayList != null)
            {
                SetValue(nameof(Value));
                ArrayList.AddRange(Value);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
