using PPDMath;
using BezierDrawControl;
using Effect2D;
using PPDConfiguration;
using PPDFramework;
using PPDFramework.Resource;
using PPDFramework.Shaders;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;

namespace Effect2DEditor.DockForm
{
    public enum BackGroundMode
    {
        Default = 0,
        Color = 1,
        Image = 2
    }

    public partial class CanvasDock : UserControl
    {
        public event EventHandler BezierEdited;
        public event EventHandler BezierReset;

        readonly DispatcherTimer timer;
        string checkBackPath;

        const int checksize = 8;
        /// <summary>
        /// 始点・終点の文字の大きさです（以前の Font.Height と同じ）。
        /// </summary>
        const int FontHeight = 12;

        public Color BackGroundColor
        {
            get;
            set;
        }

        public string BackImageFileName
        {
            get;
            set;
        }

        public BackGroundMode BackGroundMode
        {
            get;
            set;
        }

        public EffectManager EffectManager
        {
            get;
            private set;
        }

        public SelectedManager SelectedManager
        {
            get;
            private set;
        }

        public bool IsTransFormMode
        {
            get;
            private set;
        }

        public EffectManager.PlayType PlayType
        {
            get;
            set;
        }

        public DXBezierControl BezierControl
        {
            get
            {
                return bezierControl1;
            }
        }

        public int CanvasWidth
        {
            get
            {
                return bezierControl1.PixelWidth;
            }
        }

        public int CanvasHeight
        {
            get
            {
                return bezierControl1.PixelHeight;
            }
        }

