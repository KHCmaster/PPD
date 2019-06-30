using FlowScriptEngine;

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.MessageBox
{
    [IgnoreTest]
    [ToolTipText("MessageBox_Show_Summary")]
    public partial class ShowWithCaptionFlowSourceObject : ExecutableFlowSourceObject
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

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetValue(nameof(Text));
            SetValue(nameof(Caption));
            System.Windows.Forms.MessageBox.Show(Text, Caption);
            OnSuccess();
        }
    }
}
