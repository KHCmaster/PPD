namespace FlowScriptEnginePPD.FlowSourceObjects.ScoreStorage
{
    [ToolTipText("ScoreStorage_Get_Summary")]
    public partial class GetFlowSourceObject : ScoreStorageFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.ScoreStorage.Get"; }
        }

        [ToolTipText("ScoreStorage_Get_Key")]
        public string Key
        {
            private get;
            set;
        }

        [ToolTipText("ScoreStorage_Get_Value")]
        public string Value
        {
            get
            {
                SetValue(nameof(Key));
                if (scoreStorage != null)
                {
                    return scoreStorage[Key];
                }
                return null;
            }
        }
    }
}
