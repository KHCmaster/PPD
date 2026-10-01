using FlowScriptDrawControl.Command;
using FlowScriptDrawControl.Control;
using System;
using System.Linq;
using System.Windows.Input;

namespace FlowScriptDrawControl.Model
{
    class ScopeJoinManager
    {
        private readonly FlowAreaControl flowAreaControl;
        private readonly MoveManager moveManager;
        private readonly Command.CommandManager commandManager;
        private readonly CommandSet commandSet;

        public ScopeJoinManager(FlowAreaControl flowAreaControl, Command.CommandManager commandManager, MoveManager moveManager)
        {
            this.flowAreaControl = flowAreaControl;
            this.commandManager = commandManager;
            this.moveManager = moveManager;
            commandSet = new CommandSet();
            moveManager.ControlPressed += MoveManager_ControlPressed;
            moveManager.MoveEnd += MoveManager_MoveEnd;
        }

        void MoveManager_MoveEnd(object sender, EventArgs e)
        {
            if (commandSet.CommandCount > 0)
            {
                commandManager.AddCommand(commandSet, false);
            }
        }

        void MoveManager_ControlPressed(object sender, EventArgs e)
        {
            var moveManager = sender as MoveManager;
            var control = moveManager.Controls.FirstOrDefault(c => (c.CurrentPositionable as ScopeChild).Scope != null);
            if (control != null)
            {
                Dejoin(moveManager);
            }
            else
            {
                Join(moveManager);
            }
        }

        private void Join(MoveManager moveManager)
        {
            var p = flowAreaControl.ToLocal(Mouse.GetPosition(flowAreaControl));
            var control = flowAreaControl.GetScopeByPosition(p);
            if (control == null)
            {
                return;
            }

            foreach (PositionableControl positionableControl in moveManager.Controls.Where(c => !control.Contains(c)))
            {
                var command = new AddSelectableToScopeCommand(flowAreaControl, control, positionableControl);
                commandSet.AddCommand(command);
            }
        }

        private void Dejoin(MoveManager moveManager)
        {
            foreach (PositionableControl control in moveManager.Controls)
            {
                if (control.CurrentPositionable is not ScopeChild scopeChild || scopeChild.Scope == null)
                {
                    continue;
                }

                var scopeControl = flowAreaControl.GetAllScopeControls().FirstOrDefault(c => c.CurrentScope == scopeChild.Scope);
                var command = new RemoveSelectableFromScopeCommand(flowAreaControl, scopeControl, control);
                commandSet.AddCommand(command);
            }
        }
    }
}
