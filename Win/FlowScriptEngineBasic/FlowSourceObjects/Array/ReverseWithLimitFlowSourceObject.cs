namespace FlowScriptEngineBasic.FlowSourceObjects.Array
{
    [ToolTipText("Array_Reverse_Summary")]
    public partial class ReverseWithLimitFlowSourceObject : ArrayFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Array.Reverse"; }
        }

        [ToolTipText("Array_Reverse_Index")]
        public int Index
        {
            private get;
            set;
        }

        [ToolTipText("Array_Reverse_Length")]
        public int Length
        {
            private get;
            set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetArray();
            if (Array != null)
            {
                SetValue(nameof(Index));
                SetValue(nameof(Length));
                System.Array.Reverse(Array, Index, Length);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
