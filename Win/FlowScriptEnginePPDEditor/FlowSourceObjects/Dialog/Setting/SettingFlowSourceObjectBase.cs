using PPDEditorCommon.Dialog.ViewModel;

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Dialog.Setting
{
    public abstract class SettingFlowSourceObjectBase : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPDEditor.Dialog.Setting.Add"; }
        }

        [ToolTipText("Dialog_Setting_Add_Setting")]
        public SettingWindowViewModel Setting
        {
            protected get;
            set;
        }

        [ToolTipText("Dialog_Setting_Add_Key")]
        public string Key
        {
            protected get;
            set;
        }

        [ToolTipText("Dialog_Setting_Add_Description")]
        public string Description
        {
            protected get;
            set;
        }

        protected void SetSetting()
        {
            SetValue(nameof(Setting));
        }

        protected void SetKey()
        {
            SetValue(nameof(Key));
        }

        protected void SetDescription()
        {
            SetValue(nameof(Description));
        }
    }
}
