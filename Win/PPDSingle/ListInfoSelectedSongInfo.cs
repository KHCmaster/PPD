using PPDFramework.Web;

namespace PPDSingle
{
    class ListInfoSelectedSongInfo : SelectedSongInfo
    {
        public ListInfoSelectedSongInfo(ListInfo listInfo)
            : base(null)
        {
            ListInfo = listInfo;
        }

        public ListInfoSelectedSongInfo(ListScoreInfo scoreInfo)
            : base(null)
        {
            ListScoreInfo = scoreInfo;
            UpdateSongInfo();
        }

        public void UpdateSongInfo()
        {
            if (ListScoreInfo == null)
            {
                return;
            }

            var webSongInformation = WebSongInformationManager.Instance.FindById(ListScoreInfo.ScoreId);
            if (webSongInformation != null)
            {
                SongInfo = webSongInformation.GetSongInformation();
            }
        }

        public override bool IsFolder
        {
            get
            {
                if (ListInfo != null)
                {
                    return true;
                }
                return false;
            }
        }

        public override string Text
        {
            get
            {
                if (ListInfo != null)
                {
                    return ListInfo.Title;
                }
                if (ListScoreInfo != null)
                {
                    return ListScoreInfo.Title;
                }
                return base.Text;
            }
        }

        public ListInfo ListInfo
        {
            get;
            private set;
        }

        public ListScoreInfo ListScoreInfo
        {
            get;
            private set;
        }
    }
}
