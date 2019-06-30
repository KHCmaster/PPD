using SharpDX;

namespace PPDFramework
{
    /// <summary>
    /// マウス入力情報
    /// </summary>
    public class MouseInfo
    {
        Vector2 position;
        MouseEvent[] events;
        static MouseInfo empty = new MouseInfo(Vector2.Zero, new MouseEvent[0]);

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="position"></param>
        /// <param name="events"></param>
        public MouseInfo(Vector2 position, MouseEvent[] events)
        {
            this.position = position;
            this.events = events;
        }

        /// <summary>
        /// 位置
        /// </summary>
        public Vector2 Position
        {
            get
            {
                return position;
            }
        }

        /// <summary>
        /// イベント
        /// </summary>
        public MouseEvent[] Events
        {
            get
            {
                return events;
            }
        }

        /// <summary>
        /// 空のMouseInfoです
        /// </summary>
        public static MouseInfo Empty
        {
            get
            {
                return empty;
            }
        }
    }
}
