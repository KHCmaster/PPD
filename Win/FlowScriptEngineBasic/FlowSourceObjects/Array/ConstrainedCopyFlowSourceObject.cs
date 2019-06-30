namespace FlowScriptEngineBasic.FlowSourceObjects.Array
{
    [ToolTipText("Array_ConstrainedCopy_Summary")]
    public partial class ConstrainedCopyFlowSourceObject : ArrayFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Array.ConstrainedCopy"; }
        }

        [ToolTipText("Array_ConstrainedCopy_SourceIndex")]
        public int SourceIndex
        {
            private get;
            set;
        }

        [ToolTipText("Array_ConstrainedCopy_DestinationArray")]
        public object[] DestinationArray
        {
            private get;
            set;
        }

        [ToolTipText("Array_ConstrainedCopy_DestinationIndex")]
        public int DestinationIndex
        {
            private get;
            set;
        }

        [ToolTipText("Array_ConstrainedCopy_Length")]
        public int Length
        {
            private get;
            set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetArray();
            SetValue(nameof(DestinationArray));
            if (Array != null && DestinationArray != null)
            {
                SetValue(nameof(SourceIndex));
                SetValue(nameof(DestinationIndex));
                SetValue(nameof(Length));
                System.Array.ConstrainedCopy(Array, SourceIndex, DestinationArray, DestinationIndex, Length);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
