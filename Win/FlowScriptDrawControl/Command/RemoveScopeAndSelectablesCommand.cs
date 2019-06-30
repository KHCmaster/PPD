using FlowScriptDrawControl.Control;
using FlowScriptDrawControl.Model;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptDrawControl.Command
{
    class RemoveScopeAndSelectablesCommand : ScopeCommandBase
    {
        private RemoveScopeAndSelectablesCommand[] removeScopeCommands;
        private RemoveSelectablesCommand removeSelectablesCommand;

        public RemoveScopeAndSelectablesCommand(FlowAreaControl flowAreaControl, ScopeControl scopeControl, ScopeControl parentScopeControl)
            : base(flowAreaControl, scopeControl, parentScopeControl)
        {
        }

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
            removeScopeCommands = list.ToArray();
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
            if (removeSelectablesCommand != null)
            {
                removeSelectablesCommand.Undo();
            }
        }
    }
}
