using PPDFramework;
using System.Collections.Generic;

namespace PPDEditor.Command.PPDSheet
{
    class SwapLineCommand(SortedList<float, Mark>[] data, ButtonType first, ButtonType second) : Command
    {
        private readonly SortedList<float, Mark>[] data = data;
        private readonly ButtonType first = first;
        private readonly ButtonType second = second;

        public override void Execute()
        {
            var swap = new SortedList<float, Mark>(data[(int)first].Count);
            foreach (Mark mk in this.data[(int)first].Values)
            {
                mk.Type = second;
                swap.Add(mk.Time, mk);
            }
            this.data[(int)first].Clear();
            foreach (Mark mk in this.data[(int)second].Values)
            {
                mk.Type = first;
                this.data[(int)first].Add(mk.Time, mk);
            }
            this.data[(int)second].Clear();
            foreach (Mark mk in swap.Values)
            {
                this.data[(int)second].Add(mk.Time, mk);
            }
        }

        public override void Undo()
        {
            var swap = new SortedList<float, Mark>(data[(int)first].Count);
            foreach (Mark mk in this.data[(int)first].Values)
            {
                mk.Type = second;
                swap.Add(mk.Time, mk);
            }
            this.data[(int)first].Clear();
            foreach (Mark mk in this.data[(int)second].Values)
            {
                mk.Type = first;
                this.data[(int)first].Add(mk.Time, mk);
            }
            this.data[(int)second].Clear();
            foreach (Mark mk in swap.Values)
            {
                this.data[(int)second].Add(mk.Time, mk);
            }
        }

        public override CommandType CommandType
        {
            get { return CommandType.Time; }
        }
    }
}
