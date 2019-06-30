namespace FlowScriptEnginePPD.FlowSourceObjects.Mod
{
    [ToolTipText("Mod_AddSetting_Summary")]
    public partial class AddInt32SettingWithLimitFlowSourceObject : AddSettingFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Mod.AddSetting"; }
        }

        [ToolTipText("Mod_AddSetting_Default")]
        public int Default
        {
            private get;
            set;
        }

        [ToolTipText("Mod_AddSetting_Min")]
        public int Min
        {
            private get;
            set;
        }

        [ToolTipText("Mod_AddSetting_Max")]
        public int Max
        {
            private get;
            set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            base.In(e);
            SetValue(nameof(Default));
            SetValue(nameof(Min));
            SetValue(nameof(Max));
            if (modSettingManager != null)
            {
                modSettingManager.AddInt32Setting(Key, DisplayName, Description, Default, Min, Max);
            }
        }
    }
}
