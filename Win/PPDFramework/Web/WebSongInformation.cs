using System.Collections.Generic;

namespace PPDFramework.Web
{
    /// <summary>
    /// Webに登録されている譜面の情報です。
    /// </summary>
    public class WebSongInformation
    {
        private List<WebSongInformationDifficulty> difficulties;

        /// <summary>
        /// 譜面名を取得します。
        /// </summary>
        public string Title
        {
            get;
            private set;
        }

        /// <summary>
        /// 譜面のハッシュを取得します。
        /// </summary>
        public string Hash
        {
            get;
            private set;
        }

        /// <summary>
        /// 動画のURLを取得します。
        /// </summary>
        public string MovieUrl
        {
            get;
            private set;
        }

        /// <summary>
        /// リビジョンを取得します。
        /// </summary>
        public int Revision
        {
            get;
            private set;
        }

        /// <summary>
        /// アップデート可能かどうかを取得します。
        /// </summary>
        public bool CanUpdate
        {
            get;
            private set;
        }

        /// <summary>
        /// 新しい譜面かどうかを取得します。
        /// </summary>
        public bool IsNew
        {
            get;
            private set;
        }

        /// <summary>
        /// 難易度を全て取得します。
        /// </summary>
        public WebSongInformationDifficulty[] Difficulties
        {
            get
            {
                return difficulties.ToArray();
            }
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="title"></param>
        /// <param name="hash"></param>
        /// <param name="movieUrl"></param>
        /// <param name="revision"></param>
        public WebSongInformation(string title, string hash, string movieUrl, int revision)
        {
            difficulties = new List<WebSongInformationDifficulty>();
            Title = title;
            Hash = hash;
            MovieUrl = movieUrl;
            Revision = revision;
        }

        /// <summary>
        /// 難易度を追加します。
        /// </summary>
        /// <param name="difficulty"></param>
        public void AddDifficulty(WebSongInformationDifficulty difficulty)
        {
            difficulties.Add(difficulty);
        }

        /// <summary>
        /// 難易度の追加を終了したときに使用します。
        /// </summary>
        public void CheckNewOrCanUpdate()
        {
            IsNew = true;
            CanUpdate = false;
            foreach (WebSongInformationDifficulty difficulty in difficulties)
            {
                var songInfo = SongInformation.FindSongInformationByHash(difficulty.HashAsBytes);
                if (songInfo != null)
                {
                    IsNew = false;
                    CanUpdate |= difficulty.Revision < Revision;
                }
            }
        }

        /// <summary>
        /// 対応するSongInformationを取得します。
        /// </summary>
        /// <returns>対応するSongInformation。</returns>
        public SongInformation GetSongInformation()
        {
            foreach (WebSongInformationDifficulty difficulty in difficulties)
            {
                var songInfo = SongInformation.FindSongInformationByHash(difficulty.HashAsBytes);
                if (songInfo != null)
                {
                    if (difficulty.Revision <= Revision)
                    {
                        return songInfo;
                    }
                }
            }

            return null;
        }
    }
}
