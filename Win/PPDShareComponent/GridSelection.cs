using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PPDShareComponent
{
    public class GridSelection
    {
        private List<KeyValuePair<float, List<float>>> data;
        private int[] current = new int[2];
        private int stickeyX = -1;

        public int Current
        {
            get;
            private set;
        }

        public GridSelection()
        {
            data = new List<KeyValuePair<float, List<float>>>();
        }

        private List<float> Find(float y)
        {
            return data.FirstOrDefault(kvp => kvp.Key == y).Value;
        }

        public void SetAt(Vector2 pos)
        {
            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].Key == pos.Y)
                {
                    current[0] = i;
                    for (int j = 0; j < data[i].Value.Count; j++)
                    {
                        if (data[i].Value[j] == pos.X)
                        {
                            current[1] = j;
                            break;
                        }
                    }
                    break;
                }
            }
            UpdateCurrent();
        }

        public void Add(Vector2 pos)
        {
            var list = Find(pos.Y);
            if (list == null)
            {
                list = new List<float>();
                data.Add(new KeyValuePair<float, List<float>>(pos.Y, list));
            }
            list.Add(pos.X);
        }

        public void Initialize()
        {
            foreach (KeyValuePair<float, List<float>> kvp in data)
            {
                kvp.Value.Sort();
            }
            data.Sort((Comparison<KeyValuePair<float, List<float>>>)((kvp1, kvp2) => Math.Sign(kvp1.Key - kvp2.Key)));
            current[0] = current[1] = 0;
            UpdateCurrent();
        }

        private void UpdateCurrent()
        {
            int iter = 0;
            for (int i = 0; i < data.Count; i++)
            {
                if (i != current[0])
                {
                    iter += data[i].Value.Count;
                    continue;
                }
                iter += current[1];
                break;
            }
            Current = iter;
        }

        public void Left()
        {
            List<float> list = data[current[0]].Value;
            current[1]--;
            if (current[1] < 0)
            {
                current[1] = list.Count - 1;
            }
            stickeyX = -1;
            UpdateCurrent();
        }

        public void Right()
        {
            List<float> list = data[current[0]].Value;
            current[1]++;
            if (current[1] >= list.Count)
            {
                current[1] = 0;
            }
            stickeyX = -1;
            UpdateCurrent();
        }

        public void Up()
        {
            int lastX = current[1];
            current[0]--;
            if (current[0] < 0)
            {
                current[0] = data.Count - 1;
            }
            if (stickeyX >= 0)
            {
                current[1] = stickeyX;
            }
            if (current[1] >= data[current[0]].Value.Count)
            {
                current[1] = data[current[0]].Value.Count - 1;
                stickeyX = Math.Max(stickeyX, lastX);
            }
            UpdateCurrent();
        }

        public void Down()
        {
            int lastX = current[1];
            current[0]++;
            if (current[0] >= data.Count)
            {
                current[0] = 0;
            }
            if (stickeyX >= 0)
            {
                current[1] = stickeyX;
            }
            if (current[1] >= data[current[0]].Value.Count)
            {
                current[1] = data[current[0]].Value.Count - 1;
                stickeyX = Math.Max(stickeyX, lastX);
            }
            UpdateCurrent();
        }
    }
}
