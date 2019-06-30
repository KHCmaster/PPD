using PPDFramework;
using System;

namespace PPDCore
{
    public class RandomChangeManager
    {
        [Flags]
        public enum RandomType
        {
            None = 0,
            Symbol = 1,
            Direction = 2,
            LR = 4,
            All = 7,
        }

        int[] keyChange;
        Random r = new Random();

        public ButtonType this[ButtonType type]
        {
            get
            {
                return (ButtonType)keyChange[(int)type];
            }
        }

        public RandomChangeManager()
        {
            keyChange = new int[ButtonUtility.Array.Length];
            for (int i = 0; i < keyChange.Length; i++)
            {
                keyChange[i] = i;
            }
        }

        public int Invert(int type)
        {
            return Array.IndexOf(keyChange, type);
        }

        public void Initialize(RandomType randomType)
        {
            // swap LR
            if (randomType.HasFlag(RandomType.LR))
            {
                if (r.Next(0, 2) % 2 == 0)
                {
                    Swap(keyChange, 8, 9);
                }
            }
            // swap in symbol
            if (randomType.HasFlag(RandomType.Symbol))
            {
                RandomSwap(0, 4);
            }
            // swap in direction
            if (randomType.HasFlag(RandomType.Direction))
            {
                RandomSwap(4, 8);
            }
            // exchange symbol and direction
            if (randomType.HasFlag(RandomType.Symbol) && randomType.HasFlag(RandomType.Direction))
            {
                if (r.Next(0, 2) % 2 == 0)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Swap(keyChange, i, i + 4);
                    }
                }
            }
        }

        private void RandomSwap(int start, int end)
        {
            for (int i = 0; i < 100; i++)
            {
                int a = r.Next(start, end), b = r.Next(start, end);
                Swap(keyChange, a, b);
            }
        }

        private int[] Swap(int[] array, int a, int b)
        {
            int swap = array[a];
            array[a] = array[b];
            array[b] = swap;
            return array;
        }
    }
}
