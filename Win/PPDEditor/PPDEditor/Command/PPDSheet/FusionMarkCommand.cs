using PPDFramework;
using System.Collections.Generic;

namespace PPDEditor.Command.PPDSheet
{
    class FusionMarkCommand(SortedList<float, Mark>[] data, Mark mk1, Mark mk2, ExMark exmk) : Command
    {
        private readonly SortedList<float, Mark>[] data = data;
        private readonly Mark mk1 = mk1;
        private readonly Mark mk2 = mk2;
        private readonly ExMark exmk = exmk;

        readonly ButtonType typeIndex = mk1.Type;

        public override void Execute()
        {
            data[(int)typeIndex].Remove(mk1.Time);
            data[(int)typeIndex].Remove(mk2.Time);
            data[(int)typeIndex].Add(exmk.Time, exmk);
        }

        public override void Undo()
        {
            data[(int)typeIndex].Remove(exmk.Time);
            data[(int)typeIndex].Add(mk1.Time, mk1);
            data[(int)typeIndex].Add(mk2.Time, mk2);
        }

        public override CommandType CommandType
        {
            get { return CommandType.Time; }
        }
    }
}
