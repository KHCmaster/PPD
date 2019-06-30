using FlowScriptEngine;

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.MessageBox
{
    [IgnoreTest]
    [ToolTipText("MessageBox_Show_Summary")]
    public partial class ShowWithButtonFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPDEditor.MessageBox.Show"; }
        }

        [ToolTipText("MessageBox_Show_Text")]
        public string Text
        {
            private get;
            set;
        }

        [ToolTipText("MessageBox_Show_Caption")]
        public string Caption
        {
            private get;
            set;
        }

        [ToolTipText("MessageBox_Show_Button")]
        public System.Windows.Forms.MessageBoxButtons Button
        {
            private get;
            set;
        }

        [ToolTipText("MessageBox_Show_Result")]
        public System.Windows.Forms.DialogResult Result
        {
            get;
            private set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetValue(nameof(Text));
            SetValue(nameof(Caption));
            SetValue(nameof(Button));
            Result = System.Windows.Forms.MessageBox.Show(Text, Caption, Button);
            OnSuccess();
        }
    }
}
