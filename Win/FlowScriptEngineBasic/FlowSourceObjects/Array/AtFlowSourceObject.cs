using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Array
{
    [ToolTipText("Array_At_Summary")]
    public partial class AtFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Array.At"; }
        }

        [ToolTipText("Array_At_Index")]
        public int Index
        {
            private get;
            set;
        }

        [ToolTipText("Array_At_Array")]
        public object[] Array
        {
            protected get;
            set;
        }

        [ToolTipText("Array_At_Value")]
        public object Value
        {
            get
            {
                SetValue(nameof(Array));
                if (Array != null)
                {
                    SetValue(nameof(Index));
                    return Array[Index];
                }
                return null;
            }
        }
    }
}
