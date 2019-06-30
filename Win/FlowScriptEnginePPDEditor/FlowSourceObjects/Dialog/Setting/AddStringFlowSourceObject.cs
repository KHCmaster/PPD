using PPDEditorCommon.Dialog;

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Dialog.Setting
{
    [ToolTipText("Dialog_Setting_Add_Summary")]
    public partial class AddStringFlowSourceObject : SettingFlowSourceObjectBase
    {
        public override string Name
        {
            get
            {
                return "PPDEditor.Dialog.Setting.Add";
            }
        }

        [ToolTipText("Dialog_Setting_Add_Value")]
        public string Value
        {
            private get;
            set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetSetting();
            if (Setting != null)
            {
                SetKey();
                SetDescription();
                SetValue(nameof(Value));
                Setting.AddSetting(new StringSetting(Key, Description, Value));
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
