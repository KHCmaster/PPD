using System.Collections.Generic;

namespace PPDEditor.Command.PPDSheet
{
    class DeleteMarksCommand : Command
    {
        private SortedList<float, Mark>[] data;
        private Mark[] mks;
        public DeleteMarksCommand(SortedList<float, Mark>[] data, Mark[] mks)
        {
            this.data = data;
            this.mks = mks;
        }

        public override void Execute()
        {
            foreach (Mark mk in mks)
            {
                data[(int)mk.Type].Remove(mk.Time);
            }
        }

        public override void Undo()
        {
            foreach (Mark mk in mks)
            {
                data[(int)mk.Type].Add(mk.Time, mk);
            }
        }

        public override CommandType CommandType
        {
            get { return CommandType.Time; }
        }
    }
}
