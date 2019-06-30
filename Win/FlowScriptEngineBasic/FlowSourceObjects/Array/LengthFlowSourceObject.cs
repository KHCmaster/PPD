using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Array
{
    [ToolTipText("Array_Length_Summary")]
    public partial class LengthFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Array.Length"; }
        }

        [ToolTipText("Array_Length_Array")]
        public object[] Array
        {
            protected get;
            set;
        }

        [ToolTipText("Array_Length_Value")]
        public int Value
        {
            get
            {
                SetValue(nameof(Array));
                if (Array != null)
                {
                    return Array.Length;
                }
                return 0;
            }
        }
    }
}
