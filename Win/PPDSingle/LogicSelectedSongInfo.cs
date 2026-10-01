using PPDFramework;

namespace PPDSingle
{
    class LogicSelectedSongInfo(LogicFolderInfomation logicInfo) : SelectedSongInfo(SongInformation.FindSongInformationByID(logicInfo.ScoreID))
    {
        public LogicFolderInfomation LogicFolderInfomation
        {
            get;
            private set;
        } = logicInfo;

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
