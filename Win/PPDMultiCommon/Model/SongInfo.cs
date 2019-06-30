using MessagePack;
using PPDFrameworkCore;

namespace PPDMultiCommon.Model
{
    [MessagePackObject]
    public class SongInfo
    {
        [Key(0)]
        public byte[] Hash
        {
            get;
            set;
        }

        [Key(1)]
        public Difficulty Difficulty
        {
            get;
            set;
        }
    }
}
