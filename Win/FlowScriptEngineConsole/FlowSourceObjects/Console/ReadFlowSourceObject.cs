using FlowScriptEngine;

namespace FlowScriptEngineConsole.FlowSourceObjects.Console
{
    [IgnoreTest]
    [ToolTipText("Console_Read_Summary", "Console_Read_Remark")]
    public partial class ReadFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Console.Read"; }
        }

        [ToolTipText("Console_Read_Value")]
        public int Value
        {
            get;
            private set;
        }

        [ToolTipText("Console_Read_In")]
        public void In(FlowEventArgs e)
        {
            Value = Manager.ConsoleReader.Read();
        }
    }
}
