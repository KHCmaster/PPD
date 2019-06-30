namespace PPDFramework.Shaders
{
    /// <summary>
    /// カラーフィルターのタイプです。
    /// </summary>
    public enum ColorFilterType
    {
        /// <summary>
        /// なし
        /// </summary>
        None = -1,
        /// <summary>
        /// カラー
        /// </summary>
        Color = 0,
        /// <summary>
        /// 最大グレースケール
        /// </summary>
        MaxGrayScale = 1,
        /// <summary>
        /// 中間値グレースケール
        /// </summary>
        MiddleGrayScale = 2,
        /// <summary>
        /// NTSCグレースケール
        /// </summary>
        NTSCGrayScale = 3,
        /// <summary>
        /// HDTVグレースケール
        /// </summary>
        HDTVGrayScale = 4,
        /// <summary>
        /// 平均値グレースケール
        /// </summary>
        AverageGrayScale = 5,
        /// <summary>
        /// グリーングレースケール
        /// </summary>
        GreenGrayScale = 6,
        /// <summary>
        /// 中央値グレースケール
        /// </summary>
        MedianGrayScale = 7,
        /// <summary>
        /// 色相
        /// </summary>
        Hue = 8,
        /// <summary>
        /// 再度
        /// </summary>
        Saturation = 9,
        /// <summary>
        /// 輝度
        /// </summary>
        Brightness = 10,
        /// <summary>
        /// 反転
        /// </summary>
        Invert = 11
    }
}
