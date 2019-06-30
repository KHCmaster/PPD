namespace FlowScriptEngineBasic.FlowSourceObjects.Array
{
    [ToolTipText("Array_Clear_Summary")]
    public partial class ClearFlowSourceObject : ArrayFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Array.Clear"; }
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetArray();
            if (Array != null)
            {
                System.Array.Clear(Array, 0, Array.Length);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
