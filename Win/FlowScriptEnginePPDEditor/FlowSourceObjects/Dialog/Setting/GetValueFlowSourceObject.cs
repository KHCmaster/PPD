using FlowScriptEngine;

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Dialog.Setting
{
    [ToolTipText("Dialog_Setting_GetValue_Summary")]
    public partial class GetValueFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPDEditor.Dialog.Setting.GetValue"; }
        }

        [ToolTipText("Dialog_Setting_GetValue_Setting")]
        public PPDEditorCommon.Dialog.ViewModel.SettingWindowViewModel Setting
        {
            private get;
            set;
        }

        [ToolTipText("Dialog_Setting_GetValue_Key")]
        public string Key
        {
            private get;
            set;
        }

        [ToolTipText("Dialog_Setting_GetValue_Value")]
        public object Value
        {
            get
            {
                SetValue(nameof(Setting));
                if (Setting != null)
                {
                    SetValue(nameof(Key));
                    return Setting[Key];
                }
                return null;
            }
        }
    }
}
