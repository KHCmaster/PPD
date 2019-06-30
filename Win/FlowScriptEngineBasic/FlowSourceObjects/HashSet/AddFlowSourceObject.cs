namespace FlowScriptEngineBasic.FlowSourceObjects.HashSet
{
    [ToolTipText("HashSet_Add_Summary")]
    public partial class AddFlowSourceObject : HashSetFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "HashSet.Add"; }
        }

        [ToolTipText("HashSet_Add_Item")]
        public object Item
        {
            private get;
            set;
        }

        [ToolTipText("HashSet_Add_IsNew")]
        public bool IsNew
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
                IsNew = HashSet.Add(Item);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
