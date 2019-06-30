using FlowScriptDrawControl.Control;

namespace FlowScriptDrawControl.Command
{
    class RemoveScopeCommand : ScopeCommandBase
    {
        public RemoveScopeCommand(FlowAreaControl flowAreaControl, ScopeControl scopeControl, ScopeControl parentScopeControl) :
            base(flowAreaControl, scopeControl, parentScopeControl)
        {

        }

        public override void Execute()
        {
            RemoveScope();
        }

        public override void Undo()
        {
            AddScope();
        }
    }
}
