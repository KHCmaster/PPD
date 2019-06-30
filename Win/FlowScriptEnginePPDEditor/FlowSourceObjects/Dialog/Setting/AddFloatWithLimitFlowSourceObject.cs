using PPDEditorCommon.Dialog;

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Dialog.Setting
{
    [ToolTipText("Dialog_Setting_Add_Summary")]
    public partial class AddFloatWithLimitFlowSourceObject : SettingFlowSourceObjectBase
    {
        public override string Name
        {
            get
            {
                return "PPDEditor.Dialog.Setting.Add";
            }
        }

        [ToolTipText("Dialog_Setting_Add_Value")]
        public float Value
        {
            private get;
            set;
        }

        [ToolTipText("Dialog_Setting_Add_MinValue")]
        public float MinValue
        {
            private get;
            set;
        }

        [ToolTipText("Dialog_Setting_Add_MaxValue")]
        public float MaxValue
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
                Setting.AddSetting(new FloatSetting(Key, Description, Value, MinValue, MaxValue));
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
