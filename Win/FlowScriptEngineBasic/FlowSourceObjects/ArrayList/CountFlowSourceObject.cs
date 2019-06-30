namespace FlowScriptEngineBasic.FlowSourceObjects.ArrayList
{
    [ToolTipText("ArrayList_Count_Summary")]
    public partial class CountFlowSourceObject : ArrayListFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "ArrayList.Count"; }
        }

        [ToolTipText("ArrayList_Count_Value")]
        public int Value
        {
            get
            {
                SetArrayList();
                if (ArrayList != null)
                {
                    return ArrayList.Count;
                }
                return 0;
            }
        }
    }
}
