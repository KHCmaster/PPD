using MessagePack;
using PPDMultiCommon.Data;
using System;

namespace PPDMultiCommon.Model
{
    [MessagePackObject]
    public class GoToMenuNetworkData : NetworkData
    {
        [Key(2)]
        public Tuple<int, Result>[] Results
        {
            get;
            set;
        }

        public GoToMenuNetworkData()
        {
            MethodType = MethodType.GoToMenu;
        }
    }
}
