namespace PPDExpansionCore
{
    public class UpdateInfo : PackableBase
    {
        public override DataType DataType
        {
            get { return DataType.UpdateInfo; }
        }

        public int PlayerId
        {
            get;
            set;
        }

        public float CurrentTime
        {
            get;
            set;
        }

        public int Score
        {
            get;
            set;
        }

        public int Life
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
