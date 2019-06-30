using FlowScriptDrawControl.Control;

namespace FlowScriptDrawControl.Command
{
    abstract class ScopeCommandBase : CommandBase
    {
        protected FlowAreaControl flowAreaControl;
        protected ScopeControl scopeControl;
        protected ScopeControl parentScopeControl;

        protected ScopeCommandBase(FlowAreaControl flowAreaControl, ScopeControl scopeControl, ScopeControl parentScopeControl)
        {
            this.flowAreaControl = flowAreaControl;
            this.scopeControl = scopeControl;
            this.parentScopeControl = parentScopeControl;
        }

        protected void AddScope()
        {
            if (parentScopeControl != null)
            {
                parentScopeControl.AddScope(scopeControl);
                scopeControl.CurrentScope.Parent = parentScopeControl.CurrentScope;
            }
            flowAreaControl.AddScopeControl(scopeControl, parentScopeControl);
        }

        protected void RemoveScope()
        {
            if (parentScopeControl != null)
            {
                parentScopeControl.RemoveScope(scopeControl);
                scopeControl.CurrentScope.Parent = null;
            }
            flowAreaControl.scopeCanvas.Children.Remove(scopeControl);
        }
    }
}
