using System.Collections.Generic;

namespace PPDEditor.Command.PPDSheet
{
    class ShiftMarkTimeCommand : Command
    {
        private SortedList<float, Mark>[] data;
        private Mark mk;
        private ExMark exmk;
        private int typeIndex;
        private float newTime;
        private float lastTime;
        private float newEndTime;
        private float lastEndTime;
        public ShiftMarkTimeCommand(SortedList<float, Mark>[] data, int typeIndex, Mark mk, float nt)
        {
            this.data = data;
            this.mk = mk;
            this.typeIndex = typeIndex;
            this.exmk = mk as ExMark;
            this.lastTime = mk.Time;
            this.newTime = nt;
            while (data[typeIndex].ContainsKey(newTime))
            {
                newTime -= 0.0001f;
            }
            if (exmk != null)
            {
                this.lastEndTime = exmk.EndTime;
                this.newEndTime = exmk.EndTime + newTime - mk.Time;
            }
        }

        public override void Execute()
        {
            data[typeIndex].Remove(mk.Time);
            mk.Time = newTime;
            if (exmk != null)
            {
                exmk.EndTime = newEndTime;
            }
            data[typeIndex].Add(mk.Time, mk);
        }

        public override void Undo()
        {
            data[typeIndex].Remove(mk.Time);
            mk.Time = lastTime;
            if (exmk != null)
            {
                exmk.EndTime = lastEndTime;
            }
            data[typeIndex].Add(mk.Time, mk);
        }

        public override CommandType CommandType
        {
            get { return CommandType.Time; }
        }
    }
}
