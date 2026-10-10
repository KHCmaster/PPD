using FlowScriptDrawControl.Control;

namespace FlowScriptDrawControl.Command
{
    class RemoveFlowCommand(FlowAreaControl areaControl, ArrowControl arrowControl) : FlowCommand(areaControl, arrowControl, arrowControl.SrcItem, arrowControl.DestItem)
    {
        public override void Execute()
        {
            RemoveArrow(arrowControl);
        }

        public override void Undo()
        {
            Connect(src, dest, arrowControl);
        }
    }
}
