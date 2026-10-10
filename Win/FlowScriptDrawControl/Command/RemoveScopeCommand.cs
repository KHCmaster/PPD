using FlowScriptDrawControl.Control;

namespace FlowScriptDrawControl.Command
{
    class RemoveScopeCommand(FlowAreaControl flowAreaControl, ScopeControl scopeControl, ScopeControl parentScopeControl) : ScopeCommandBase(flowAreaControl, scopeControl, parentScopeControl)
    {
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
