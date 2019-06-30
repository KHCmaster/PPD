using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Repeat
{
    [IgnoreTest]
    [ToolTipText("Repeat_Continue_Summary")]
    public partial class ContinueFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Repeat.Continue"; }
        }

        [ToolTipText("Execute_In")]
        public void In(FlowEventArgs e)
        {
            ContinueLoop();
        }
    }
}
