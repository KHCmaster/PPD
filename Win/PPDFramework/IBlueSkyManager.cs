using System;

namespace PPDFramework
{
    /// <summary>
    /// 成功かどうかつきイベント
    /// </summary>
    /// <param name="ok"></param>
    public delegate void BoolEventHandler(bool ok);
    /// <summary>
    /// 結果を投稿するマネージャークラスです。
    /// </summary>
    public interface IBlueSkyManager
    {
        /// <summary>
        /// 投稿できるかどうか
        /// </summary>
        bool CanPost { get; }
        /// <summary>
        /// 投稿できる場合のツイートするテキスト
        /// </summary>
        string PostText { get; }
        /// <summary>
        /// 終了した日時
        /// </summary>
        DateTime FinishDate { get; }
        /// <summary>
        /// 投稿する場合の追加の画像のパス
        /// </summary>
        string PostFilePath { get; set; }
        /// <summary>
        /// 投稿が完了した
        /// </summary>
        event BoolEventHandler PostFinished;
        /// <summary>
        /// 投稿する
        /// </summary>
        void Post();
    }
}
