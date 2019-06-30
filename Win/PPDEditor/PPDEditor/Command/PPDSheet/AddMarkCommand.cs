using System.Collections.Generic;

namespace PPDEditor.Command.PPDSheet
{
    class AddMarkCommand : Command
    {
        private SortedList<float, Mark>[] data;
        private Mark mk;
        public AddMarkCommand(SortedList<float, Mark>[] data, Mark mk)
        {
            this.data = data;
            this.mk = mk;
        }
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
