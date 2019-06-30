namespace PPDExpansionCore
{
    public class PlayerInfo : PackableBase
    {
        public override DataType DataType
        {
            get { return DataType.PlayerInfo; }
        }

        public bool IsSelf
        {
            get;
            set;
        }

        public int PlayerId
        {
            get;
            set;
        }

        public string UserName
        {
            get;
            set;
        }

        public string AcccountId
        {
            get;
            set;
        }

        public byte R
        {
            get;
            set;
        }

        public byte G
        {
            get;
            set;
        }

        public byte B
        {
            get;
            set;
        }
    }
}
