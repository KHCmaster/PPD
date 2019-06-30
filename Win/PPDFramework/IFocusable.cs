namespace PPDFramework
{
    /// <summary>
    /// フォーカス可能なインターフェースです
    /// </summary>
    public interface IFocusable
    {
        /// <summary>
        /// フォーカスマネージャーを取得、設定します
        /// </summary>
        FocusManager FocusManager { get; set; }
        /// <summary>
        /// フォーカスされたかどうかを取得、設定します
        /// </summary>
        bool Focused { get; set; }
        /// <summary>
        /// フォーカススタックにあるかどうかを取得、設定します
        /// </summary>
        bool OverFocused { get; set; }
        /// <summary>
        /// オーバーフォーカス時に入力を受け取るかどうかを取得、設定します
        /// </summary>
        bool HandleOverFocusInput { get; set; }
        /// <summary>
        /// 入力を処理します
        /// </summary>
        /// <param name="inputInfo"></param>
        bool ProcessInput(InputInfoBase inputInfo);
    }
}
