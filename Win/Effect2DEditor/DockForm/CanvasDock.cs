using BezierDrawControl;
using Effect2D;
using PPDConfiguration;
using PPDFramework;
using PPDFramework.Resource;
using PPDFramework.Shaders;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Effect2DEditor.DockForm
{
    public enum BackGroundMode
    {
        Default = 0,
        Color = 1,
        Image = 2
    }

    public partial class CanvasDock : DockContent
    {
        public event EventHandler BezierEdited;
        public event EventHandler BezierReset;

        Timer timer;
        string checkBackPath;

        const int checksize = 8;

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

        public SortedList<string, Image> ImagePool
        {
            get;
            private set;
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
                return bezierControl1.Width;
            }
        }

        public int CanvasHeight
        {
            get
            {
                return bezierControl1.Height;
            }
        }


        public CanvasDock()
        {
            ImagePool = new SortedList<string, Image>();
            BackGroundColor = Color.White;

            InitializeComponent();

            bezierControl1.Controller.AllowMouseOperation = false;
            bezierControl1.Controller.BeforePaint += bezierControl1_BeforePaint;
            bezierControl1.Controller.Edited += bezierControl1_Edited;
            bezierControl1.SizeChanged += bezierControl1_SizeChanged;
            timer = new Timer
            {
                Interval = 1000 / 60
            };
            timer.Tick += timer_Tick;
            bezierControl1.Controller.BeforePaint += bezierControl1_BeforePaint;
            bezierControl1.Controller.Center = new Point(bezierControl1.Width / 2, bezierControl1.Height / 2);
            CreateCheckBack(bezierControl1.Width);

            this.SizeChanged += CanvasDock_SizeChanged;
        }

        void CanvasDock_SizeChanged(object sender, EventArgs e)
        {
            ResetLocation();
        }

        private void ResetLocation()
        {
            int offsetX = 0, offsetY = 0;
            if (this.ClientSize.Width > bezierControl1.Width)
            {
                offsetX = (this.ClientSize.Width - bezierControl1.Width) / 2;
            }
            if (this.ClientSize.Height > bezierControl1.Height)
            {
                offsetY = (this.ClientSize.Height - bezierControl1.Height) / 2;
            }
            bezierControl1.Location = new Point(offsetX, offsetY);
        }

        public void SetEffectManager(EffectManager effectManager)
        {
            EffectManager = effectManager;
            SelectedManager = new SelectedManager(EffectManager);
            EffectManager.Finish += EffectManager_Finish;
        }

        public void SetLang(SettingReader lang)
        {
            選択したアンカーを削除ToolStripMenuItem.Text = lang["DeleteSelectedAnchor"];
            変形を開始するToolStripMenuItem.Text = lang["StartTransform"];
            変形を終了するToolStripMenuItem.Text = lang["EndTransform"];
            リセットするToolStripMenuItem.Text = lang["Reset"];
        }

        public void Initialize(int width, int height)
        {
            bezierControl1.Size = new Size(width, height);
            bezierControl1.Initialize();
        }

        void bezierControl1_SizeChanged(object sender, EventArgs e)
        {
            bezierControl1.Controller.Center = new Point(bezierControl1.Width / 2, bezierControl1.Height / 2);
            CreateCheckBack(bezierControl1.Width);
            RefreshCanvas();
            ResetLocation();
        }

        void bezierControl1_Edited(object sender, BezierDrawControl.BezierControlPointEditEventArgs e)
        {
            if (BezierEdited != null)
            {
                BezierEdited.Invoke(this, EventArgs.Empty);
            }
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

        private void CreateCheckBack(int width)
        {
            checkBackPath = Path.GetTempFileName();
            using (var bitmap = new Bitmap(width, width))
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                for (int i = 0; i <= width / (checksize * 2); i++)
                {
                    for (int j = 0; j < width / (checksize * 2); j++)
                    {
                        g.FillRectangle(Brushes.White, new Rectangle(2 * i * checksize, 2 * j * checksize, checksize, checksize));
                        g.FillRectangle(Brushes.LightGray, new Rectangle((2 * i + 1) * checksize, 2 * j * checksize, checksize, checksize));
                        g.FillRectangle(Brushes.White, new Rectangle((2 * i + 1) * checksize, (2 * j + 1) * checksize, checksize, checksize));
                        g.FillRectangle(Brushes.LightGray, new Rectangle(2 * i * checksize, (2 * j + 1) * checksize, checksize, checksize));
                    }
                }
                bitmap.Save(checkBackPath, ImageFormat.Png);
            }
        }
        void bezierControl1_BeforePaint(object sender, BezierDrawControl.UserPaintEventArgs e)
        {
            var context = ((DXBezierControl.Context)e.Context);
            DrawBack(context.Drawer);
            if (EffectManager != null)
            {
                EffectManager.Draw(Draw);
            }
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
                    drawer.Background.AddChild(new PictureObject(drawer.Device, bezierControl1.Drawer.ResourceManager, PathObject.Absolute(checkBackPath)));
                    break;
                case DockForm.BackGroundMode.Color:
                    drawer.Background.AddChild(new RectangleComponent(drawer.Device, drawer.ResourceManager,
                        new SharpDX.Color4(BackGroundColor.R / 255f, BackGroundColor.G / 255f, BackGroundColor.B / 255f, 1.0f))
                    {
                        RectangleWidth = bezierControl1.Width,
                        RectangleHeight = bezierControl1.Height
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
                    for (int i = 0; i * resource.Width < bezierControl1.Width; i++)
                    {
                        for (int j = 0; j * resource.Height < bezierControl1.Height; j++)
                        {
                            drawer.Background.AddChild(new PictureObject(drawer.Device, drawer.ResourceManager, PathObject.Absolute(BackImageFileName))
                            {
                                Position = new SharpDX.Vector2(i * resource.Width, j * resource.Height)
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
                context.DrawString("S", Color.Black, Font.Height, bezierControl1.Controller.BCPS[0].Second);
                context.DrawString("T", Color.Black, Font.Height, bezierControl1.Controller.BCPS[bezierControl1.Controller.BCPS.Length - 1].Second);
            }
        }

        private void DrawCenter(IBezierDrawContext context)
        {
            context.DrawLine(Color.FromArgb(128, 0, 0, 0), new Point(bezierControl1.Width / 2, 0), new Point(bezierControl1.Width / 2, bezierControl1.Height));
            context.DrawLine(Color.FromArgb(128, 0, 0, 0), new Point(0, bezierControl1.Height / 2), new Point(bezierControl1.Width, bezierControl1.Height / 2));
        }

        private void Draw(string filename, EffectStateStructure state)
        {
            if (state == null || state.Alpha <= 0 || state.ScaleX == 0 || state.ScaleY == 0) return;

            var ir = bezierControl1.Drawer.ResourceManager.GetResource<ImageResourceBase>(filename);
            if (ir == null)
            {
                ir = ImageResourceFactoryManager.Factory.Create(bezierControl1.Drawer.Device, filename, false);
            }
            var obj = new CustomEffectObject(bezierControl1.Drawer.Device, bezierControl1.Drawer.ResourceManager, filename, state)
            {
                Position = new SharpDX.Vector2(bezierControl1.Width / 2, bezierControl1.Height / 2)
            };
            bezierControl1.Drawer.DrawTarget.InsertChild(obj, 0);
        }

        void timer_Tick(object sender, EventArgs e)

        {
            if (EffectManager.State == EffectManager.PlayState.Stop)
            {
                timer.Stop();
            }
            EffectManager.Update();
            bezierControl1.DrawAndRefresh();
        }


        private void 選択したアンカーを削除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bezierControl1.Controller.Delete();
        }

        private void 変形を開始するToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IsTransFormMode = true;
            bezierControl1.Controller.StartTransform();
        }

        private void 変形を終了するToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IsTransFormMode = false;
            bezierControl1.Controller.EndTransform();
        }

        private void リセットするToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (BezierReset != null)
            {
                BezierReset.Invoke(this, EventArgs.Empty);
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (bezierControl1.Controller.AllowMouseOperation)
            {
                選択したアンカーを削除ToolStripMenuItem.Enabled = bezierControl1.Controller.SelectedIndex != -1 && !IsTransFormMode;
                変形を開始するToolStripMenuItem.Enabled = !IsTransFormMode;
                変形を終了するToolStripMenuItem.Enabled = IsTransFormMode;
                リセットするToolStripMenuItem.Enabled = SelectedManager.Set.IsBezierPosition;
            }
            else
            {
                e.Cancel = true;
            }
        }

        /// <summary>
        /// エフェクト描画クラス
        /// </summary>
        public class CustomEffectObject : GameComponent
        {
            ResourceManager resourceManager;
            EffectStateStructure effestState;
            ImageResourceBase imageResource;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="device"></param>
            /// <param name="resourceManager">リソースマネージャー</param>
            public CustomEffectObject(PPDDevice device, ResourceManager resourceManager, string filename, EffectStateStructure effectState) : base(device)
            {
                this.effestState = effectState;
                this.resourceManager = resourceManager;
                imageResource = resourceManager.GetResource<ImageResourceBase>(filename);
                if (imageResource == null)
                {
                    imageResource = ImageResourceFactoryManager.Factory.Create(device, filename, false);
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
                var pos = new SharpDX.Vector2(-imageResource.Width / 2, -imageResource.Height / 2);
                alphaBlendContext.SetSRT(SharpDX.Matrix.Translation(new SharpDX.Vector3(pos, 0)), alphaBlendContext.SRTDepth);
                device.GetModule<AlphaBlend>().Draw(device, alphaBlendContext);
            }
        }
    }
}
