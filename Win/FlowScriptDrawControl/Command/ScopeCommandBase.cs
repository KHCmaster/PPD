using FlowScriptDrawControl.Control;

namespace FlowScriptDrawControl.Command
{
    abstract class ScopeCommandBase(FlowAreaControl flowAreaControl, ScopeControl scopeControl, ScopeControl parentScopeControl) : CommandBase
    {
        protected FlowAreaControl flowAreaControl = flowAreaControl;
        protected ScopeControl scopeControl = scopeControl;
        protected ScopeControl parentScopeControl = parentScopeControl;

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
