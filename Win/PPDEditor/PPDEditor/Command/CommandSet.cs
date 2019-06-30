using System.Collections.Generic;

namespace PPDEditor.Command
{
    class CommandSet : Command
    {
        List<Command> commands;
        CommandType commandType;
        public CommandSet()
        {
            commands = new List<Command>();
        }

        public void AddCommand(Command command)
        {
            commands.Add(command);
        }

        public int CommandCount
        {
            get
            {
                return commands.Count;
            }
        }

        public void EndAdd()
        {
            for (int i = 0; i < commands.Count; i++)
            {
                commandType |= commands[i].CommandType;
            }
        }

        public override void Execute()
        {
            for (int i = 0; i < commands.Count; i++)
            {
                commands[i].Execute();
            }
        }

        public override void Undo()
        {
            for (int i = commands.Count - 1; i >= 0; i--)
            {
                commands[i].Undo();
            }
        }

        public override CommandType CommandType
        {
            get { return commandType; }
        }
    }
}
