using PPDMath;
using PPDFramework;
using PPDFramework.Resource;
using PPDShareComponent;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;
using PointF = System.Drawing.PointF;
using RectangleF = System.Drawing.RectangleF;
using System.Diagnostics;

namespace BezierDrawControl
{
    /// <summary>
    /// DirectX で描画するベジエ曲線の編集領域です（WPF）。
    /// </summary>
    /// <remarks>
    /// 表示しないウィンドウで描画し、バックバッファーを読み出して表示します。
    /// 大きさはピクセル数で指定し、WPF の単位（1/96 インチ）でも同じ数値で表示します（以前の DPI 非対応のときと同じ見た目の大きさ）。
    /// </remarks>
    public class DXBezierControl : FrameworkElement, IDisposable
    {
        public class BezierDrawer : GameCoreBase
        {
            SpriteObject spriteObject;
            readonly RectangleComponent back;

            public PPDDevice Device
            {
                get { return device; }
            }

            public ResourceManager ResourceManager
            {
                get;
                private set;
            }

            public SpriteObject DrawTarget
            {
                get;
                private set;
            }

            public SpriteObject Background
            {
                get;
                private set;
            }

            internal BezierDrawer(IntPtr windowHandle, int width, int height) : base(PPDExecuteArg.Empty, windowHandle)
            {
                InitializeDirectX(width, height, width, height, 1);
                Initialize();
                ResourceManager = new ResourceManager();
                spriteObject = new SpriteObject(device);
                spriteObject.AddChild(DrawTarget = new SpriteObject(device));
                spriteObject.AddChild(Background = new SpriteObject(device));
                spriteObject.AddChild(back = new RectangleComponent(device, ResourceManager, PPDColors.White)
                {
                    RectangleHeight = height,
                    RectangleWidth = width
                });
            }

            protected override void Update()
            {
                spriteObject.Update();
            }

            protected override void Draw()
            {
                spriteObject.Draw();
            }

            protected override void DisposeResource()
            {
                spriteObject?.Dispose();
                spriteObject = null;
                ResourceManager?.Dispose();
                ResourceManager = null;
                base.DisposeResource();
            }
        }

        public class Context(DXBezierControl.BezierDrawer drawer) : IBezierDrawContext
        {
            public BezierDrawer Drawer
            {
                get;
                private set;
            } = drawer;

            public void DrawString(string text, Color color, float fontSize, PointF point)
            {
                Drawer.DrawTarget.InsertChild(new TextureString(Drawer.Device, text, (int)fontSize,
                    new Vortice.Mathematics.Color4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f))
                {
                    Position = new System.Numerics.Vector2(point.X, point.Y)
                }, 0);
            }

            public void DrawEllipse(Color color, RectangleF rect)
            {
                var center = new System.Numerics.Vector2(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
                var maxRadius = (int)Math.Ceiling(Math.Max(rect.Width / 2, rect.Height / 2));
                var points = new System.Numerics.Vector2[maxRadius * 4 + 1];
                for (var i = 0; i < points.Length; i++)
                {
                    var rad = Math.PI * 2 * i / points.Length;
                    points[i] = center + new System.Numerics.Vector2(rect.Width / 2 * (float)Math.Cos(rad), rect.Height / 2 * (float)Math.Sin(rad));
                }
                var line = new LineComponent(Drawer.Device, Drawer.ResourceManager,
                    new Vortice.Mathematics.Color4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f))
                {
                    Points = points,
                    LineWidth = 1
                };
                Drawer.DrawTarget.InsertChild(line, 0);
            }

            public void DrawLine(Color color, PointF p1, PointF p2)
            {
                var line = new LineComponent(Drawer.Device, Drawer.ResourceManager,
                    new Vortice.Mathematics.Color4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f))
                {
                    Points = [
                        new(p1.X,p1.Y),
                        new(p2.X,p2.Y)
                    ],
                    LineWidth = 1
                };
                Drawer.DrawTarget.InsertChild(line, 0);
            }