        public CanvasDock()
        {
            BackGroundColor = Color.White;

            InitializeComponent();

            bezierControl1.Controller.AllowMouseOperation = false;
            bezierControl1.Controller.BeforePaint += BezierControl1_BeforePaint;
            bezierControl1.Controller.Edited += BezierControl1_Edited;
            bezierControl1.PixelSizeChanged += BezierControl1_SizeChanged;
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1000.0 / 60)
            };
            timer.Tick += Timer_Tick;
        }

        public void SetEffectManager(EffectManager effectManager)
        {
            EffectManager = effectManager;
            SelectedManager = new SelectedManager(EffectManager);
            EffectManager.Finish += EffectManager_Finish;
        }

        public void SetLang(SettingReader lang)
        {
            選択したアンカーを削除ToolStripMenuItem.Header = lang["DeleteSelectedAnchor"];
            変形を開始するToolStripMenuItem.Header = lang["StartTransform"];
            変形を終了するToolStripMenuItem.Header = lang["EndTransform"];
            リセットするToolStripMenuItem.Header = lang["Reset"];
        }

        public void Initialize(int width, int height)
        {
            bezierControl1.Initialize(width, height);
        }

        void BezierControl1_SizeChanged(object sender, EventArgs e)
        {
            bezierControl1.Controller.Center = new Point(CanvasWidth / 2, CanvasHeight / 2);
            CreateCheckBack(CanvasWidth);
            RefreshCanvas();
        }

        void BezierControl1_Edited(object sender, BezierDrawControl.BezierControlPointEditEventArgs e)
        {
            BezierEdited?.Invoke(this, EventArgs.Empty);
        }

        public void RefreshCanvas()
        {
            bezierControl1.DrawAndRefresh();
        }

        public void Stop()
        {
            timer.Stop();
            EffectManager.Update(0, null);
            EffectManager.Stop();
            bezierControl1.DrawAndRefresh();
        }

        public void Pause()
        {
            timer.Stop();
            EffectManager.Pause();
        }

        public void Play()
        {
            timer.Stop();
            EffectManager.CheckFrameLength();
            EffectManager.Play(PlayType);
            timer.Start();
        }

        /// <summary>
        /// 描画の資源と、背景の一時ファイルを破棄します。
        /// </summary>
        public void Close()
        {
            timer.Stop();
            bezierControl1.Dispose();
            DeleteCheckBack();
        }

        private void DeleteCheckBack()
        {
            if (checkBackPath != null)
            {
                try
                {
                    File.Delete(checkBackPath);
                }
                catch (IOException)
                {
                }
                checkBackPath = null;
            }
        }

        private void CreateCheckBack(int width)
        {
            // 背景の市松模様を画像ファイルにする（白と薄い灰色を checksize ごとに交互に並べる）
            DeleteCheckBack();
            checkBackPath = Path.GetTempFileName();
            var stride = width * 4;
            var pixels = new byte[stride * width];
            for (var y = 0; y < width; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    byte value = ((x / checksize + y / checksize) % 2 == 0) ? (byte)0xFF : (byte)0xD3;
                    var index = y * stride + x * 4;
                    pixels[index] = value;
                    pixels[index + 1] = value;
                    pixels[index + 2] = value;
                    pixels[index + 3] = 0xFF;
                }
            }
            var bitmap = BitmapSource.Create(width, width, 96, 96, PixelFormats.Bgra32, null, pixels, stride);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using var stream = File.Create(checkBackPath);
            encoder.Save(stream);
        }
        void BezierControl1_BeforePaint(object sender, BezierDrawControl.UserPaintEventArgs e)
        {
            var context = ((DXBezierControl.Context)e.Context);
            DrawBack(context.Drawer);
            EffectManager?.Draw(Draw);
            DrawCenter(context);
            DrawBezierMark(context);
        }

        void EffectManager_Finish(object sender, EventArgs e)
        {
            timer.Stop();
        }

        private void DrawBack(DXBezierControl.BezierDrawer drawer)
        {
            drawer.Background.ClearChildren();
            switch (BackGroundMode)
            {
                case DockForm.BackGroundMode.Default:
                    drawer.Background.AddChild(new PictureObject(drawer.Device, drawer.ResourceManager, PathObject.Absolute(checkBackPath)));
                    break;
                case DockForm.BackGroundMode.Color:
                    drawer.Background.AddChild(new RectangleComponent(drawer.Device, drawer.ResourceManager,
                        new Vortice.Mathematics.Color4(BackGroundColor.R / 255f, BackGroundColor.G / 255f, BackGroundColor.B / 255f, 1.0f))
                    {
                        RectangleWidth = CanvasWidth,
                        RectangleHeight = CanvasHeight
                    });
                    break;
                case DockForm.BackGroundMode.Image:
                    if (!File.Exists(BackImageFileName))
                    {
                        return;
                    }
                    var resource = drawer.ResourceManager.GetResource<ImageResourceBase>(BackImageFileName);
                    if (resource == null)
                    {
                        resource = ImageResourceFactoryManager.Factory.Create(drawer.Device, BackImageFileName, false);
                        drawer.ResourceManager.Add(BackImageFileName, resource);
                    }
                    for (int i = 0; i * resource.Width < CanvasWidth; i++)
                    {
                        for (int j = 0; j * resource.Height < CanvasHeight; j++)
                        {
                            drawer.Background.AddChild(new PictureObject(drawer.Device, drawer.ResourceManager, PathObject.Absolute(BackImageFileName))
                            {
                                Position = new System.Numerics.Vector2(i * resource.Width, j * resource.Height)
                            });
                        }
                    }
                    break;
            }
        }

        private void DrawBezierMark(IBezierDrawContext context)
        {
            if (bezierControl1.Controller.BCPS != null && bezierControl1.Controller.BCPS.Length >= 2)
            {
                context.DrawString("S", Color.Black, FontHeight, bezierControl1.Controller.BCPS[0].Second);
                context.DrawString("T", Color.Black, FontHeight, bezierControl1.Controller.BCPS[^1].Second);
            }
        }

        private void DrawCenter(IBezierDrawContext context)
        {
            context.DrawLine(Color.FromArgb(128, 0, 0, 0), new Point(CanvasWidth / 2, 0), new Point(CanvasWidth / 2, CanvasHeight));
            context.DrawLine(Color.FromArgb(128, 0, 0, 0), new Point(0, CanvasHeight / 2), new Point(CanvasWidth, CanvasHeight / 2));
        }

        private void Draw(string filename, EffectStateStructure state)
        {
            if (state == null || state.Alpha <= 0 || state.ScaleX == 0 || state.ScaleY == 0) return;

            var obj = new CustomEffectObject(bezierControl1.Drawer.Device, bezierControl1.Drawer.ResourceManager, filename, state)
            {
                Position = new System.Numerics.Vector2(CanvasWidth / 2, CanvasHeight / 2)
            };
            bezierControl1.Drawer.DrawTarget.InsertChild(obj, 0);
        }

        void Timer_Tick(object sender, EventArgs e)
        {
            if (EffectManager.State == EffectManager.PlayState.Stop)
            {
                timer.Stop();
            }
            EffectManager.Update();
            bezierControl1.DrawAndRefresh();
        }


        private void 選択したアンカーを削除ToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            bezierControl1.Controller.Delete();
        }

        private void 変形を開始するToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            IsTransFormMode = true;
            bezierControl1.Controller.StartTransform();
        }

        private void 変形を終了するToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            IsTransFormMode = false;
            bezierControl1.Controller.EndTransform();
        }

        private void リセットするToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            BezierReset?.Invoke(this, EventArgs.Empty);
        }

        private void ContextMenu_Opening(object sender, ContextMenuEventArgs e)
        {
            if (bezierControl1.Controller.AllowMouseOperation)
            {
                選択したアンカーを削除ToolStripMenuItem.IsEnabled = bezierControl1.Controller.SelectedIndex != -1 && !IsTransFormMode;
                変形を開始するToolStripMenuItem.IsEnabled = !IsTransFormMode;
                変形を終了するToolStripMenuItem.IsEnabled = IsTransFormMode;
                リセットするToolStripMenuItem.IsEnabled = SelectedManager.Set.IsBezierPosition;
            }
            else
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// エフェクト描画クラス
        /// </summary>
        public class CustomEffectObject : GameComponent
        {
            readonly ResourceManager resourceManager;
            readonly EffectStateStructure effestState;
            readonly ImageResourceBase imageResource;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="device"></param>
            /// <param name="resourceManager">リソースマネージャー</param>
            public CustomEffectObject(PPDDevice device, ResourceManager resourceManager, string filename, EffectStateStructure effectState) : base(device)
            {
                this.effestState = effectState;
                this.resourceManager = resourceManager;
                // 読み込んだ画像はリソースマネージャーに登録し、次からはそれを使う（リソースマネージャーと一緒に破棄される）
                imageResource = resourceManager.GetResource<ImageResourceBase>(filename);
                if (imageResource == null)
                {
                    imageResource = ImageResourceFactoryManager.Factory.Create(device, filename, false);
                    if (imageResource != null)
                    {
                        resourceManager.Add(filename, imageResource);
                    }
                }
            }

            /// <summary>
            /// 描画処理を行います
            /// </summary>
            /// <param name="alphaBlendContext"></param>
            protected override void DrawImpl(AlphaBlendContext alphaBlendContext)
            {
                if (imageResource == null)
                {
                    return;
                }
                var initialAlpha = alphaBlendContext.Alpha;
                var initialDepth = alphaBlendContext.SRTDepth;
                alphaBlendContext.Texture = imageResource.Texture;
                alphaBlendContext.Vertex = imageResource.Vertex;
                alphaBlendContext.Alpha = initialAlpha * effestState.ComposedAlpha;
                alphaBlendContext.SRTDepth = initialDepth + 1;
                if (effestState.ComposedBlendMode != BlendMode.None)
                {
                    alphaBlendContext.BlendMode = effestState.ComposedBlendMode;
                }
                foreach (var matrix in effestState.ComposedMatrices)
                {
                    alphaBlendContext.SetSRT(matrix, alphaBlendContext.SRTDepth++);
                }
                var pos = new System.Numerics.Vector2(-imageResource.Width / 2, -imageResource.Height / 2);
                alphaBlendContext.SetSRT(MathCompat.Translation(new System.Numerics.Vector3(pos, 0)), alphaBlendContext.SRTDepth);
                device.GetModule<AlphaBlend>().Draw(device, alphaBlendContext);
            }
        }
    }
}
