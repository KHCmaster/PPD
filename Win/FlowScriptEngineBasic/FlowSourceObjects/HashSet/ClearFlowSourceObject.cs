namespace FlowScriptEngineBasic.FlowSourceObjects.HashSet
{
    [ToolTipText("HashSet_Clear_Summary")]
    public partial class ClearFlowSourceObject : HashSetFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "HashSet.Clear"; }
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetHashSet();
            if (HashSet != null)
            {
                HashSet.Clear();
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
