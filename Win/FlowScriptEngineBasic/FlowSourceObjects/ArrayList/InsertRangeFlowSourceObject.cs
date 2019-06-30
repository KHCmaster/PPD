using FlowScriptEngine;
using System.Collections.Generic;

namespace FlowScriptEngineBasic.FlowSourceObjects.ArrayList
{
    [ToolTipText("ArrayList_InsertRange_Summary")]
    public partial class InsertRangeFlowSourceObject : ArrayListFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "ArrayList.InsertRange"; }
        }

        [ToolTipText("ArrayList_InsertRange_Value")]
        public IEnumerable<object> Value
        {
            private get;
            set;
        }

        [ToolTipText("ArrayList_InsertRange_Index")]
        public int Index
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
                SetValue(nameof(Index));
                ArrayList.InsertRange(Index, Value);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
