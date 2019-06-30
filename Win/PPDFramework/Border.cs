using Effect2D;
using SharpDX;

namespace PPDFramework
{
    /// <summary>
    /// 文字列用のボーダー設定クラス
    /// </summary>
    public class Border
    {
        /// <summary>
        /// 太さ
        /// </summary>
        public float Thickness
        {
            get;
            set;
        } = 1;

        /// <summary>
        /// カラー
        /// </summary>
        public Color4 Color
        {
            get;
            set;
        } = PPDColors.Black;

        /// <summary>
        /// タイプ
        /// </summary>
        public BorderType Type
        {
            get;
            set;
        } = BorderType.Outside;

        /// <summary>
        /// ブレンド
        /// </summary>
        public BlendMode Blend
        {
            get;
            set;
        } = BlendMode.Normal;
    }
}