            public void DrawLines(Color color, PointF[] points)
            {
                var line = new LineComponent(Drawer.Device, Drawer.ResourceManager,
                    new Vortice.Mathematics.Color4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f))
                {
                    Points = [.. points.Select(p => new System.Numerics.Vector2(p.X, p.Y))],
                    LineWidth = 1
                };
                Drawer.DrawTarget.InsertChild(line, 0);
            }

            public void DrawRectangle(Color color, float x, float y, float width, float height)
            {
                var lineRect = new LineRectangleComponent(Drawer.Device, Drawer.ResourceManager,
                    new Vortice.Mathematics.Color4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f))
                {
                    Position = new System.Numerics.Vector2(x, y),
                    RectangleWidth = width,
                    RectangleHeight = height,
                    BorderThickness = 1
                };
                Drawer.DrawTarget.InsertChild(lineRect, 0);
            }

            public void FillRectangle(Color color, float x, float y, float width, float height)
            {
                var rect = new RectangleComponent(Drawer.Device, Drawer.ResourceManager,
                    new Vortice.Mathematics.Color4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f))
                {
                    Position = new System.Numerics.Vector2(x, y),
                    RectangleWidth = width,
                    RectangleHeight = height
                };
                Drawer.DrawTarget.InsertChild(rect, 0);
            }
        }

        HiddenRenderWindow renderWindow;
        WriteableBitmap bitmap;
        bool renderScheduled;
        TimeSpan lastRenderingTime = TimeSpan.MinValue;

        public BezierDrawer Drawer
        {
            get;
            private set;
        }

        public BezierController Controller
        {
            get;
            private set;
        }

        /// <summary>
        /// 描画領域の幅（ピクセル）を取得します。
        /// </summary>
        public int PixelWidth
        {
            get;
            private set;
        }

        /// <summary>
        /// 描画領域の高さ（ピクセル）を取得します。
        /// </summary>
        public int PixelHeight
        {
            get;
            private set;
        }

        /// <summary>
        /// 描画領域の大きさが変わったときに発生します。
        /// </summary>
        public event EventHandler PixelSizeChanged;

        public DXBezierControl()
        {
            Controller = new BezierController();
            Controller.DrawRequired += Controller_DrawRequired;
            Focusable = true;
        }

        /// <summary>
        /// 指定した大きさで描画の準備をします。前の描画の資源は破棄します。
        /// </summary>
        public void Initialize(int width, int height)
        {
            DisposeDrawer();
            PixelWidth = width;
            PixelHeight = height;
            renderWindow = new HiddenRenderWindow(width, height);
            Drawer = new BezierDrawer(renderWindow.Handle, width, height);
            bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr32, null);
            Width = width;
            Height = height;
            PixelSizeChanged?.Invoke(this, EventArgs.Empty);
            InvalidateVisual();
        }

        private void Controller_DrawRequired()
        {
            DrawAndRefresh();
        }

        /// <summary>
        /// 描き直しを予約します。次に画面を更新する直前に 1 回だけ描きます。
        /// </summary>
        /// <remarks>
        /// ドラッグ中はマウスが動くたびに呼ばれるので、そのたびに描くと（描画とバックバッファーの読み出しで GPU を待つため）
        /// マウスのメッセージが溜まって、表示が遅れてついてくるようになる。
        /// </remarks>
        public void DrawAndRefresh()
        {
            if (Drawer == null || renderScheduled)
            {
                return;
            }
            renderScheduled = true;
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            // Rendering は WPF が描画するたびに呼ばれ、同じフレームの中で何回も来ることがある（ビットマップを更新するとまた来る）。
            // フレームの時刻が前に描いたときと同じなら描かずに、次のフレームを待つ
            var renderingTime = (e as RenderingEventArgs)?.RenderingTime ?? TimeSpan.Zero;
            if (renderingTime == lastRenderingTime)
            {
                return;
            }
            lastRenderingTime = renderingTime;
            CompositionTarget.Rendering -= CompositionTarget_Rendering;
            renderScheduled = false;
            Render();
        }

        private void Render()
        {
            if (Drawer == null)
            {
                return;
            }

            Drawer.DrawTarget.ClearDisposeChildren();
            Controller.Draw(new Context(Drawer));
            Drawer.RenderWithoutPresent();
            bitmap.Lock();
            try
            {
                Drawer.Device.CopyBackBuffer(bitmap.BackBuffer, bitmap.BackBufferStride);
                bitmap.AddDirtyRect(new Int32Rect(0, 0, PixelWidth, PixelHeight));
            }
            finally
            {
                bitmap.Unlock();
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (bitmap != null)
            {
                drawingContext.DrawImage(bitmap, new Rect(0, 0, ActualWidth, ActualHeight));
            }
        }

        private System.Windows.Point ToPixel(MouseEventArgs e)
        {
            var pos = e.GetPosition(this);
            if (ActualWidth <= 0 || ActualHeight <= 0)
            {
                return pos;
            }
            return new System.Windows.Point(pos.X * PixelWidth / ActualWidth, pos.Y * PixelHeight / ActualHeight);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            if (e.ChangedButton == MouseButton.Left)
            {
                CaptureMouse();
            }
            var pos = ToPixel(e);
            Controller.MouseDown((float)pos.X, (float)pos.Y, e.ChangedButton == MouseButton.Left, WpfCursors.ToBezierModifierKeys(Keyboard.Modifiers));
            DrawAndRefresh();
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            if (IsMouseCaptured)
            {
                ReleaseMouseCapture();
            }
            Controller.MouseUp();
            DrawAndRefresh();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var pos = ToPixel(e);
            var cursor = Controller.MouseMove((float)pos.X, (float)pos.Y, WpfCursors.ToBezierModifierKeys(Keyboard.Modifiers));
            if (cursor.HasValue)
            {
                Cursor = WpfCursors.Get(cursor.Value);
            }
        }

        private void DisposeDrawer()
        {
            if (renderScheduled)
            {
                CompositionTarget.Rendering -= CompositionTarget_Rendering;
                renderScheduled = false;
            }
            Drawer?.Dispose();
            Drawer = null;
            renderWindow?.Dispose();
            renderWindow = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeDrawer();
            }
        }
    }
}
