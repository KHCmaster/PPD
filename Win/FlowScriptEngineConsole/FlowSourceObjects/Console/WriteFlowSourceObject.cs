using FlowScriptEngine;

namespace FlowScriptEngineConsole.FlowSourceObjects.Console
{
    [ToolTipText("Console_Write_Summary")]
    public partial class WriteFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Console.Write"; }
        }

        [ToolTipText("Console_Write_Text")]
        public string Text
        {
            private get;
            set;
        }

        [ToolTipText("Console_Write_In")]
        public void In(FlowEventArgs e)
        {
            SetValue(nameof(Text));
            System.Console.Write(Text);
        }
    }
}
