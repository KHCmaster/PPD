using FlowScriptDrawControl.Control;
using System.Windows;

namespace FlowScriptDrawControl.Command
{
    class AddCommentCommand : CommandBase
    {
        private FlowAreaControl areaControl;

        public CommentControl CommentControl
        {
            get;
            private set;
        }

        public AddCommentCommand(FlowAreaControl areaControl, double posX, double posY)
        {
            this.areaControl = areaControl;
            CommentControl = areaControl.CreateComment();
            CommentControl.CurrentComment.Position = new Point(posX, posY);
        }

        public override void Execute()
        {
            areaControl.controlCanvas.Children.Add(CommentControl);
        }

        public override void Undo()
        {
            areaControl.controlCanvas.Children.Remove(CommentControl);
        }
    }
}
