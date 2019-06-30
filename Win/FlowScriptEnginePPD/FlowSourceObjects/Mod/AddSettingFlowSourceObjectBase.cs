using FlowScriptEngine;

namespace FlowScriptEnginePPD.FlowSourceObjects.Mod
{
    public abstract class AddSettingFlowSourceObjectBase : SettingFlowSourceObjectBase
    {
        [ToolTipText("Mod_AddSetting_Key")]
        public string Key
        {
            protected get;
            set;
        }

        [ToolTipText("Mod_AddSetting_DisplayName")]
        public string DisplayName
        {
            protected get;
            set;
        }

        [ToolTipText("Mod_AddSetting_Description")]
        public string Description
        {
            protected get;
            set;
        }

        [ToolTipText("Mod_AddSetting_In")]
        public virtual void In(FlowEventArgs e)
        {
            SetValue(nameof(Key));
            SetValue(nameof(DisplayName));
            SetValue(nameof(Description));
        }
    }
}
