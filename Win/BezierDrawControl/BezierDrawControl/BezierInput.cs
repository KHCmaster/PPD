namespace BezierDrawControl
{
    /// <summary>
    /// 操作に使う修飾キーです（WinForms と WPF のどちらにも依存しないようにするため）。
    /// </summary>
    [System.Flags]
    public enum BezierModifierKeys
    {
        None = 0,
        Shift = 1,
        Control = 2,
        Alt = 4
    }

    /// <summary>
    /// カーソルの種類です。
    /// </summary>
    public enum BezierCursorKind
    {
        Default,
        SizeAll,
        Rotate,
        Anchor,
        AnchorPlus,
        Handle,
        /// <summary>
        /// 向きのある両矢印です（<see cref="BezierCursor.Angle"/> の向き）。
        /// </summary>
        Arrow
    }

    /// <summary>
    /// 表示するカーソルです。実際のカーソルへの変換は、WinForms 用・WPF 用のコントロールで行います。
    /// </summary>
    /// <param name="Kind">種類。</param>
    /// <param name="Angle">矢印の向き（度、0 から 179）。</param>
    public readonly record struct BezierCursor(BezierCursorKind Kind, int Angle = 0)
    {
        public static BezierCursor Default => new(BezierCursorKind.Default);
        public static BezierCursor SizeAll => new(BezierCursorKind.SizeAll);
        public static BezierCursor Rotate => new(BezierCursorKind.Rotate);
        public static BezierCursor Anchor => new(BezierCursorKind.Anchor);
        public static BezierCursor AnchorPlus => new(BezierCursorKind.AnchorPlus);
        public static BezierCursor Handle => new(BezierCursorKind.Handle);

        public static BezierCursor Arrow(int angle)
        {
            angle %= 180;
            if (angle < 0) angle += 180;
            return new BezierCursor(BezierCursorKind.Arrow, angle);
        }
    }
}
