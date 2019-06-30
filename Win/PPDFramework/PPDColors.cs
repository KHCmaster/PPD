using SharpDX;

namespace PPDFramework
{
    /// <summary>
    /// PPDの色です。
    /// </summary>
    public static class PPDColors
    {
        /// <summary>
        /// 透明
        /// </summary>
        public static readonly Color4 Transparent = new Color4(0, 0, 0, 0);

        /// <summary>
        /// 黒
        /// </summary>
        public static readonly Color4 Black = new Color4(0, 0, 0, 1);

        /// <summary>
        /// 白
        /// </summary>
        public static readonly Color4 White = new Color4(1, 1, 1, 1);

        /// <summary>
        /// 緑
        /// </summary>
        public static readonly Color4 Green = new Color4(0, 1, 0, 1);

        /// <summary>
        /// 赤
        /// </summary>
        public static readonly Color4 Red = new Color4(1, 0, 0, 1);

        /// <summary>
        /// 青
        /// </summary>
        public static readonly Color4 Blue = new Color4(0, 0, 1, 1);

        /// <summary>
        /// グレー
        /// </summary>
        public static readonly Color4 Gray = new Color4(0.5f, 0.5f, 0.5f, 1);

        /// <summary>
        /// 選択
        /// </summary>
        public static readonly Color4 Selection = new Color4(0.61f, 0.91f, 0.99f, 1);

        /// <summary>
        /// アクティブ
        /// </summary>
        public static readonly Color4 Active = new Color4(0.14f, 0.62f, 0.9f, 1);

        /// <summary>
        /// ライフゲージ
        /// </summary>
        public static readonly Color4 LifeGage = new Color4(49 / 255f, 224 / 255f, 61 / 255f, 1);
    }
}
