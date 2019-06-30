using FlowScriptEngine;
using System.Collections.Generic;

namespace FlowScriptEngineBasic.FlowSourceObjects.ArrayList
{
    [ToolTipText("ArrayList_GetRange_Summary")]
    public partial class GetRangeFlowSourceObject : ArrayListFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "ArrayList.GetRange"; }
        }

        [ToolTipText("ArrayList_GetRange_Index")]
        public int Index
        {
            private get;
            set;
        }

        [ToolTipText("ArrayList_GetRange_Count")]
        public int Count
        {
            private get;
            set;
        }

        [ToolTipText("ArrayList_GetRange_Value")]
        public List<object> Value
        {
            get;
            private set;
        }

        public override void In(FlowEventArgs e)
        {
            SetArrayList();
            if (ArrayList != null)
            {
                SetValue(nameof(Index));
                SetValue(nameof(Count));
                Value = ArrayList.GetRange(Index, Count);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
