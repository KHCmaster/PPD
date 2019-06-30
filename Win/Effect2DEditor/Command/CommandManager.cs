using System;
using System.Collections.Generic;

namespace Effect2DEditor.Command
{
    public class CommandManager
    {
        public event EventHandler CommandChanged;
        Stack<CommandBase> donecommand;
        Stack<CommandBase> undonecommand;
        public CommandBase LastCommand
        {
            get
            {
                if (donecommand.Count == 0) return null;
                else return donecommand.Peek();
            }
        }
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
        public Stack<CommandBase> DoneCommand
        {
            get
            {
                return donecommand;
            }
        }
        public Stack<CommandBase> UndoneCommand
        {
            get
            {
                return undonecommand;
            }
        }
        public CommandBase LastDoneCommand
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
            FireChangeEvent();
        }
        public CommandManager()
        {
            donecommand = new Stack<CommandBase>();
            undonecommand = new Stack<CommandBase>();
        }
        public void AddCommand(CommandBase command)
        {
            undonecommand.Clear();
            donecommand.Push(command);
            FireChangeEvent();
        }
        public CommandBase Undo()
        {
            var cu = donecommand.Pop();
            cu.Undo();
            undonecommand.Push(cu);
            FireChangeEvent();
            return cu;
        }
        public CommandBase Redo()
        {
            var cu = undonecommand.Pop();
            cu.Execute();
            donecommand.Push(cu);
            FireChangeEvent();
            return cu;
        }
        private void FireChangeEvent()
        {
            if (CommandChanged != null)
            {
                CommandChanged.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
