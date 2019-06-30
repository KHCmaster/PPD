using FlowScriptDrawControl.Control;

namespace FlowScriptDrawControl.Command
{
    class AddScopeCommand : ScopeCommandBase
    {
        public AddScopeCommand(FlowAreaControl flowAreaControl, ScopeControl scopeControl, ScopeControl parentScopeControl) :
            base(flowAreaControl, scopeControl, parentScopeControl)
        {
        }

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
