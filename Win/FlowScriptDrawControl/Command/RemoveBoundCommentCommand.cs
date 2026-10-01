using FlowScriptDrawControl.Control;

namespace FlowScriptDrawControl.Command
{
    class RemoveBoundCommentCommand(FlowAreaControl areaControl, SourceControl sourceControl, BoundCommentControl commentControl) : BoundCommentCommandBase(areaControl, sourceControl, commentControl)
    {
        public override void Execute()
        {
            RemoveComment();
        }

        public override void Undo()
        {
            AddComment();
        }
    }
}
