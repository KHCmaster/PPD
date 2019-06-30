using FlowScriptDrawControl.Control;

namespace FlowScriptDrawControl.Command
{
    class AddFlowCommand : FlowCommand
    {
        private ArrowControl removedArrow;
        private SourceItemControl removedSrcItem;

        public AddFlowCommand(FlowAreaControl areaControl, ArrowControl arrowControl, SourceItemControl src,
            SourceItemControl dest)
            : base(areaControl, arrowControl, src, dest)
        {
        }

        public override void Execute()
        {
            Connect(src, dest, arrowControl);
        }

        public override void Undo()
        {
            if (removedSrcItem != null && removedArrow != null)
            {
                Connect(removedSrcItem, dest, removedArrow);
            }
            else
            {
                RemoveArrow(arrowControl);
            }
        }

        protected override void Connect(SourceItemControl srcItem, SourceItemControl destItem, ArrowControl arrowControl)
        {
            if (destItem != null && destItem.CurrentItem.InConnection != null)
            {
                removedArrow = areaControl.GetArrowControl(destItem);
                removedSrcItem = removedArrow.SrcItem;
                RemoveArrow(removedArrow);
            }
            base.Connect(srcItem, destItem, arrowControl);
        }
    }
}
