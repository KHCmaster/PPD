using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PPDCore
{
    class PPDPlayInput
    {
        public bool IsEmpty
        {
            get
            {
                return PressCount.All(val => val == 0) && Pressed.All(val => !val) && Released.All(val => !val);
            }
        }

        public UInt16[] PressCount
        {
            get;
            private set;
        }

        public bool[] Released
        {
            get;
            private set;
        }

        public bool[] Pressed
        {
            get;
            private set;
        }

        public byte[] PressCountCompressed
        {
            get
            {
                bool[] pressed = new bool[PressCount.Length];
                for (int i = 0; i < pressed.Length; i++)
                {
                    pressed[i] = PressCount[i] > 0;
                }
                var bits = new BitArray(pressed);
                byte[] temp = new byte[(int)Math.Ceiling(bits.Length / 8.0)];
                bits.CopyTo(temp, 0);
                var ret = new List<byte>(temp);
                for (int i = 0; i < pressed.Length; i++)
                {
                    if (pressed[i])
                    {
                        ret.AddRange(BitConverter.GetBytes(PressCount[i]));
                    }
                }
                return ret.ToArray();
            }
        }

        public byte[] PressedCompressed
        {
            get
            {
                var bits = new BitArray(Pressed);
                byte[] ret = new byte[(int)Math.Ceiling(bits.Length / 8.0)];
                bits.CopyTo(ret, 0);
                return ret;
            }
        }

        public byte[] ReleasedCompressed
        {
            get
            {
                var bits = new BitArray(Released);
                byte[] ret = new byte[(int)Math.Ceiling(bits.Length / 8.0)];
                bits.CopyTo(ret, 0);
                return ret;
            }
        }

        public float Time
        {
            get;
            private set;
        }

        public long CurrentTime
        {
            get;
            private set;
        }

        public string PressCountAsString
        {
            get
            {
                return String.Join(",", PressCount);
            }
        }

        public string PressedAsString
        {
            get
            {
                return String.Join(",", Pressed.Select(b => b ? "1" : "0").ToArray());
            }
        }

        public string ReleasedAsString
        {
            get
            {
                return String.Join(",", Released.Select(b => b ? "1" : "0").ToArray());
            }
        }

        public PPDPlayInput(float time, long currentTime, UInt16[] presscount, bool[] pressed, bool[] released)
        {
            PressCount = new UInt16[10];
            Pressed = new bool[10];
            Released = new bool[10];
            Array.Copy(presscount, PressCount, PressCount.Length);
            Array.Copy(pressed, Pressed, Pressed.Length);
            Array.Copy(released, Released, Released.Length);
            this.Time = time;
            this.CurrentTime = currentTime;
        }

        public PPDPlayInput(float time, long currentTime, UInt16[] presscount, byte[] pressed, byte[] released)
        {
            PressCount = new UInt16[10];
            Released = new bool[10];
            Pressed = new bool[10];
            Array.Copy(presscount, PressCount, PressCount.Length);
            var bits = new BitArray(pressed);
            for (int i = 0; i < Pressed.Length; i++)
            {
                Pressed[i] = bits.Get(i);
            }
            bits = new BitArray(released);
            for (int i = 0; i < Released.Length; i++)
            {
                Released[i] = bits.Get(i);
            }
            this.Time = time;
            this.CurrentTime = currentTime;
        }

        public override string ToString()
        {
            return String.Format("T:{0}, PC:{1}, P:{2}, R:{3}", Time, PressCountAsString, PressedAsString, ReleasedAsString);
        }
    }
}
