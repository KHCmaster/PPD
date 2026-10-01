using FlowScriptDrawControl.Control;
using FlowScriptDrawControl.Model;

namespace FlowScriptDrawControl.Command
{
    class AddSelectableToScopeCommand(FlowAreaControl flowAreaControl, ScopeControl scopeControl,
        PositionableControl positionableControl) : EditScopeCommandBase(flowAreaControl, scopeControl, positionableControl)
    {
        private Scope previousScope;

        public override void Execute()
        {
            var scopeChild = positionableControl.CurrentPositionable as ScopeChild;
            previousScope = scopeChild.Scope;
            AddToScope();
        }

        public override void Undo()
        {
            RemoveFromScope(previousScope);
        }
    }
}
