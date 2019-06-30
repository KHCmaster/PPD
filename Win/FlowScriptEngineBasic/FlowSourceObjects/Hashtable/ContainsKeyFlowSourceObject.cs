namespace FlowScriptEngineBasic.FlowSourceObjects.Hashtable
{
    [ToolTipText("Hashtable_ContainsKey_Summary")]
    public partial class ContainsKeyFlowSourceObject : HashtableFlowSourceBase
    {
        public override string Name
        {
            get { return "Hashtable.ContainsKey"; }
        }

        [ToolTipText("Hashtable_ContainsKey_Key")]
        public object Key
        {
            private get;
            set;
        }

        [ToolTipText("Hashtable_ContainsKey_Value")]
        public bool Value
        {
            get;
            private set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetHashtable();
            if (Hashtable != null)
            {
                SetValue(nameof(Key));
                Value = Hashtable.ContainsKey(Key);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
