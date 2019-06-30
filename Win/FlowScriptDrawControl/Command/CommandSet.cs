using System.Collections.Generic;
using System.Linq;

namespace FlowScriptDrawControl.Command
{
    class CommandSet : CommandBase
    {
        private List<CommandBase> list;

        public int CommandCount
        {
            get
            {
                return list.Count;
            }
        }

        public CommandSet()
        {
            list = new List<CommandBase>();
        }

        public void AddCommand(CommandBase command)
        {
            list.Add(command);
            command.Execute();
        }

        public override void Execute()
        {
            foreach (CommandBase command in list)
            {
                command.Execute();
            }
        }

        public override void Undo()
        {
            foreach (CommandBase command in list.Reverse<CommandBase>())
            {
                command.Undo();
            }
        }
    }
}
