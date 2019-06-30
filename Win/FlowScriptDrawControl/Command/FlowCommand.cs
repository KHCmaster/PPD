using FlowScriptDrawControl.Control;
using FlowScriptDrawControl.Model;

namespace FlowScriptDrawControl.Command
{
    abstract class FlowCommand : CommandBase
    {
        protected FlowAreaControl areaControl;
        protected ArrowControl arrowControl;
        protected SourceItemControl src;
        protected SourceItemControl dest;

        protected FlowCommand(FlowAreaControl areaControl, ArrowControl arrowControl, SourceItemControl src,
            SourceItemControl dest)
        {
            this.areaControl = areaControl;
            this.arrowControl = arrowControl;
            this.src = src;
            this.dest = dest;
        }

        protected void RemoveArrow(ArrowControl arrowControl)
        {
            arrowControl.SrcItem.CurrentItem.RemoveOutConnection(arrowControl.DestItem.CurrentItem);
            arrowControl.DestItem.CurrentItem.InConnection = null;
            arrowControl.SrcItem = arrowControl.DestItem = null;
            areaControl.arrowCanvas.Children.Remove(arrowControl);
        }

        protected virtual void Connect(SourceItemControl srcItem, SourceItemControl destItem, ArrowControl arrowControl)
        {
            arrowControl.SrcItem = srcItem;
            if (srcItem != null)
            {
                srcItem.CurrentItem.AddOutConnection(new Connection { Target = destItem.CurrentItem });
            }

            arrowControl.SrcItem = srcItem;
            arrowControl.DestItem = destItem;
            destItem.CurrentItem.InConnection = new Connection { Target = srcItem.CurrentItem };
            if (!areaControl.arrowCanvas.Children.Contains(arrowControl))
            {
                areaControl.arrowCanvas.Children.Add(arrowControl);
            }
        }
    }
}
