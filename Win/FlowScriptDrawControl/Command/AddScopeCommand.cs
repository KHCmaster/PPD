using FlowScriptDrawControl.Control;

namespace FlowScriptDrawControl.Command
{
    class AddScopeCommand(FlowAreaControl flowAreaControl, ScopeControl scopeControl, ScopeControl parentScopeControl) : ScopeCommandBase(flowAreaControl, scopeControl, parentScopeControl)
    {
        public override void Execute()
        {
            AddScope();
        }

        public override void Undo()
        {
            RemoveScope();
        }
    }
}
