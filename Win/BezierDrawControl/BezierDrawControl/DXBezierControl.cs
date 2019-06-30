using PPDFramework;
using PPDFramework.Resource;
using PPDShareComponent;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BezierDrawControl
{
    public partial class DXBezierControl : UserControl
    {
        public class BezierDrawer : GameCoreBase
        {
            SpriteObject spriteObject;
            RectangleComponent back;

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

            public BezierDrawer(Control control) : base(PPDExecuteArg.Empty, control)
            {
                InitializeDirectX(control.Width, control.Height, control.Width, control.Height, 1);
                Initialize();
                ResourceManager = new ResourceManager();
                spriteObject = new SpriteObject(device);
                spriteObject.AddChild(DrawTarget = new SpriteObject(device));
                spriteObject.AddChild(Background = new SpriteObject(device));
                spriteObject.AddChild(back = new RectangleComponent(device, ResourceManager, PPDColors.White)
                {
                    RectangleHeight = control.Height,
                    RectangleWidth = control.Width
                });
            }

            public void Resize(int width, int height)
            {
                back.RectangleWidth = width;
                back.RectangleHeight = height;
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
                if (spriteObject != null)
                {
                    spriteObject.Dispose();
                    spriteObject = null;
                }
                if (ResourceManager != null)
                {
                    ResourceManager.Dispose();
                    ResourceManager = null;
                }
                base.DisposeResource();
            }
        }

        public class Context : IBezierDrawContext
        {
            public BezierDrawer Drawer
            {
                get;
                private set;
            }

            public Context(BezierDrawer drawer)
            {
                Drawer = drawer;
            }

            public void DrawString(string text, Color color, float fontSize, PointF point)
            {
                Drawer.DrawTarget.InsertChild(new TextureString(Drawer.Device, text, (int)fontSize,
                    new SharpDX.Color4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f))
                {
                    Position = new SharpDX.Vector2(point.X, point.Y)
                }, 0);
            }

            public void DrawEllipse(Color color, RectangleF rect)
            {
                var center = new SharpDX.Vector2(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
                var maxRadius = (int)Math.Ceiling(Math.Max(rect.Width / 2, rect.Height / 2));
                var points = new SharpDX.Vector2[maxRadius * 4 + 1];
                for (var i = 0; i < points.Length; i++)
                {
                    var rad = Math.PI * 2 * i / points.Length;
                    points[i] = center + new SharpDX.Vector2(rect.Width / 2 * (float)Math.Cos(rad), rect.Height / 2 * (float)Math.Sin(rad));
                }
                var line = new LineComponent(Drawer.Device, Drawer.ResourceManager,
                    new SharpDX.Color4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f))
                {
                    Points = points,
                    LineWidth = 1
                };
                Drawer.DrawTarget.InsertChild(line, 0);
            }

            public void DrawLine(Color color, PointF p1, PointF p2)
            {
                var line = new LineComponent(Drawer.Device, Drawer.ResourceManager,
                    new SharpDX.Color4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f))
                {
                    Points = new SharpDX.Vector2[] {
                        new SharpDX.Vector2(p1.X,p1.Y),
                        new SharpDX.Vector2(p2.X,p2.Y)
                    },
                    LineWidth = 1
                };
                Drawer.DrawTarget.InsertChild(line, 0);
            }

            public void DrawLines(Color color, PointF[] points)
            {
                var line = new LineComponent(Drawer.Device, Drawer.ResourceManager,
                    new SharpDX.Color4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f))
                {
                    Points = points.Select(p => new SharpDX.Vector2(p.X, p.Y)).ToArray(),
                    LineWidth = 1
                };
                Drawer.DrawTarget.InsertChild(line, 0);
            }

            public void DrawRectangle(Color color, float x, float y, float width, float height)
            {
                var lineRect = new LineRectangleComponent(Drawer.Device, Drawer.ResourceManager,
                    new SharpDX.Color4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f))
                {
                    Position = new SharpDX.Vector2(x, y),
                    RectangleWidth = width,
                    RectangleHeight = height,
                    BorderThickness = 1
                };
                Drawer.DrawTarget.InsertChild(lineRect, 0);
            }

            public void FillRectangle(Color color, float x, float y, float width, float height)
            {
                var rect = new RectangleComponent(Drawer.Device, Drawer.ResourceManager,
                    new SharpDX.Color4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f))
                {
                    Position = new SharpDX.Vector2(x, y),
                    RectangleWidth = width,
                    RectangleHeight = height
                };
                Drawer.DrawTarget.InsertChild(rect, 0);
            }
        }

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

        public DXBezierControl()
        {
            Controller = new BezierController();
            Controller.DrawRequired += Controller_DrawRequired;
            InitializeComponent();
            Disposed += DXBezierControl_Disposed;
        }

        public void Initialize()
        {
            Drawer = new BezierDrawer(this);
        }

        private void DXBezierControl_Disposed(object sender, EventArgs e)
        {
            if (Drawer != null)
            {
                Drawer.Dispose();
                Drawer = null;
            }
        }

        private void Controller_DrawRequired()
        {
            DrawAndRefresh();
        }

        private void DrawToBuffer()
        {
            Controller.Draw(new Context(Drawer));
        }

        public void DrawAndRefresh()
        {
            if (Drawer == null)
            {
                return;
            }

            Drawer.DrawTarget.ClearDisposeChildren();
            DrawToBuffer();
            Drawer.Routine();
        }

        private void DXBezierControl_MouseDown(object sender, MouseEventArgs e)
        {
            Controller.MouseDown(e.X, e.Y, e.Button, ModifierKeys);
            DrawAndRefresh();
        }

        private void DXBezierControl_MouseUp(object sender, MouseEventArgs e)
        {
            Controller.MouseUp();
            DrawAndRefresh();
        }

        private void DXBezierControl_MouseMove(object sender, MouseEventArgs e)
        {
            var cursor = Controller.MouseMove(e.X, e.Y, ModifierKeys);
            if (cursor != null)
            {
                Cursor = cursor;
            }
        }

        private void DXBezierControl_SizeChanged(object sender, EventArgs e)
        {
            if (this.Width > 0 && this.Height > 0 && Drawer != null)
            {
                Drawer.Resize(Width, Height);
                DrawAndRefresh();
            }
        }
    }
}
