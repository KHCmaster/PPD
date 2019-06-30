using MessagePack;
using PPDMultiCommon.Data;
using System;

namespace PPDMultiCommon.Model
{
    [MessagePackObject]
    public class GameRule
    {
        [Key(0)]
        public bool ItemAvailable { get; set; }
        [Key(1)]
        public int MaxItemCount { get; set; }
        [Key(2)]
        public int ItemSupplyComboCount { get; set; }
        [Key(3)]
        public int ItemSupplyWorstCount { get; set; }
        [Key(4)]
        public ItemSupplyType ItemSupplyType { get; set; }
        [Key(5)]
        public ResultSortType ResultSortType { get; set; }

        public GameRule()
        {
            ItemAvailable = false;
            MaxItemCount = 6;
            ItemSupplyComboCount = 100;
            ItemSupplyWorstCount = 50;
            ItemSupplyType = ItemSupplyType.ComboWorstCount;
            ResultSortType = ResultSortType.Score;
        }

        public override string ToString()
        {
            return String.Format("Item:{0}, MaxItemCount:{1}, ItemSupplyComboCount:{2}, ItemSupplyWorstCount:{3}, ItemSupplyType:{4}, ResultSortType:{5}",
                ItemAvailable, MaxItemCount, ItemSupplyComboCount, ItemSupplyWorstCount, ItemSupplyType, ResultSortType);
        }
    }
}
