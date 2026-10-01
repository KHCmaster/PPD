using PPDMultiCommon.Data;
using System;

namespace PPDExpansion.Model
{
    class Item(ItemType itemType, string userName, TimeSpan timeSpan)
    {
        public ItemType ItemType
        {
            get;
            private set;
        } = itemType;

        public string UserName
        {
            get;
            private set;
        } = userName;

        public TimeSpan TimeSpan
        {
            get;
            private set;
        } = timeSpan;
    }
}
