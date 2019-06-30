using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Repeat
{
    [IgnoreTest]
    [ToolTipText("Repeat_Break_Summary")]
    public partial class BreakFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Repeat.Break"; }
        }

        [ToolTipText("Execute_In")]
        public void In(FlowEventArgs e)
        {
            BreakLoop();
        }
    }
}
