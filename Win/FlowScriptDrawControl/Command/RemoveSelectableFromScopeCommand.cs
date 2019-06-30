using FlowScriptDrawControl.Control;
using FlowScriptDrawControl.Model;

namespace FlowScriptDrawControl.Command
{
    class RemoveSelectableFromScopeCommand : EditScopeCommandBase
    {
        private Scope previousScope;

        public RemoveSelectableFromScopeCommand(FlowAreaControl flowAreaControl, ScopeControl scopeControl, PositionableControl positionableControl)
            : base(flowAreaControl, scopeControl, positionableControl)
        {
        }

        public override void Execute()
        {
            var scopeChild = positionableControl.CurrentPositionable as ScopeChild;
            previousScope = scopeChild.Scope;
            RemoveFromScope();
        }

        public override void Undo()
        {
            AddToScope(previousScope);
        }
    }
}
