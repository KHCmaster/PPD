using System.Collections.Generic;

namespace PPDEditor.Command.PPDSheet
{
    class DeleteDataCommand : Command
    {
        private Mark[][] prevData;
        private SortedList<float, Mark>[] data;
        int index;
        public DeleteDataCommand(SortedList<float, Mark>[] data, int index)
        {
            this.data = data;
            this.index = index;
            if (index < 0)
            {
                prevData = new Mark[10][];
                for (int i = 0; i < 10; i++)
                {
                    prevData[i] = new Mark[data[i].Count];
                    for (int j = 0; j < data[i].Count; j++)
                    {
                        prevData[i][j] = data[i].Values[j];
                    }
                }
            }
            else
            {
                prevData = new Mark[1][];
                prevData[0] = new Mark[data[index].Count];
                for (int i = 0; i < data[index].Count; i++)
                {
                    prevData[0][i] = data[index].Values[i];
                }
            }
        }
        public override void Execute()
        {
            if (index < 0)
            {
                for (int i = 0; i < 10; i++)
                {
                    data[i].Clear();
                }
            }
            else
            {
                data[index].Clear();
            }
        }

        public override void Undo()
        {
            if (index < 0)
            {
                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < prevData[i].Length; j++)
                    {
                        data[i].Add(prevData[i][j].Time, prevData[i][j]);
                    }
                }
            }
            else
            {
                for (int i = 0; i < prevData[0].Length; i++)
                {
                    data[index].Add(prevData[0][i].Time, prevData[0][i]);
                }
            }
        }

        public override CommandType CommandType
        {
            get { return CommandType.Time; }
        }
    }
}
