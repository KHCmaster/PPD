using PPDEditorCommon.Dialog;

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Dialog.Setting
{
    [ToolTipText("Dialog_Setting_Add_Summary")]
    public partial class AddDoubleWithLimitFlowSourceObject : SettingFlowSourceObjectBase
    {
        public override string Name
        {
            get
            {
                return "PPDEditor.Dialog.Setting.Add";
            }
        }

        [ToolTipText("Dialog_Setting_Add_Value")]
        public double Value
        {
            private get;
            set;
        }

        [ToolTipText("Dialog_Setting_Add_MinValue")]
        public double MinValue
        {
            private get;
            set;
        }

        [ToolTipText("Dialog_Setting_Add_MaxValue")]
        public double MaxValue
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
                SetValue(nameof(MinValue));
                SetValue(nameof(MaxValue));
                Setting.AddSetting(new DoubleSetting(Key, Description, Value, MinValue, MaxValue));
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
