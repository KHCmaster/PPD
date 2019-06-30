using PPDFramework.Web;

namespace PPDSingle
{
    class ActiveScoreSelectedSongInfo : SelectedSongInfo
    {
        public ActiveScoreSelectedSongInfo(WebSongInformation activeScore)
            : base(activeScore.GetSongInformation())
        {
            ActiveScore = activeScore;
        }

        public void UpdateSongInfo()
        {
            SongInfo = ActiveScore.GetSongInformation();
        }

        public override bool IsFolder
        {
            get
            {
                return false;
            }
        }

        public override string Text
        {
            get
            {
                return ActiveScore.Title;
            }
        }

        public override PPDFramework.SongInformation.AvailableDifficulty Difficulty
        {
            get
            {
                return SongInfo != null ? SongInfo.Difficulty : PPDFramework.SongInformation.AvailableDifficulty.None;
            }
        }

        public WebSongInformation ActiveScore
        {
            get;
            private set;
        }
    }
}
