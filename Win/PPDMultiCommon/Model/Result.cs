using MessagePack;

namespace PPDMultiCommon.Model
{
    [MessagePackObject]
    public class Result
    {
        [Key(0)]
        public int Score
        {
            get;
            set;
        }

        [Key(1)]
        public int CoolCount
        {
            get;
            set;
        }

        [Key(2)]
        public int GoodCount
        {
            get;
            set;
        }

        [Key(3)]
        public int SafeCount
        {
            get;
            set;
        }

        [Key(4)]
        public int SadCount
        {
            get;
            set;
        }

        [Key(5)]
        public int WorstCount
        {
            get;
            set;
        }

        [Key(6)]
        public int MaxCombo
        {
            get;
            set;
        }
    }
}
