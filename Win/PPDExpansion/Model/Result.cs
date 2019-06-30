using System;
using System.Collections.Generic;

namespace PPDExpansion.Model
{
    class Result
    {
        public string Hash
        {
            get;
            private set;
        }

        public int Score
        {
            get;
            private set;
        }

        public KeyValuePair<DateTime, int>[] Data
        {
            get;
            private set;
        }

        public byte[] DataAsByte
        {
            get
            {
                byte[] ret = new byte[Data.Length * 12];
                for (int i = 0; i < Data.Length; i++)
                {
                    var temp = BitConverter.GetBytes(Data[i].Key.ToBinary());
                    Array.Copy(temp, 0, ret, i * 12, temp.Length);
                    temp = BitConverter.GetBytes(Data[i].Value);
                    Array.Copy(temp, 0, ret, i * 12 + 8, temp.Length);
                }
                return ret;
            }
        }

        public Result(string hash, int score, KeyValuePair<DateTime, int>[] data)
        {
            Hash = hash;
            Score = score;
            Data = data;
        }

        public Result(string hash, int score, byte[] data)
        {
            Hash = hash;
            Score = score;
            KeyValuePair<DateTime, int>[] pairs = new KeyValuePair<DateTime, int>[data.Length / 12];
            for (int i = 0; i < pairs.Length; i++)
            {
                var tempTime = BitConverter.ToInt64(data, i * 12);
                var tempScore = BitConverter.ToInt32(data, i * 12 + 8);
                pairs[i] = new KeyValuePair<DateTime, int>(DateTime.FromBinary(tempTime), tempScore);
            }
            Data = pairs;
        }
    }
}
