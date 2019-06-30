using PPDFramework;
using PPDFrameworkCore;
using System.Linq;

namespace PPDSingle
{
    class SelectedSongInfo
    {
        private bool[] perfectTrials = new bool[4];

        public SelectedSongInfo(SongInformation songInformation)
        {
            SongInfo = songInformation;
            UpdatePerfectTrialInfo();
        }

        public void UpdatePerfectTrialInfo()
        {
            if (SongInfo != null && SongInfo.IsPPDSong)
            {
                int totalCount = 0;
                for (int i = 0; i < 4; i++)
                {
                    if (SongInfo.Difficulty.HasFlag((SongInformation.AvailableDifficulty)(1 << i)))
                    {
                        var str = CryptographyUtility.Getx2Encoding(SongInfo.GetScoreHash((PPDFrameworkCore.Difficulty)i));
                        perfectTrials[i] = PerfectTrialCache.Instance.IsPerfect(str);
                        totalCount++;
                    }
                }
                if (totalCount == 0)
                {
                    PerfectRatio = 0;
                }
                else
                {
                    PerfectRatio = perfectTrials.Count(p => p) / (float)totalCount;
                }
            }
        }

        public SongInformation SongInfo
        {
            get;
            protected set;
        }

        public virtual string Text
        {
            get
            {
                if (SongInfo != null)
                {
                    return SongInfo.DirectoryName;
                }
                return "";
            }
        }

        public virtual PPDFramework.SongInformation.AvailableDifficulty Difficulty
        {
            get
            {
                return SongInfo.Difficulty;
            }
        }

        public virtual bool IsFolder
        {
            get
            {
                return !SongInfo.IsPPDSong;
            }
        }

        public float PerfectRatio
        {
            get;
            private set;
        }

        public bool[] Perfects
        {
            get
            {
                return perfectTrials;
            }
        }
    }
}
