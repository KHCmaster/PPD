namespace FlowScriptEnginePPD.FlowSourceObjects.Mod
{
    [ToolTipText("Mod_AddSetting_Summary")]
    public partial class AddStringSettingWithMaxLengthFlowSourceObject : AddSettingFlowSourceObjectBase
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

        [ToolTipText("Mod_AddSetting_MaxLength")]
        public int MaxLength
        {
            private get;
            set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            base.In(e);
            SetValue(nameof(Default));
            SetValue(nameof(MaxLength));
            if (modSettingManager != null)
            {
                modSettingManager.AddStringSetting(Key, DisplayName, Description, Default, MaxLength);
            }
        }
    }
}
