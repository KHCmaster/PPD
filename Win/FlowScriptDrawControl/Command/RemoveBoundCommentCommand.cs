using FlowScriptDrawControl.Control;

namespace FlowScriptDrawControl.Command
{
    class RemoveBoundCommentCommand : BoundCommentCommandBase
    {
        public RemoveBoundCommentCommand(FlowAreaControl areaControl, SourceControl sourceControl, BoundCommentControl commentControl) :
            base(areaControl, sourceControl, commentControl)
        {
        }

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
