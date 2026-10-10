using System;

namespace PPDFramework.Sprites
{
    /// <summary>
    /// スプライト情報のクラスです。
    /// </summary>
    /// <remarks>
    /// コンストラクタです。
    /// </remarks>
    /// <param name="path"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="spaceWidth"></param>
    /// <param name="spaceHeight"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="lastWriteTime"></param>
    public class SpriteInfo(string path, int width, int height, int spaceWidth, int spaceHeight, int x, int y, DateTime lastWriteTime)
    {
        /// <summary>
        /// パスを取得します。
        /// </summary>
        public string Path
        {
            get;
            private set;
        } = path;

        /// <summary>
        /// 幅を取得します。
        /// </summary>
        public int Width
        {
            get;
            private set;
        } = width;

        /// <summary>
        /// 全体としてのスペースの幅を取得します。
        /// </summary>
        public int SpaceWidth
        {
            get;
            private set;
        } = spaceWidth;

        /// <summary>
        /// 高さを取得します。
        /// </summary>
        public int Height
        {
            get;
            private set;
        } = height;

        /// <summary>
        /// 全体としてのスペースの高さを取得します。
        /// </summary>
        public int SpaceHeight
        {
            get;
            private set;
        } = spaceHeight;

        /// <summary>
        /// X座標を取得します。
        /// </summary>
        public int X
        {
            get;
            private set;
        } = x;

        /// <summary>
        /// Y座標を取得します。
        /// </summary>
        public int Y
        {
            get;
            private set;
        } = y;

        /// <summary>
        /// 最後に書き込んだ日時を取得します。
        /// </summary>
        public DateTime LastWriteTime
        {
            get;
            private set;
        } = lastWriteTime;

        /// <summary>
        /// イメージのスケールを取得します。
        /// </summary>
        public ImageScale ImageScale
        {
            get;
            private set;
        }
    }
}
