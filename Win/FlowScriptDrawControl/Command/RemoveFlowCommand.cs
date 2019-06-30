using FlowScriptDrawControl.Control;

namespace FlowScriptDrawControl.Command
{
    class RemoveFlowCommand : FlowCommand
    {
        public RemoveFlowCommand(FlowAreaControl areaControl, ArrowControl arrowControl)
            : base(areaControl, arrowControl, arrowControl.SrcItem, arrowControl.DestItem)
        {
        }

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
