using FlowScriptEngine;

namespace FlowScriptEngineConsole.FlowSourceObjects.Console
{
    [ToolTipText("Console_Clear_Summary")]
    public partial class ClearFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Console.Clear"; }
        }

        [ToolTipText("Console_Clear_In")]
        public void In(FlowEventArgs e)
        {
#if DEBUG
#else
            System.Console.Clear();
#endif
        }
    }
}
