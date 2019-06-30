using PPDFramework;
using System.Collections.Generic;

namespace PPDEditor.Command.PPDSheet
{
    class DeFusionMarkCommand : Command
    {
        private SortedList<float, Mark>[] data;
        private Mark mk1;
        private Mark mk2;
        private ExMark exmk;

        ButtonType typeIndex;
        public DeFusionMarkCommand(SortedList<float, Mark>[] data, Mark mk1, Mark mk2, ExMark exmk)
        {
            this.data = data;
            this.mk1 = mk1;
            this.mk2 = mk2;
            this.exmk = exmk;

            typeIndex = mk1.Type;
        }
        public override void Execute()
        {
            SafeRemoveMark(exmk.Time, data[(int)typeIndex]);
            SafeAddMark(mk1, data[(int)typeIndex]);
            SafeAddMark(mk2, data[(int)typeIndex]);
        }

        public override void Undo()
        {
            SafeRemoveMark(mk1.Time, data[(int)typeIndex]);
            SafeRemoveMark(mk2.Time, data[(int)typeIndex]);
            SafeAddMark(exmk, data[(int)typeIndex]);
        }

        private void SafeAddMark(Mark mk, SortedList<float, Mark> data)
        {
            if (!data.ContainsKey(mk.Time))
            {
                data.Add(mk.Time, mk);
            }
        }

        private void SafeRemoveMark(float time, SortedList<float, Mark> data)
        {
            if (data.ContainsKey(time))
            {
                data.Remove(time);
            }
        }

        public override CommandType CommandType
        {
            get { return CommandType.Time; }
        }
    }
}
