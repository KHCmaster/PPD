using FlowScriptEngine;

namespace FlowScriptEngineConsole.FlowSourceObjects.Console
{
    [IgnoreTest]
    [ToolTipText("Console_ReadLine_Summary", "Console_ReadLine_Remark")]
    public partial class ReadLineFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Console.ReadLine"; }
        }

        [ToolTipText("Console_ReadLine_Value")]
        public string Value
        {
            get;
            private set;
        }

        [ToolTipText("Console_ReadLine_In")]
        public void In(FlowEventArgs e)
        {
            Value = Manager.ConsoleReader.ReadLine();
        }
    }
}
