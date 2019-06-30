using FlowScriptDrawControl.Control;

namespace FlowScriptDrawControl.Command
{
    class AddBoundCommentBommand : BoundCommentCommandBase
    {
        public AddBoundCommentBommand(FlowAreaControl areaControl, SourceControl sourceControl)
            : base(areaControl, sourceControl,
            new BoundCommentControl { DataContext = new Model.Comment { Text = "Comment" } })
        {
        }

        public override void Execute()
        {
            AddComment();
        }

        public override void Undo()
        {
            RemoveComment();
        }
    }
}
