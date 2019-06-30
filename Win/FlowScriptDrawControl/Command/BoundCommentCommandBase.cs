using FlowScriptDrawControl.Control;

namespace FlowScriptDrawControl.Command
{
    abstract class BoundCommentCommandBase : CommandBase
    {
        private FlowAreaControl areaControl;
        private SourceControl sourceControl;
        private BoundCommentControl commentControl;

        public BoundCommentControl CommentControl
        {
            get
            {
                return commentControl;
            }
        }

        protected BoundCommentCommandBase(FlowAreaControl areaControl, SourceControl sourceControl, BoundCommentControl commentControl)
        {
            this.areaControl = areaControl;
            this.sourceControl = sourceControl;
            this.commentControl = commentControl;
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
