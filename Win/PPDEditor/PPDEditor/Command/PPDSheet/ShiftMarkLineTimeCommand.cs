using System.Collections.Generic;

namespace PPDEditor.Command.PPDSheet
{
    class ShiftMarkLineTimeCommand : Command
    {
        float[][][] lastTime;
        int index;
        float dt;
        SortedList<float, Mark>[] data;
        public ShiftMarkLineTimeCommand(SortedList<float, Mark>[] data, int index, float dt)
        {
            this.data = data;
            this.index = index;
            this.dt = dt;
            lastTime = new float[10][][];
            for (int i = 0; i < lastTime.Length; i++)
            {
                lastTime[i] = new float[data[i].Count][];
                for (int j = 0; j < data[i].Count; j++)
                {
                    Mark mk = data[i].Values[j];
                    var exmk = mk as ExMark;
                    if (exmk == null)
                    {
                        lastTime[i][j] = new float[] { mk.Time };
                    }
                    else
                    {
                        lastTime[i][j] = new float[] { exmk.Time, exmk.EndTime };
                    }
                }
            }
        }

        public int Index
        {
            get
            {
                return index;
            }
        }

        public float Dt
        {
            get
            {
                return dt;
            }
            set
            {
                dt = value;
            }
        }

        public override void Execute()
        {
            if (index >= 0 && index < 10)
            {
                var tempData = new List<Mark>(data[index].Values);
                data[index].Clear();

                for (int i = 0; i < tempData.Count; i++)
                {
                    Mark mk = tempData[i];
                    if (mk is ExMark exmk)
                    {
                        exmk.EndTime = lastTime[index][i][1] + dt;
                    }
                    mk.Time = lastTime[index][i][0] + dt;
                    data[index].Add(mk.Time, mk);
                }
            }
            else if (index == int.MaxValue)
            {
                for (int i = 0; i < 10; i++)
                {
                    var tempData = new List<Mark>(data[i].Values);
                    data[i].Clear();

                    for (int j = 0; j < tempData.Count; j++)
                    {
                        Mark mk = tempData[j];
                        if (mk is ExMark exmk)
                        {
                            exmk.EndTime = lastTime[i][j][1] + dt;
                        }
                        mk.Time = lastTime[i][j][0] + dt;
                        data[i].Add(mk.Time, mk);
                    }
                }
            }
        }

        public override void Undo()
        {
            if (index >= 0 && index < 10)
            {
                var tempData = new List<Mark>(data[index].Values);
                data[index].Clear();

                for (int i = 0; i < tempData.Count; i++)
                {
                    Mark mk = tempData[i];
                    if (mk is ExMark exmk)
                    {
                        exmk.EndTime = lastTime[index][i][1];
                    }
                    mk.Time = lastTime[index][i][0];
                    data[index].Add(mk.Time, mk);
                }
            }
            else if (index == int.MaxValue)
            {
                for (int i = 0; i < 10; i++)
                {
                    var tempData = new List<Mark>(data[i].Values);
                    data[i].Clear();

                    for (int j = 0; j < tempData.Count; j++)
                    {
                        Mark mk = tempData[j];
                        if (mk is ExMark exmk)
                        {
                            exmk.EndTime = lastTime[i][j][1];
                        }
                        mk.Time = lastTime[i][j][0];
                        data[i].Add(mk.Time, mk);
                    }
                }
            }
        }

        public override CommandType CommandType
        {
            get { return CommandType.Time; }
        }
    }
}
