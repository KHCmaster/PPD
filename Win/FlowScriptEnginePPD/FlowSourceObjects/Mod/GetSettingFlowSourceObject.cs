namespace FlowScriptEnginePPD.FlowSourceObjects.Mod
{
    [ToolTipText("Mod_GetSetting_Summary")]
    public partial class GetSettingFlowSourceObject : SettingFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Mod.GetSetting"; }
        }

        [ToolTipText("Mod_GetSetting_Key")]
        public string Key
        {
            private get;
            set;
        }

        [ToolTipText("Mod_GetSetting_Value")]
        public object Value
        {
            get
            {
                SetValue(nameof(Key));
                if (modSettingManager != null)
                {
                    return modSettingManager[Key];
                }
                return null;
            }
        }
    }
}
