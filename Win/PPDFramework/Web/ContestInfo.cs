using PPDFrameworkCore;
using System;

namespace PPDFramework.Web
{
    /// <summary>
    /// コンテストの情報を表すクラスです。
    /// </summary>
    public class ContestInfo
    {
        /// <summary>
        /// コンテストのIDを取得します。
        /// </summary>
        public int Id
        {
            get;
            private set;
        }

        /// <summary>
        /// 譜面のIDを取得します。
        /// </summary>
        public string ScoreLibraryId
        {
            get;
            private set;
        }

        /// <summary>
        /// スコアのハッシュを取得します。
        /// </summary>
        public string ScoreHash
        {
            get;
            private set;
        }

        /// <summary>
        /// 難易度を取得します。
        /// </summary>
        public Difficulty Difficulty
        {
            get;
            private set;
        }

        /// <summary>
        /// 開始時間を取得します。
        /// </summary>
        public DateTime StartTime
        {
            get;
            private set;
        }

        /// <summary>
        /// サーバーの現在の時間を取得します。
        /// </summary>
        public DateTime CurrentTime
        {
            get;
            private set;
        }

        /// <summary>
        /// 終了時間を取得します。
        /// </summary>
        public DateTime EndTime
        {
            get;
            private set;
        }

        /// <summary>
        /// 譜面名を取得します。
        /// </summary>
        public string Title
        {
            get;
            private set;
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="id">コンテストのID。</param>
        /// <param name="scoreLibraryId">譜面のID。</param>
        /// <param name="scoreHash">譜面のハッシュ。</param>
        /// <param name="difficulty">難易度。</param>
        /// <param name="startTime">開始時間。</param>
        /// <param name="currentTime">サーバーの現在の時間。</param>
        /// <param name="endTime">終了時間。</param>
        /// <param name="title">タイトル。</param>
        public ContestInfo(int id, string scoreLibraryId, string scoreHash, Difficulty difficulty, DateTime startTime, DateTime currentTime, DateTime endTime, string title)
        {
            Id = id;
            ScoreLibraryId = scoreLibraryId;
            ScoreHash = scoreHash;
            Difficulty = difficulty;
            StartTime = startTime;
            CurrentTime = currentTime;
            EndTime = endTime;
            Title = title;
        }
    }
}
