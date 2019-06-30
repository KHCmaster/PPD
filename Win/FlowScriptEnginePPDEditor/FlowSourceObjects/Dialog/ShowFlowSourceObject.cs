using FlowScriptEngine;
using PPDEditorCommon.Dialog;
using PPDEditorCommon.Dialog.ViewModel;
using System.Windows.Forms;

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Dialog
{
    [IgnoreTest]
    [ToolTipText("Dialog_Show_Summary")]
    public partial class ShowFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPDEditor.Dialog.Show"; }
        }

        [ToolTipText("Dialog_Show_Setting")]
        public SettingWindowViewModel Setting
        {
            private get;
            set;
        }

        [ToolTipText("Dialog_Show_Result")]
        public DialogResult Result
        {
            get;
            private set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetValue(nameof(Setting));
            var window = new SettingWindow();
            Setting.Initialize();
            window.DataContext = Setting;
            Result = window.ShowDialog() == true ? DialogResult.OK : DialogResult.Cancel;
            OnSuccess();
        }
    }
}
