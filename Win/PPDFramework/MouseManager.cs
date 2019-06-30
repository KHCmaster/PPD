using SharpDX;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PPDFramework
{
    /// <summary>
    /// マウスマネージャー
    /// </summary>
    public class MouseManager
    {
        private Control control;
        private Vector2 offset;
        private Vector2 scale;
        private List<MouseEvent> mouseEvents;
        private Queue<MouseInfo> mouseInfos;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="control"></param>
        /// <param name="offset"></param>
        /// <param name="scale"></param>
        public MouseManager(Control control, Vector2 offset, Vector2 scale)
        {
            this.control = control;
            this.offset = offset;
            this.scale = scale;
            mouseEvents = new List<MouseEvent>();
            mouseInfos = new Queue<MouseInfo>();
            control.MouseDown += control_MouseDown;
            control.MouseUp += control_MouseUp;
            control.MouseWheel += control_MouseWheel;
        }

        void control_MouseWheel(object sender, MouseEventArgs e)
        {
            mouseEvents.Add(new MouseEvent(ConvertToVector2(e.X, e.Y), e.Delta, MouseEvent.MouseEventType.Wheel));
        }

        void control_MouseUp(object sender, MouseEventArgs e)
        {
            mouseEvents.Add(new MouseEvent(ConvertToVector2(e.X, e.Y), true, ConvertToEventType(e.Button)));
        }

        void control_MouseDown(object sender, MouseEventArgs e)
        {
            mouseEvents.Add(new MouseEvent(ConvertToVector2(e.X, e.Y), false, ConvertToEventType(e.Button)));
        }

        private Vector2 ConvertToVector2(float x, float y)
        {
            return new Vector2((x - offset.X) / scale.X, (y - offset.Y) / scale.Y);
        }

        private MouseEvent.MouseEventType ConvertToEventType(MouseButtons buttons)
        {
            if ((buttons & MouseButtons.Left) == MouseButtons.Left)
            {
                return MouseEvent.MouseEventType.Left;
            }
            if ((buttons & MouseButtons.Right) == MouseButtons.Right)
            {
                return MouseEvent.MouseEventType.Right;
            }
            if ((buttons & MouseButtons.Middle) == MouseButtons.Middle)
            {
                return MouseEvent.MouseEventType.Middle;
            }
            return MouseEvent.MouseEventType.None;
        }

        /// <summary>
        /// マウス入力をポーリングします
        /// </summary>
        /// <returns></returns>
        public MouseInfo GetMouseEvents()
        {
            control.Invoke(() =>
            {
                var p = control.PointToClient(Cursor.Position);
                var mouseInfo = new MouseInfo(ConvertToVector2(p.X, p.Y), mouseEvents.ToArray());
                mouseEvents.Clear();
                lock (mouseInfos)
                {
                    mouseInfos.Enqueue(mouseInfo);
                }
            });
            lock (mouseInfos)
            {
                if (mouseInfos.Count == 0)
                {
                    return MouseInfo.Empty;
                }
                return mouseInfos.Dequeue();
            }
        }
    }

    /// <summary>
    /// マウスイベント
    /// </summary>
    public class MouseEvent
    {
        Vector2 position;
        int wheelValue;
        bool up;
        MouseEventType eventType;

        /// <summary>
        /// マウスイベントのタイプ
        /// </summary>
        public enum MouseEventType
        {
            /// <summary>
            /// None
            /// </summary>
            None = 0,
            /// <summary>
            /// 左ボタン
            /// </summary>
            Left,
            /// <summary>
            /// 右ボタン
            /// </summary>
            Right,
            /// <summary>
            /// 中ボタン
            /// </summary>
            Middle,
            /// <summary>
            /// ホイール
            /// </summary>
            Wheel
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="position"></param>
        /// <param name="up"></param>
        /// <param name="eventType"></param>
        public MouseEvent(Vector2 position, bool up, MouseEventType eventType)
        {
            InnerStruct(position, 0, up, eventType);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="position"></param>
        /// <param name="wheelValue"></param>
        /// <param name="eventType"></param>
        public MouseEvent(Vector2 position, int wheelValue, MouseEventType eventType)
        {
            InnerStruct(position, wheelValue, false, eventType);
        }

        private void InnerStruct(Vector2 position, int wheelValue, bool up, MouseEventType eventType)
        {
            this.position = position;
            this.wheelValue = wheelValue;
            this.up = up;
            this.eventType = eventType;
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
        /// ホイール量
        /// </summary>
        public float WheelValue
        {
            get
            {
                return wheelValue;
            }
        }

        /// <summary>
        /// 押上かどうか
        /// </summary>
        public bool Up
        {
            get
            {
                return up;
            }
        }

        /// <summary>
        /// イベントのタイプ
        /// </summary>
        public MouseEventType EventType
        {
            get
            {
                return eventType;
            }
        }
    }
}
