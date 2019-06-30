namespace FlowScriptEnginePPD.FlowSourceObjects.Mod
{
    [ToolTipText("Mod_AddSetting_Summary")]
    public partial class AddStringSettingFlowSourceObject : AddSettingFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Mod.AddSetting"; }
        }

        [ToolTipText("Mod_AddSetting_Default")]
        public string Default
        {
            private get;
            set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            base.In(e);
            SetValue(nameof(Default));
            if (modSettingManager != null)
            {
                modSettingManager.AddStringSetting(Key, DisplayName, Description, Default);
            }
        }
    }
}
