using PPDFramework;
using System.Collections.Generic;

namespace PPDEditor.Command.PPDSheet
{
    class FusionMarkCommand : Command
    {
        private SortedList<float, Mark>[] data;
        private Mark mk1;
        private Mark mk2;
        private ExMark exmk;

        ButtonType typeIndex;
        public FusionMarkCommand(SortedList<float, Mark>[] data, Mark mk1, Mark mk2, ExMark exmk)
        {
            this.data = data;
            this.mk1 = mk1;
            this.mk2 = mk2;
            this.exmk = exmk;

            typeIndex = mk1.Type;
        }
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
