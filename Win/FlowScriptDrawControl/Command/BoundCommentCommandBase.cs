using FlowScriptDrawControl.Control;

namespace FlowScriptDrawControl.Command
{
    abstract class BoundCommentCommandBase(FlowAreaControl areaControl, SourceControl sourceControl, BoundCommentControl commentControl) : CommandBase
    {
        private readonly FlowAreaControl areaControl = areaControl;
        private readonly SourceControl sourceControl = sourceControl;
        private readonly BoundCommentControl commentControl = commentControl;

        public BoundCommentControl CommentControl
        {
            get
            {
                return commentControl;
            }
        }

        protected void AddComment()
        {
            commentControl.Source = sourceControl;
            sourceControl.CurrentSource.Comment = commentControl.CurrentComment;
            areaControl.boundCommentCanvas.Children.Add(commentControl);
        }

        protected void RemoveComment()
        {
            areaControl.boundCommentCanvas.Children.Remove(commentControl);
            commentControl.Source = null;
            sourceControl.CurrentSource.Comment = null;
        }
    }
}
