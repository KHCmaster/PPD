namespace PPDFramework.Web
{
    /// <summary>
    /// エラーの理由です
    /// </summary>
    public enum ErrorReason
    {
        /// <summary>
        /// 成功
        /// </summary>
        OK = 0,
        /// <summary>
        /// 認証失敗
        /// </summary>
        AuthFailed = 1,
        /// <summary>
        /// スコアがなかった
        /// </summary>
        ScoreNotFound = 2,
        /// <summary>
        /// 検証失敗
        /// </summary>
        ValidateFailed = 3,
        /// <summary>
        /// 引数がおかしい
        /// </summary>
        ArgumentError = 4,
        /// <summary>
        /// 最新バージョンの不一致
        /// </summary>
        VersionUnmatch = 5,
        /// <summary>
        /// 時間が短い
        /// </summary>
        InvalidPlayTime = 6,
        /// <summary>
        /// 対象の譜面ではない
        /// </summary>
        NotAvailableScore = 7,
        /// <summary>
        /// すでにクリアしている
        /// </summary>
        AlreadyCleared = 8,
        /// <summary>
        /// お金が足りない
        /// </summary>
        LackOfMoney = 9,
        /// <summary>
        /// ネットワークのエラー
        /// </summary>
        NetworkError = 255
    }
}
