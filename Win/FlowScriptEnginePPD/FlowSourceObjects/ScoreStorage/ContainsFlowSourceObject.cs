namespace FlowScriptEnginePPD.FlowSourceObjects.ScoreStorage
{
    [ToolTipText("ScoreStorage_Contains_Summary")]
    public partial class ContainsFlowSourceObject : ScoreStorageFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.ScoreStorage.Contains"; }
        }

        [ToolTipText("ScoreStorage_Contains_Key")]
        public string Key
        {
            private get;
            set;
        }

        [ToolTipText("ScoreStorage_Contains_Value")]
        public bool Value
        {
            get
            {
                SetValue(nameof(Key));
                if (scoreStorage != null)
                {
                    return scoreStorage.ContainsKey(Key);
                }
                return false;
            }
        }
    }
}
