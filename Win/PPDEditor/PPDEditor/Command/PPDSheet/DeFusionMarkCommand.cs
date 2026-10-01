using PPDFramework;
using System.Collections.Generic;

namespace PPDEditor.Command.PPDSheet
{
    class DeFusionMarkCommand(SortedList<float, Mark>[] data, Mark mk1, Mark mk2, ExMark exmk) : Command
    {
        private readonly SortedList<float, Mark>[] data = data;
        private readonly Mark mk1 = mk1;
        private readonly Mark mk2 = mk2;
        private readonly ExMark exmk = exmk;

        readonly ButtonType typeIndex = mk1.Type;

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
