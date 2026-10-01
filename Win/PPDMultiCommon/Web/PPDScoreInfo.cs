using PPDFrameworkCore;
using System;

namespace PPDMultiCommon.Web
{
    public class PPDScoreInfo
    {
        byte[] easy, normal, hard, extreme;

        public string Title
        {
            get;
            set;
        }

        public string EasyHash
        {
            get;
            set;
        }

        public string NormalHash
        {
            get;
            set;
        }

        public string HardHash
        {
            get;
            set;
        }

        public string ExtremeHash
        {
            get;
            set;
        }

        public string Hash
        {
            get;
            set;
        }

        public string MovieUrl
        {
            get;
            set;
        }

        public byte[] Easy
        {
            get
            {
                easy ??= ParseString(EasyHash);
                return easy;
            }
        }

        public byte[] Normal
        {
            get
            {
                normal ??= ParseString(NormalHash);
                return normal;
            }
        }

        public byte[] Hard
        {
            get
            {
                hard ??= ParseString(HardHash);
                return hard;
            }
        }

        public byte[] Extreme
        {
            get
            {
                extreme ??= ParseString(ExtremeHash);
                return extreme;
            }
        }

        private byte[] ParseString(string str)
        {
            if (String.IsNullOrEmpty(str))
            {
                return new byte[0];
            }

            byte[] ret = new byte[32];
            for (int i = 0; i < str.Length; i += 2)
            {
                ret[i / 2] = (byte)int.Parse(str.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return ret;
        }

        private byte[] GetHash(Difficulty difficulty)
        {
            switch (difficulty)
            {
                case Difficulty.Easy:
                    return Easy;
                case Difficulty.Normal:
                    return Normal;
                case Difficulty.Hard:
                    return Hard;
                case Difficulty.Extreme:
                    return Extreme;
            }

            return null;
        }

        public string CreateUrl()
        {
            return String.Format(String.Format("{0}/score/index/id/{1}", PPDFrameworkCore.Web.WebManager.BaseUrl, Hash.ToLower()));
        }
    }
}