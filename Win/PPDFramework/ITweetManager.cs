using System;

namespace PPDFramework
{
    /// <summary>
    /// 成功かどうかつきイベント
    /// </summary>
    /// <param name="ok"></param>
    public delegate void BoolEventHandler(bool ok);
    /// <summary>
    /// 結果をツイートするマネージャークラスです。
    /// </summary>
    public interface ITweetManager
    {
        /// <summary>
        /// ツイートできるかどうか
        /// </summary>
        bool CanTweet { get; }
        /// <summary>
        /// ツイートできる場合のツイートするテキスト
        /// </summary>
        string TweetText { get; }
        /// <summary>
        /// 終了した日時
        /// </summary>
        DateTime FinishDate { get; }
        /// <summary>
        /// ツイートする場合の追加の画像のパス
        /// </summary>
        string TweetFilePath { get; set; }
        /// <summary>
        /// ツイートが完了した
        /// </summary>
        event BoolEventHandler TweetFinished;
        /// <summary>
        /// ツイートする
        /// </summary>
        void Tweet();
    }
}
