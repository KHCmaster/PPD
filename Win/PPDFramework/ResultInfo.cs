using PPDFrameworkCore;
using System;
using System.Collections.Generic;

namespace PPDFramework
{
    class ResultInfoComparer : IComparer<ResultInfo>
    {
        static ResultInfoComparer comparer = new ResultInfoComparer();
        public static ResultInfoComparer Comparer
        {
            get
            {
                return comparer;
            }
        }
        #region IComparer<ResultInfo> メンバ

        public int Compare(ResultInfo x, ResultInfo y)
        {
            return DateTime.Compare(x.Date, y.Date);
        }

        #endregion
    }

    /// <summary>
    /// リザルトのクラス
    /// </summary>
    public class ResultInfo
    {
        /// <summary>
        /// ID
        /// </summary>
        public int ID
        {
            get;
            internal set;
        }

        /// <summary>
        /// 譜面ID
        /// </summary>
        public int ScoreID
        {
            get;
            internal set;
        }

        /// <summary>
        /// 難易度
        /// </summary>
        public Difficulty Difficulty
        {
            get;
            internal set;
        }

        /// <summary>
        /// 評価
        /// </summary>
        public ResultEvaluateType ResultEvaluate
        {
            get;
            internal set;
        }

        /// <summary>
        /// スコア
        /// </summary>
        public int Score
        {
            get;
            internal set;
        }

        /// <summary>
        /// Coolの数
        /// </summary>
        public int CoolCount
        {
            get;
            internal set;
        }

        /// <summary>
        /// Goodの数
        /// </summary>
        public int GoodCount
        {
            get;
            internal set;
        }

        /// <summary>
        /// Safeの数
        /// </summary>
        public int SafeCount
        {
            get;
            internal set;
        }

        /// <summary>
        /// Sadの数
        /// </summary>
        public int SadCount
        {
            get;
            internal set;
        }

        /// <summary>
        /// Worstの数
        /// </summary>
        public int WorstCount
        {
            get;
            internal set;
        }

        /// <summary>
        /// 最大コンボ
        /// </summary>
        public int MaxCombo
        {
            get;
            internal set;
        }

        /// <summary>
        /// 終了時間
        /// </summary>
        public float FinishTime
        {
            get;
            internal set;
        }

        /// <summary>
        /// 日付
        /// </summary>
        public DateTime Date
        {
            get;
            internal set;
        }

        /// <summary>
        /// SongInformationからリザルトを取得します
        /// </summary>
        /// <param name="songInfo"></param>
        /// <returns></returns>
        public static ResultInfo[] GetInfoFromSongInformation(SongInformation songInfo)
        {
            if (songInfo == null || !songInfo.IsPPDSong) return new ResultInfo[0];
            return PPDDatabase.DB.GetResults(songInfo.ID);
        }

        /// <summary>
        /// リザルトを削除します。
        /// </summary>
        public void Delete()
        {
            PPDDatabase.DB.DeleteResult(ID);
        }
    }
}
