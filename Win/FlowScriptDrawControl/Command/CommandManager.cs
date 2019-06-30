using System;
using System.Collections.Generic;

namespace FlowScriptDrawControl.Command
{
    public class CommandManager
    {
        private Stack<CommandBase> doneCommands;
        private Stack<CommandBase> undoneCommands;
        private CommandSet commandSet;

        public event Action Modified;

        public bool CanUndo
        {
            get
            {
                return doneCommands.Count > 0;
            }
        }

        public bool CanRedo
        {
            get
            {
                return undoneCommands.Count > 0;
            }
        }

        public CommandBase LastCommand
        {
            get
            {
                if (doneCommands.Count == 0)
                {
                    return null;
                }
                return doneCommands.Peek();
            }
        }

        public CommandManager()
        {
            doneCommands = new Stack<CommandBase>();
            undoneCommands = new Stack<CommandBase>();
        }

        public void AddCommand(CommandBase command)
        {
            AddCommand(command, true);
        }

        public void AddCommand(CommandBase command, bool doExecute)
        {
            if (commandSet == null)
            {
                undoneCommands.Clear();
                if (doExecute)
                {
                    command.Execute();
                }
                doneCommands.Push(command);
            }
            else
            {
                commandSet.AddCommand(command);
            }
            OnModified();
        }

        public CommandBase Undo()
        {
            var command = doneCommands.Pop();
            command.Undo();
            undoneCommands.Push(command);
            OnModified();
            return command;
        }

        public CommandBase Redo()
        {
            var command = undoneCommands.Pop();
            command.Execute();
            doneCommands.Push(command);
            OnModified();
            return command;
        }

        private void OnModified()
        {
            Modified?.Invoke();
        }

        public void Clear()
        {
            doneCommands.Clear();
            undoneCommands.Clear();
        }

        public void StartCommandSet()
        {
            commandSet = new CommandSet();
        }

        public void EndCommandSet()
        {
            var temp = commandSet;
            commandSet = null;
            if (temp.CommandCount > 0)
            {
                AddCommand(temp, false);
            }
        }
    }
}
