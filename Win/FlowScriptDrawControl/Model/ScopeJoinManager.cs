using FlowScriptDrawControl.Command;
using FlowScriptDrawControl.Control;
using System;
using System.Linq;
using System.Windows.Input;

namespace FlowScriptDrawControl.Model
{
    class ScopeJoinManager
    {
        private FlowAreaControl flowAreaControl;
        private MoveManager moveManager;
        private Command.CommandManager commandManager;
        private CommandSet commandSet;

        public ScopeJoinManager(FlowAreaControl flowAreaControl, Command.CommandManager commandManager, MoveManager moveManager)
        {
            this.flowAreaControl = flowAreaControl;
            this.commandManager = commandManager;
            this.moveManager = moveManager;
            commandSet = new CommandSet();
            moveManager.ControlPressed += moveManager_ControlPressed;
            moveManager.MoveEnd += moveManager_MoveEnd;
        }

        void moveManager_MoveEnd(object sender, EventArgs e)
        {
            if (commandSet.CommandCount > 0)
            {
                commandManager.AddCommand(commandSet, false);
            }
        }

        void moveManager_ControlPressed(object sender, EventArgs e)
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
                var scopeChild = control.CurrentPositionable as ScopeChild;
                if (scopeChild == null || scopeChild.Scope == null)
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
