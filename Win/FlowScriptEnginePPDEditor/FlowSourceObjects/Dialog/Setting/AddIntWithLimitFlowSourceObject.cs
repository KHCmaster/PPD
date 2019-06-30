using PPDEditorCommon.Dialog;

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Dialog.Setting
{
    [ToolTipText("Dialog_Setting_Add_Summary")]
    public partial class AddIntWithLimitFlowSourceObject : SettingFlowSourceObjectBase
    {
        public override string Name
        {
            get
            {
                return "PPDEditor.Dialog.Setting.Add";
            }
        }

        [ToolTipText("Dialog_Setting_Add_Value")]
        public int Value
        {
            private get;
            set;
        }

        [ToolTipText("Dialog_Setting_Add_MinValue")]
        public int MinValue
        {
            private get;
            set;
        }

        [ToolTipText("Dialog_Setting_Add_MaxValue")]
        public int MaxValue
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
                Setting.AddSetting(new IntSetting(Key, Description, Value, MinValue, MaxValue));
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
