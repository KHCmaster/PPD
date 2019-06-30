using SharpDX;


namespace PPDFramework
{
    /// <summary>
    /// 描画可能インターフェース
    /// </summary>
    public interface IDrawable
    {
        /// <summary>
        /// 位置
        /// </summary>
        Vector2 Position { get; set; }

        /// <summary>
        /// アルファ
        /// </summary>
        float Alpha { get; set; }

        /// <summary>
        /// 描画するか
        /// </summary>
        bool Hidden { get; set; }

        /// <summary>
        /// 描画する
        /// </summary>
        void Draw();
    }
}
