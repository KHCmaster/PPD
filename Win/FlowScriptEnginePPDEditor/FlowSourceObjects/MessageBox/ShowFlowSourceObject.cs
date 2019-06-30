using FlowScriptEngine;

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.MessageBox
{
    [IgnoreTest]
    [ToolTipText("MessageBox_Show_Summary")]
    public partial class ShowFlowSourceObject : ExecutableFlowSourceObject
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

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetValue(nameof(Text));
            System.Windows.Forms.MessageBox.Show(Text);
            OnSuccess();
        }
    }
}
