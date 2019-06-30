using PPDEditorCommon.Dialog;

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Dialog.Setting
{
    [ToolTipText("Dialog_Setting_Add_Summary")]
    public partial class AddStringWithLimitFlowSourceObject : SettingFlowSourceObjectBase
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

        [ToolTipText("Dialog_Setting_Add_MaxLength")]
        public int MaxLength
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
                SetValue(nameof(MaxLength));
                Setting.AddSetting(new StringSetting(Key, Description, Value, MaxLength));
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
