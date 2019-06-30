using FlowScriptEngine;

namespace FlowScriptEngineConsole.FlowSourceObjects.Console
{
    [ToolTipText("Console_WriteLine_Summary")]
    public partial class WriteLineFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Console.WriteLine"; }
        }

        [ToolTipText("Console_WriteLine_In")]
        public void In(FlowEventArgs e)
        {
            SetValue(nameof(Text));
            System.Console.WriteLine(Text);
        }

        [ToolTipText("Console_WriteLine_Text")]
        public string Text
        {
            private get;
            set;
        }
    }
}
