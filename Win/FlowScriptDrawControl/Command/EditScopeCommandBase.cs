using FlowScriptDrawControl.Control;
using FlowScriptDrawControl.Model;

namespace FlowScriptDrawControl.Command
{
    abstract class EditScopeCommandBase : CommandBase
    {
        protected FlowAreaControl flowAreaControl;
        protected ScopeControl scopeControl;
        protected PositionableControl positionableControl;

        protected EditScopeCommandBase(FlowAreaControl flowAreaControl, ScopeControl scopeControl, PositionableControl positionableControl)
        {
            this.flowAreaControl = flowAreaControl;
            this.scopeControl = scopeControl;
            this.positionableControl = positionableControl;
        }

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
