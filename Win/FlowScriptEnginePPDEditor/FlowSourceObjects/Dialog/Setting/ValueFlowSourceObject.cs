using PPDEditorCommon.Dialog.ViewModel;

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Dialog.Setting
{
    [ToolTipText("Dialog_Setting_Value_Summary")]
    public partial class ValueFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPDEditor.Dialog.Setting.Value"; }
        }

        [ToolTipText("Dialog_Setting_Value_Title")]
        public string Title
        {
            set;
            private get;
        }

        [ToolTipText("Dialog_Setting_Value_Value")]
        public SettingWindowViewModel Value
        {
            get;
            private set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetValue(nameof(Title));
            Value = new SettingWindowViewModel(Title);
            OnSuccess();
        }
    }
}
