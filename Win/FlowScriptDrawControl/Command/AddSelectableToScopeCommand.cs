using FlowScriptDrawControl.Control;
using FlowScriptDrawControl.Model;

namespace FlowScriptDrawControl.Command
{
    class AddSelectableToScopeCommand : EditScopeCommandBase
    {
        private Scope previousScope;

        public AddSelectableToScopeCommand(FlowAreaControl flowAreaControl, ScopeControl scopeControl,
            PositionableControl positionableControl)
            : base(flowAreaControl, scopeControl, positionableControl)
        {

        }

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
