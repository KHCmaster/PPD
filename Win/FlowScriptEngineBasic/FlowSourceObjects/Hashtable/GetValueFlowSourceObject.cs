namespace FlowScriptEngineBasic.FlowSourceObjects.Hashtable
{
    [ToolTipText("Hashtable_GetValue_Summary")]
    public partial class GetValueFlowSourceObject : HashtableFlowSourceBase
    {
        public override string Name
        {
            get { return "Hashtable.GetValue"; }
        }

        [ToolTipText("Hashtable_GetValue_Key")]
        public object Key
        {
            private get;
            set;
        }

        [ToolTipText("Hashtable_GetValue_Value")]
        public object Value
        {
            get
            {
                SetHashtable();
                if (Hashtable != null)
                {
                    SetValue(nameof(Key));
                    return Hashtable[Key];
                }
                return null;
            }
        }
    }
}
