namespace PPDFramework
{
    /// <summary>
    /// レビューマネージャーのインスタンスです。
    /// </summary>
    public interface IReviewManager
    {
        /// <summary>
        /// レビューが完了したイベントです。
        /// </summary>
        event BoolEventHandler ReviewFinished;

        /// <summary>
        /// レビュー可能かどうかを取得します。
        /// </summary>
        bool CanReview { get; }

        /// <summary>
        /// レビューします。
        /// </summary>
        /// <param name="str">レビュー文字列。</param>
        /// <param name="rate">レート。</param>
        void Review(string str, int rate);
    }
}
