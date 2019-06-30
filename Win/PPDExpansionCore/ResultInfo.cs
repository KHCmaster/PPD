using System;

namespace PPDExpansionCore
{
    public class ResultInfo : PackableBase
    {
        public override DataType DataType
        {
            get { return DataType.ResultInfo; }
        }

        [Ignore]
        public DateTime DateTime
        {
            get;
            set;
        }

        public int Score
        {
            get;
            set;
        }

        public int CoolCount
        {
            get;
            set;
        }

        public int GoodCount
        {
            get;
            set;
        }

        public int SafeCount
        {
            get;
            set;
        }

        public int SadCount
        {
            get;
            set;
        }

        public int WorstCount
        {
            get;
            set;
        }

        public int MaxCombo
        {
            get;
            set;
        }
    }
}
