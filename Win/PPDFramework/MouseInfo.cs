using System.Numerics;
using PPDMath;

namespace PPDFramework
{
    /// <summary>
    /// マウス入力情報
    /// </summary>
    /// <remarks>
    /// コンストラクタ
    /// </remarks>
    /// <param name="position"></param>
    /// <param name="events"></param>
    public class MouseInfo(Vector2 position, MouseEvent[] events)
    {
        Vector2 position = position;
        readonly MouseEvent[] events = events;
        static readonly MouseInfo empty = new(Vector2.Zero, []);

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
