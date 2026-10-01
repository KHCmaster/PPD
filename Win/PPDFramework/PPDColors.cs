using System.Numerics;
using PPDMath;
using Color4 = Vortice.Mathematics.Color4;

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
        public static readonly Color4 Transparent = new(0, 0, 0, 0);

        /// <summary>
        /// 黒
        /// </summary>
        public static readonly Color4 Black = new(0, 0, 0, 1);

        /// <summary>
        /// 白
        /// </summary>
        public static readonly Color4 White = new(1, 1, 1, 1);

        /// <summary>
        /// 緑
        /// </summary>
        public static readonly Color4 Green = new(0, 1, 0, 1);

        /// <summary>
        /// 赤
        /// </summary>
        public static readonly Color4 Red = new(1, 0, 0, 1);

        /// <summary>
        /// 青
        /// </summary>
        public static readonly Color4 Blue = new(0, 0, 1, 1);

        /// <summary>
        /// グレー
        /// </summary>
        public static readonly Color4 Gray = new(0.5f, 0.5f, 0.5f, 1);

        /// <summary>
        /// 選択
        /// </summary>
        public static readonly Color4 Selection = new(0.61f, 0.91f, 0.99f, 1);

        /// <summary>
        /// アクティブ
        /// </summary>
        public static readonly Color4 Active = new(0.14f, 0.62f, 0.9f, 1);

        /// <summary>
        /// ライフゲージ
        /// </summary>
        public static readonly Color4 LifeGage = new(49 / 255f, 224 / 255f, 61 / 255f, 1);
    }
}
