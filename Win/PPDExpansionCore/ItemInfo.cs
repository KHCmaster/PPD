using PPDMultiCommon.Data;

namespace PPDExpansionCore
{
    public class ItemInfo : PackableBase
    {
        public override DataType DataType
        {
            get { return DataType.ItemInfo; }
        }

        public float CurrentTime
        {
            get;
            set;
        }

        public ItemType ItemType
        {
            get;
            set;
        }

        public int PlayerId
        {
            get;
            set;
        }
    }
}
