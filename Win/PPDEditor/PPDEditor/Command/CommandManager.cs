using System.Collections.Generic;

namespace PPDEditor.Command
{
    public delegate void CommandEventHandler(CommandType commandType);
    public class CommandManager
    {
        public event CommandEventHandler CommandChanged;
        Stack<Command> donecommand;
        Stack<Command> undonecommand;

        private CommandSet commandSet;

        public bool CanUndo
        {
            get
            {
                return donecommand.Count != 0;
            }
        }
        public bool CanRedo
        {
            get
            {
                return undonecommand.Count != 0;
            }
        }
        public int DoneCommandNumber
        {
            get
            {
                return donecommand.Count;
            }
        }
        public int UndoneCommandNumber
        {
            get
            {
                return undonecommand.Count;
            }
        }
        public Stack<Command> DoneCommand
        {
            get
            {
                return donecommand;
            }
        }
        public Stack<Command> UndoneCommand
        {
            get
            {
                return undonecommand;
            }
        }
        public Command LastDoneCommand
        {
            get
            {
                if (donecommand.Count == 0) return null;
                else return donecommand.Peek();
            }
        }
        public void ClearAll()
        {
            donecommand.Clear();
            undonecommand.Clear();
            OnCommandChange(CommandType.Pos | CommandType.Time | CommandType.ID);
        }
        public CommandManager()
        {
            donecommand = new Stack<Command>();
            undonecommand = new Stack<Command>();
        }

        public void StartGroupCommand()
        {
            commandSet = new CommandSet();
        }

        public void AddCommand(Command command)
        {
            if (commandSet == null)
            {
                undonecommand.Clear();
                command.Execute();
                if (LastDoneCommand != command)
                {
                    donecommand.Push(command);
                }
                OnCommandChange(command.CommandType);
            }
            else
            {
                commandSet.AddCommand(command);
                command.Execute();
            }
        }

        public void EndGroupCommand()
        {
            CommandSet temp = commandSet;
            temp.EndAdd();
            commandSet = null;
            if (temp.CommandCount > 0)
            {
                donecommand.Push(temp);
                OnCommandChange(temp.CommandType);
            }
        }

        public Command Undo()
        {
            var c = donecommand.Pop();
            c.Undo();
            undonecommand.Push(c);
            OnCommandChange(c.CommandType);
            return c;
        }
        public Command Redo()
        {
            var c = undonecommand.Pop();
            c.Execute();
            donecommand.Push(c);
            OnCommandChange(c.CommandType);
            return c;
        }
        private void OnCommandChange(CommandType commandType)
        {
            if (CommandChanged != null)
            {
                CommandChanged.Invoke(commandType);
            }
        }
    }
}
