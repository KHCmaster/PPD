using PPDMultiCommon.Data;
using System;

namespace PPDExpansion.Model
{
    class Item
    {
        public ItemType ItemType
        {
            get;
            private set;
        }

        public string UserName
        {
            get;
            private set;
        }

        public TimeSpan TimeSpan
        {
            get;
            private set;
        }

        public Item(ItemType itemType, string userName, TimeSpan timeSpan)
        {
            ItemType = itemType;
            UserName = userName;
            TimeSpan = timeSpan;
        }
    }
}
