namespace FlowScriptEnginePPD.FlowSourceObjects.Mod
{
    [ToolTipText("Mod_AddSetting_Summary")]
    public partial class AddFloatSettingFlowSourceObject : AddSettingFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Mod.AddSetting"; }
        }

        [ToolTipText("Mod_AddSetting_Default")]
        public float Default
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
                modSettingManager.AddFloatSetting(Key, DisplayName, Description, Default);
            }
        }
    }
}
