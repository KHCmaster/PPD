namespace FlowScriptEngineBasic.FlowSourceObjects.HashSet
{
    [ToolTipText("HashSet_Remove_Summary")]
    public partial class RemoveFlowSourceObject : HashSetFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "HashSet.Remove"; }
        }

        [ToolTipText("HashSet_Remove_Item")]
        public object Item
        {
            private get;
            set;
        }

        [ToolTipText("HashSet_Remove_Removed")]
        public bool Removed
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
                Removed = HashSet.Remove(Item);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
