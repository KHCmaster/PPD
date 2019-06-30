using FlowScriptEngine;
using System.Collections.Generic;

namespace FlowScriptEngineBasic.FlowSourceObjects.ArrayList
{
    [ToolTipText("ArrayList_SetRange_Summary")]
    public partial class SetRangeFlowSourceObject : ArrayListFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "ArrayList.SetRange"; }
        }

        [ToolTipText("ArrayList_SetRange_Value")]
        public List<object> Value
        {
            private get;
            set;
        }

        [ToolTipText("ArrayList_SetRange_Index")]
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
                for (int i = Index; i < Value.Count; i++)
                {
                    ArrayList[i - Index] = Value[i];
                }
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
