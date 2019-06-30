using PPDFramework;

namespace PPDSingle
{
    class LogicSelectedSongInfo : SelectedSongInfo
    {
        public LogicFolderInfomation LogicFolderInfomation
        {
            get;
            private set;
        }

        public LogicSelectedSongInfo(LogicFolderInfomation logicInfo)
            : base(SongInformation.FindSongInformationByID(logicInfo.ScoreID))
        {
            LogicFolderInfomation = logicInfo;
        }

        public override string Text
        {
            get
            {
                return LogicFolderInfomation.Name;
            }
        }

        public override bool IsFolder
        {
            get
            {
                return LogicFolderInfomation.IsFolder;
            }
        }

        public override SongInformation.AvailableDifficulty Difficulty
        {
            get
            {
                return SongInfo != null ? base.Difficulty : SongInformation.AvailableDifficulty.None;
            }
        }
    }
}
