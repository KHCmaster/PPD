using PPDFramework;
using PPDMultiCommon.Model;

namespace PPDMulti
{
    class SongInfoEx : SongInfo
    {
        public SongInformation SongInformation
        {
            get
            {
                return SongInformation.FindSongInformationByHash(Hash, Difficulty);
            }
        }
    }
}
