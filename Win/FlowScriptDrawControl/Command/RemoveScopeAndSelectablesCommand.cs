using FlowScriptDrawControl.Control;
using FlowScriptDrawControl.Model;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptDrawControl.Command
{
    class RemoveScopeAndSelectablesCommand(FlowAreaControl flowAreaControl, ScopeControl scopeControl, ScopeControl parentScopeControl) : ScopeCommandBase(flowAreaControl, scopeControl, parentScopeControl)
    {
        private RemoveScopeAndSelectablesCommand[] removeScopeCommands;
        private RemoveSelectablesCommand removeSelectablesCommand;

        public override void Execute()
        {
            var selectableControls = flowAreaControl.GetAllSelectableControls().Where(s =>
            {
                var scopeChild = s.CurrentSelectable as ScopeChild;
                return scopeChild.Scope == scopeControl.CurrentScope;
            }).ToArray();
            if (selectableControls.Length > 0)
            {
                removeSelectablesCommand = new RemoveSelectablesCommand(flowAreaControl, selectableControls);
                removeSelectablesCommand.Execute();
            }
            var list = new List<RemoveScopeAndSelectablesCommand>();
            foreach (ScopeControl control in scopeControl.ChildScopes)
            {
                list.Add(new RemoveScopeAndSelectablesCommand(flowAreaControl, control, scopeControl));
            }
            removeScopeCommands = [.. list];
            foreach (RemoveScopeAndSelectablesCommand command in removeScopeCommands)
            {
                command.Execute();
            }
            RemoveScope();
        }

        public override void Undo()
        {
            AddScope();
            foreach (RemoveScopeAndSelectablesCommand command in removeScopeCommands)
            {
                command.Undo();
            }
            removeSelectablesCommand?.Undo();
        }
    }
}
