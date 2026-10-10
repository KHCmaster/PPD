using FlowScriptDrawControl.Control;
using FlowScriptDrawControl.Model;

namespace FlowScriptDrawControl.Command
{
    abstract class EditScopeCommandBase(FlowAreaControl flowAreaControl, ScopeControl scopeControl, PositionableControl positionableControl) : CommandBase
    {
        protected FlowAreaControl flowAreaControl = flowAreaControl;
        protected ScopeControl scopeControl = scopeControl;
        protected PositionableControl positionableControl = positionableControl;

        protected void AddToScope()
        {
            AddToScope(scopeControl.CurrentScope);
        }

        protected void AddToScope(Scope newScope)
        {
            var scopeChild = positionableControl.CurrentPositionable as ScopeChild;
            scopeChild.Scope = newScope;
            scopeControl.AddPositionable(positionableControl);
        }

        protected void RemoveFromScope()
        {
            RemoveFromScope(null);
        }

        protected void RemoveFromScope(Scope newScope)
        {
            var scopeChild = positionableControl.CurrentPositionable as ScopeChild;
            scopeChild.Scope = newScope;
            scopeControl.RemovePositionable(positionableControl);
        }
    }
}
