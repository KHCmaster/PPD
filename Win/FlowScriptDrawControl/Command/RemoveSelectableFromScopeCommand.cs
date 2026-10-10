using FlowScriptDrawControl.Control;
using FlowScriptDrawControl.Model;

namespace FlowScriptDrawControl.Command
{
    class RemoveSelectableFromScopeCommand(FlowAreaControl flowAreaControl, ScopeControl scopeControl, PositionableControl positionableControl) : EditScopeCommandBase(flowAreaControl, scopeControl, positionableControl)
    {
        private Scope previousScope;

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
