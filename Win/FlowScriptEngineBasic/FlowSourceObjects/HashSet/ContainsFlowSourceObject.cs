namespace FlowScriptEngineBasic.FlowSourceObjects.HashSet
{
    [ToolTipText("HashSet_Contains_Summary")]
    public partial class ContainsFlowSourceObject : HashSetFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "HashSet.Contains"; }
        }

        [ToolTipText("HashSet_Contains_Item")]
        public object Item
        {
            private get;
            set;
        }

        [ToolTipText("HashSet_Contains_Result")]
        public bool Result
        {
            get;
            private set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetHashSet();
            if (HashSet != null)
            {
                SetValue(nameof(Item));
                Result = HashSet.Contains(Item);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
