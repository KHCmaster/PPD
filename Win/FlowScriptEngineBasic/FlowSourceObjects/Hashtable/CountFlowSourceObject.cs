namespace FlowScriptEngineBasic.FlowSourceObjects.Hashtable
{
    [ToolTipText("Hashtable_Count_Summary")]
    public partial class CountFlowSourceObject : HashtableFlowSourceBase
    {
        public override string Name
        {
            get { return "Hashtable.Count"; }
        }

        [ToolTipText("Hashtable_Count_Value")]
        public int Value
        {
            get
            {
                SetHashtable();
                if (Hashtable != null)
                {
                    return Hashtable.Count;
                }
                return 0;
            }
        }
    }
}
