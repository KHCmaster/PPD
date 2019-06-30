namespace FlowScriptEnginePPD.FlowSourceObjects.Mod
{
    [ToolTipText("Mod_AddSetting_Summary")]
    public partial class AddDoubleSettingWithLimitFlowSourceObject : AddSettingFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Mod.AddSetting"; }
        }

        [ToolTipText("Mod_AddSetting_Default")]
        public double Default
        {
            private get;
            set;
        }

        [ToolTipText("Mod_AddSetting_Min")]
        public double Min
        {
            private get;
            set;
        }

        [ToolTipText("Mod_AddSetting_Max")]
        public double Max
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
                modSettingManager.AddDoubleSetting(Key, DisplayName, Description, Default, Min, Max);
            }
        }
    }
}
