namespace FlowScriptEngineBasic.FlowSourceObjects.Array
{
    [ToolTipText("Array_Copy_Summary")]
    public partial class CopyFlowSourceObject : ArrayFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Array.Copy"; }
        }

        [ToolTipText("Array_Copy_DestinationArray")]
        public object[] DestinationArray
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
                SetValue(nameof(Length));
                System.Array.Copy(Array, DestinationArray, Length);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
