using System.Collections.Generic;

namespace PPDEditor.Command.PPDSheet
{
    class AddMarkCommand(SortedList<float, Mark>[] data, Mark mk) : Command
    {
        private readonly SortedList<float, Mark>[] data = data;
        private readonly Mark mk = mk;

        public override void Execute()
        {
            data[(int)mk.Type].Add(mk.Time, mk);
        }

        public override void Undo()
        {
            data[(int)mk.Type].Remove(mk.Time);
        }

        public override CommandType CommandType
        {
            get { return CommandType.Time; }
        }
    }
}
