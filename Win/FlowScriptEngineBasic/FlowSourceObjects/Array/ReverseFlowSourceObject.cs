namespace FlowScriptEngineBasic.FlowSourceObjects.Array
{
    [ToolTipText("Array_Reverse_Summary")]
    public partial class ReverseFlowSourceObject : ArrayFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Array.Reverse"; }
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetArray();
            if (Array != null)
            {
                System.Array.Reverse(Array);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
