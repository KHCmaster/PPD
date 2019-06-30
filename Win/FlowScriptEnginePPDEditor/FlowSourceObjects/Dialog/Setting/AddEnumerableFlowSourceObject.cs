using PPDEditorCommon.Dialog;
using System.Collections.Generic;

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Dialog.Setting
{
    [ToolTipText("Dialog_Setting_Add_Summary")]
    public partial class AddEnumerableFlowSourceObject : SettingFlowSourceObjectBase
    {
        public override string Name
        {
            get
            {
                return "PPDEditor.Dialog.Setting.Add";
            }
        }

        [ToolTipText("Dialog_Setting_Add_Value")]
        public object Value
        {
            private get;
            set;
        }

        [ToolTipText("Dialog_Setting_Add_Values")]
        public IEnumerable<object> Values
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
                SetValue(nameof(Values));
                Setting.AddSetting(new EnumerableSetting(Key, Description, Values, Value));
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
