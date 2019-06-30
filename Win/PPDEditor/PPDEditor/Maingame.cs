using PPDFramework;
using PPDMovie;
using PPDShareComponent;
using SharpDX;
using System;

namespace PPDEditor
{
    class MainGame : GameComponent
    {
        IMovie m;
        PPDFramework.Resource.ResourceManager resourceManager;
        GridComponent grid;
        MainGameTop mgt;
        MainGameBottom mgb;
        PictureObject up;
        PictureObject upSelect;
        PictureObject right;
        PictureObject rightSelect;
        PictureObject area;
        LineComponent angleLine;
        TextureString kasi;
        MarkPointDrawer mpd;
        DummyDrawComponent dummy;

        public IMovie Movie
        {
            get { return m; }
        }

        public MarkPointDrawer MarkPointDrawer
        {
            get { return mpd; }
        }

        public TextureString Kasi
        {
            get { return kasi; }
        }

        public GridComponent Grid
        {
            get { return grid; }
        }

        public DummyDrawComponent Dummy
        {
            get { return dummy; }
        }

        public MainGame(PPDDevice device, MyGame mygame, PPDFramework.Resource.ResourceManager resourceManager, SquareGrid squareGrid) : base(device)
        {
            this.resourceManager = resourceManager;
            AddChild(mpd = new MarkPointDrawer(device, resourceManager));
            AddChild(kasi = new TextureString(device, "", 20, true, PPDColors.White)
            {
                Position = new Vector2(400, 415)
            });
            AddChild(mgt = new MainGameTop(device, resourceManager));
            AddChild(mgb = new MainGameBottom(device, resourceManager));
            AddChild(up = new PictureObject(device, resourceManager, Utility.Path.Combine("assist", "up.png")));
            AddChild(upSelect = new PictureObject(device, resourceManager, Utility.Path.Combine("assist", "upselect.png")));
            AddChild(right = new PictureObject(device, resourceManager, Utility.Path.Combine("assist", "right.png")));
            AddChild(rightSelect = new PictureObject(device, resourceManager, Utility.Path.Combine("assist", "rightselect.png")));
            AddChild(area = new PictureObject(device, resourceManager, Utility.Path.Combine("assist", "area.png")));
            AddChild(angleLine = new LineComponent(device, resourceManager, PPDColors.Blue)
            {
                LineWidth = 3
            });
            AddChild(dummy = new DummyDrawComponent(device));
            AddChild(grid = new GridComponent(device, resourceManager, squareGrid));
        }

        public void UpdateAssist(MarkSelectMode markSelectMode, MarkSelectMode onMouseSelectMode)
        {
            up.Hidden = upSelect.Hidden = right.Hidden = rightSelect.Hidden = area.Hidden = angleLine.Hidden = true;
            if (WindowUtility.Seekmain == null)
            {
                return;
            }
            Mark smk = WindowUtility.Seekmain.SelectedMark;
            Mark hmk = WindowUtility.Seekmain.HeadMark;
            if (smk != null && !smk.Hidden)
            {
                switch (markSelectMode)
                {
                    case MarkSelectMode.Area:
                        UpdateArea(smk.Position);
                        break;
                    case MarkSelectMode.Up:
                        UpdateUp(smk.Position);
                        break;
                    case MarkSelectMode.Right:
                        UpdateRight(smk.Position);
                        break;
                    case MarkSelectMode.Angle:
                        UpdateAngle(smk.Position, smk.Rotation);
                        break;
                    default:
                        switch (onMouseSelectMode)
                        {
                            case MarkSelectMode.Area:
                                UpdateArea(smk.Position);
                                break;
                            case MarkSelectMode.Up:
                                UpdateUp(smk.Position);
                                break;
                            case MarkSelectMode.Right:
                                UpdateRight(smk.Position);
                                break;
                            case MarkSelectMode.Angle:
                                UpdateNormal(smk.Position);
                                break;
                        }

                        break;
                }
            }
            else if (hmk != null && !hmk.Hidden)
            {
                if (markSelectMode == MarkSelectMode.Angle)
                {
                    UpdateAngle(hmk.Position, hmk.Rotation);
                }
            }
        }

        private void UpdateArea(Vector2 p)
        {
            area.Position = new Vector2(p.X - 25, p.Y - 25);
            upSelect.Position = new Vector2(p.X - 9, p.Y - 80);
            rightSelect.Position = new Vector2(p.X, p.Y - 9);
            area.Hidden = PPDStaticSetting.HideToggleRectangle;
            upSelect.Hidden = PPDStaticSetting.HideToggleArrow;
            rightSelect.Hidden = PPDStaticSetting.HideToggleArrow;
        }

        private void UpdateUp(Vector2 p)
        {
            upSelect.Position = new Vector2(p.X - 9, p.Y - 80);
            right.Position = new Vector2(p.X, p.Y - 9);
            upSelect.Hidden = PPDStaticSetting.HideToggleArrow;
            right.Hidden = PPDStaticSetting.HideToggleArrow;
        }

        private void UpdateRight(Vector2 p)
        {
            up.Position = new Vector2(p.X - 9, p.Y - 80);
            rightSelect.Position = new Vector2(p.X, p.Y - 9);
            up.Hidden = PPDStaticSetting.HideToggleArrow;
            rightSelect.Hidden = PPDStaticSetting.HideToggleArrow;
        }

        private void UpdateNormal(Vector2 p)
        {
            up.Position = new Vector2(p.X - 9, p.Y - 80);
            right.Position = new Vector2(p.X, p.Y - 9);
            up.Hidden = PPDStaticSetting.HideToggleArrow;
            right.Hidden = PPDStaticSetting.HideToggleArrow;
        }

        private void UpdateAngle(Vector2 p1, float r)
        {
            var v = new Vector2((float)Math.Cos(r), -(float)Math.Sin(r));
            var p2 = new Vector2(v.X * 920 + p1.X, v.Y * 920 + p1.Y);
            var dir = new Vector2(p2.X - p1.X, p2.Y - p1.Y);
            dir = Vector2.Normalize(dir);
            var v1 = new Vector2(-dir.Y, dir.X);
            var v2 = new Vector2(-v1.X, -v1.Y);
            angleLine.Points = new Vector2[] { p1, p2 };
            angleLine.Hidden = false;
        }

        public void InitializeMovie(string fileName)
        {
            if (m != null)
            {
                this.RemoveChild((GameComponent)m);
                m.Dispose();
                m = null;
            }
            if (PPDSetting.Setting.IsMovie(fileName))
            {
                m = new SampleGrabberMovie(device, fileName);
            }
            else
            {
                m = new MusicPlayer(device, fileName);
            }
            m.Initialize();
            this.AddChild((GameComponent)m);
        }
    }
}
