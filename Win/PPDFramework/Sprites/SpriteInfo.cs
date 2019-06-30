using System;

namespace PPDFramework.Sprites
{
    /// <summary>
    /// スプライト情報のクラスです。
    /// </summary>
    public class SpriteInfo
    {
        /// <summary>
        /// パスを取得します。
        /// </summary>
        public string Path
        {
            get;
            private set;
        }

        /// <summary>
        /// 幅を取得します。
        /// </summary>
        public int Width
        {
            get;
            private set;
        }

        /// <summary>
        /// 全体としてのスペースの幅を取得します。
        /// </summary>
        public int SpaceWidth
        {
            get;
            private set;
        }

        /// <summary>
        /// 高さを取得します。
        /// </summary>
        public int Height
        {
            get;
            private set;
        }

        /// <summary>
        /// 全体としてのスペースの高さを取得します。
        /// </summary>
        public int SpaceHeight
        {
            get;
            private set;
        }

        /// <summary>
        /// X座標を取得します。
        /// </summary>
        public int X
        {
            get;
            private set;
        }

        /// <summary>
        /// Y座標を取得します。
        /// </summary>
        public int Y
        {
            get;
            private set;
        }

        /// <summary>
        /// 最後に書き込んだ日時を取得します。
        /// </summary>
        public DateTime LastWriteTime
        {
            get;
            private set;
        }

        /// <summary>
        /// イメージのスケールを取得します。
        /// </summary>
        public ImageScale ImageScale
        {
            get;
            private set;
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="spaceWidth"></param>
        /// <param name="spaceHeight"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="lastWriteTime"></param>
        public SpriteInfo(string path, int width, int height, int spaceWidth, int spaceHeight, int x, int y, DateTime lastWriteTime)
        {
            Path = path;
            Width = width;
            SpaceWidth = spaceWidth;
            Height = height;
            SpaceHeight = spaceHeight;
            X = x;
            Y = y;
            LastWriteTime = lastWriteTime;
        }
    }
}
