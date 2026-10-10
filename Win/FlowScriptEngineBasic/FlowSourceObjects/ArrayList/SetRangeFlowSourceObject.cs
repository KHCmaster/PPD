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
                // 元は Value の Index 番目からを ArrayList の先頭に入れていた（Index が 0 のときだけ正しかった）
                for (int i = 0; i < Value.Count; i++)
                {
                    ArrayList[Index + i] = Value[i];
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
