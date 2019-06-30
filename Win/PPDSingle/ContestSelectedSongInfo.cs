using PPDFramework;
using PPDFramework.Web;
using PPDFrameworkCore;
using System;

namespace PPDSingle
{
    class ContestSelectedSongInfo : SelectedSongInfo
    {
        public ContestSelectedSongInfo(ContestInfo contestInfo)
            : base(SongInformation.FindSongInformationByHash(CryptographyUtility.Parsex2String(contestInfo.ScoreHash), contestInfo.Difficulty))
        {
            ContestInfo = contestInfo;
        }

        public void UpdateSongInfo()
        {
            SongInfo = SongInformation.FindSongInformationByHash(CryptographyUtility.Parsex2String(ContestInfo.ScoreHash), ContestInfo.Difficulty);
        }

        public ContestInfo ContestInfo
        {
            get;
            private set;
        }

        public override string Text
        {
            get
            {
                return String.Format("期限:{0} {1}", ContestInfo.EndTime.AddSeconds(-1).ToString("MM/dd HH:mm:ss"), ContestInfo.Title);
            }
        }

        public override bool IsFolder
        {
            get
            {
                return false;
            }
        }

        public override SongInformation.AvailableDifficulty Difficulty
        {
            get
            {
                return SongInformation.ConvertDifficulty(ContestInfo.Difficulty);
            }
        }

        public Ranking GetRanking()
        {
            var ranking = new Ranking(WebManager.Instance.GetContestRanking(), ContestInfo.Difficulty);
            return ranking;
        }
    }
}
