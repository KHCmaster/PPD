namespace FlowScriptEngineBasic.FlowSourceObjects.Array
{
    [ToolTipText("Array_Copy_Summary")]
    public partial class CopyWithLimitFlowSourceObject : ArrayFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Array.Copy"; }
        }

        [ToolTipText("Array_Copy_SourceIndex")]
        public int SourceIndex
        {
            private get;
            set;
        }

        [ToolTipText("Array_Copy_DestinationArray")]
        public object[] DestinationArray
        {
            private get;
            set;
        }

        [ToolTipText("Array_Copy_DestinationIndex")]
        public int DestinationIndex
        {
            private get;
            set;
        }

        [ToolTipText("Array_Copy_Length")]
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
                System.Array.Copy(Array, SourceIndex, DestinationArray, DestinationIndex, Length);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
