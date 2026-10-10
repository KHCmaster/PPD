using System.Collections.Generic;

namespace PPDEditor.Command.PPDSheet
{
    class ForceToNearBarCommand : Command
    {
        readonly Dictionary<Mark, float[]> lastTimes;
        readonly Dictionary<Mark, float[]> times;
        readonly SortedList<float, Mark>[] data;
        public ForceToNearBarCommand(SortedList<float, Mark>[] data, Dictionary<Mark, float[]> times)
        {
            this.data = data;
            this.times = times;
            lastTimes = [];
            foreach (var mark in times.Keys)
            {
                if (mark is ExMark exmk)
                {
                    lastTimes[exmk] = [exmk.Time, exmk.EndTime];
                }
                else
                {
                    lastTimes[mark] = [mark.Time];
                }
            }
        }

        public override void Execute()
        {
            Change(times);
        }

        public override void Undo()
        {
            Change(lastTimes);
        }

        private void Change(Dictionary<Mark, float[]> times)
        {
            foreach (var p in times)
            {
                var index = (int)p.Key.Type;
                data[index].Remove(p.Key.Time);

                if (p.Key is ExMark exmk)
                {
                    exmk.EndTime = p.Value[1];
                }
                p.Key.Time = p.Value[0];
                if (!data[index].ContainsKey(p.Value[0]))
                {
                    data[index][p.Key.Time] = p.Key;
                }
            }
        }

        public override CommandType CommandType
        {
            get { return CommandType.Time; }
        }
    }
}
