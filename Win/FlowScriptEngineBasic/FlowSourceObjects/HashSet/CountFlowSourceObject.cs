namespace FlowScriptEngineBasic.FlowSourceObjects.HashSet
{
    [ToolTipText("HashSet_Count_Summary")]
    public partial class CountFlowSourceObject : HashSetFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "HashSet.Count"; }
        }

        [ToolTipText("HashSet_Count_Value")]
        public int Value
        {
            get
            {
                SetHashSet();
                if (HashSet != null)
                {
                    return HashSet.Count;
                }
                return 0;
            }
        }
    }
}
