using PPDConfiguration;
using PPDFramework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace PPDEditor
{
    public class PPDEditorSkin
    {
        public enum BackGroundDisplayMode
        {
            right = 0,
            left = 1,
            center = 2,
            fill = 3
        }

        private static PPDEditorSkin skin = new PPDEditorSkin();
        string[] smallcolorimagepaths = new string[(int)ButtonType.Start];
        PathObject[] colorimagepaths = new PathObject[(int)ButtonType.Start + 4];
        PathObject[] imagepaths = new PathObject[(int)ButtonType.Start + 4];
        PathObject[] traceimagepaths = new PathObject[(int)ButtonType.Start + 4];
        PathObject longnotecirclepath;
        float _innerradius;
        float _outerradius;
        PathObject axisimagepath;
        PathObject clockaxisimagepath;
        PathObject[] effectpaths = new PathObject[(int)MarkEvaluateType.Worst + 1];
        PathObject appeareffectpath;
        PathObject holdpath;
        float holdx;
        float holdy;

        public static PPDEditorSkin Skin
        {
            get
            {
                return skin;
            }
        }

        private PPDEditorSkin()
        {
            OverLayColor = Color.FromArgb(220, 255, 255, 255);
            FillColor = Color.FromArgb(255, 255, 255);
            BorderColor = Color.FromArgb(90, 90, 90);
            TransparentTextColor = Color.Black;
            TimeLineCurrentTimeColor = Color.Red;
            TimeLineHorizontalLineColor = Color.Gray;
            TimeLineSeekAreaColor = Color.LightGray;
            TimeLineSeekAreaBorderColor = Color.Gray;
            TimeLineVerticalLineColor1 = Color.DarkSlateGray;
            TimeLineVerticalLineColor2 = Color.Silver;
            TimeLineVerticalLineColor3 = Color.FromArgb(204, 204, 204);
            TimeLineVerticalLineColor4 = Color.Green;
            TimeLineVerticalLineColor5 = Color.LightGreen;
            TimeLineBackGroundColors = new Dictionary<int, Brush>();
            TimeLineHoldExtentColor = Color.Orange;
            TimeLineRowHeight = 26;
            TimeLineHeight = 22;
            var sb = new SolidBrush(Color.FromArgb(128, Color.Yellow));
            for (int i = 0; i < 10; i++)
            {
                TimeLineBackGroundColors.Add(i, sb);
            }
            DisplayMode = BackGroundDisplayMode.right;
            var dockPanelSkin = new DockPanelSkin();
            dockPanelSkin.AutoHideStripSkin.DockStripGradient = CreateDockPanelGradient(81, 85, 94, 37, 42, 45, System.Drawing.Drawing2D.LinearGradientMode.Horizontal);
            dockPanelSkin.AutoHideStripSkin.TabGradient = CreateTabGradient(147, 207, 208, 7, 117, 118, System.Drawing.Drawing2D.LinearGradientMode.Horizontal);
            dockPanelSkin.DockPaneStripSkin.DocumentGradient.ActiveTabGradient = CreateTabGradient(231, 245, 248, 0, 157, 188, System.Drawing.Drawing2D.LinearGradientMode.Horizontal);
            dockPanelSkin.DockPaneStripSkin.DocumentGradient.DockStripGradient = CreateTabGradient(37, 42, 45, 81, 85, 94, System.Drawing.Drawing2D.LinearGradientMode.Horizontal);
            dockPanelSkin.DockPaneStripSkin.DocumentGradient.InactiveTabGradient = CreateTabGradient(147, 207, 208, 7, 117, 118, System.Drawing.Drawing2D.LinearGradientMode.Horizontal);
            dockPanelSkin.DockPaneStripSkin.ToolWindowGradient.ActiveCaptionGradient = CreateTabGradient(231, 245, 248, 0, 157, 188, System.Drawing.Drawing2D.LinearGradientMode.Horizontal);
            dockPanelSkin.DockPaneStripSkin.ToolWindowGradient.ActiveTabGradient = CreateTabGradient(231, 245, 248, 0, 157, 188, System.Drawing.Drawing2D.LinearGradientMode.Horizontal);
            dockPanelSkin.DockPaneStripSkin.ToolWindowGradient.DockStripGradient = CreateTabGradient(81, 85, 94, 37, 42, 45, System.Drawing.Drawing2D.LinearGradientMode.Horizontal);
            dockPanelSkin.DockPaneStripSkin.ToolWindowGradient.InactiveCaptionGradient = CreateTabGradient(147, 207, 208, 7, 117, 118, System.Drawing.Drawing2D.LinearGradientMode.Horizontal);
            dockPanelSkin.DockPaneStripSkin.ToolWindowGradient.InactiveTabGradient = CreateTabGradient(147, 207, 208, 7, 117, 118, System.Drawing.Drawing2D.LinearGradientMode.Horizontal);
            DockPanelSkin = dockPanelSkin;

            if (File.Exists("defaultskin.ini"))
            {
                var sr = new StreamReader("defaultskin.ini");
                var setting = new SettingReader(sr.ReadToEnd());
                sr.Close();
                for (ButtonType type = ButtonType.Square; type < ButtonType.Start; type++)
                {
                    colorimagepaths[(int)type] = Utility.Path.Combine(setting.ReadString("Color" + type));
                    imagepaths[(int)type] = Utility.Path.Combine(setting.ReadString(type.ToString()));
                    traceimagepaths[(int)type] = Utility.Path.Combine(setting.ReadString("Trace" + type));
                }
                colorimagepaths[(int)ButtonType.Start] = Utility.Path.Combine(setting["ColorSliderRight"]);
                colorimagepaths[(int)ButtonType.Start + 1] = Utility.Path.Combine(setting["ColorSliderLeft"]);
                colorimagepaths[(int)ButtonType.Start + 2] = Utility.Path.Combine(setting["ColorSliderRightExtra"]);
                colorimagepaths[(int)ButtonType.Start + 3] = Utility.Path.Combine(setting["ColorSliderLeftExtra"]);
                imagepaths[(int)ButtonType.Start] = Utility.Path.Combine(setting["SliderRight"]);
                imagepaths[(int)ButtonType.Start + 1] = Utility.Path.Combine(setting["SliderLeft"]);
                imagepaths[(int)ButtonType.Start + 2] = Utility.Path.Combine(setting["SliderRightExtra"]);
                imagepaths[(int)ButtonType.Start + 3] = Utility.Path.Combine(setting["SliderLeftExtra"]);
                traceimagepaths[(int)ButtonType.Start] =
                    traceimagepaths[(int)ButtonType.Start + 1] =
                    traceimagepaths[(int)ButtonType.Start + 2] =
                    traceimagepaths[(int)ButtonType.Start + 3] = Utility.Path.Combine(setting["TraceSlider"]);
                longnotecirclepath = Utility.Path.Combine(setting.ReadString("LongNoteCirle"));
                _innerradius = float.Parse(setting.ReadString("InnerRadius"));
                _outerradius = float.Parse(setting.ReadString("OuterRadius"));
                axisimagepath = Utility.Path.Combine(setting.ReadString("CircleAxis"));
                clockaxisimagepath = Utility.Path.Combine(setting.ReadString("ClockAxis"));
                for (MarkEvaluateType type = MarkEvaluateType.Cool; type <= MarkEvaluateType.Worst; type++)
                {
                    effectpaths[(int)type] = Utility.Path.Combine(setting.ReadString("Effect" + type));
                }
                appeareffectpath = Utility.Path.Combine(setting.ReadString("EffectAppear"));
                holdpath = Utility.Path.Combine(setting.ReadString("Hold"));
                holdx = float.Parse(setting.ReadString("HoldX"));
                holdy = float.Parse(setting.ReadString("HoldY"));
            }
        }

        public void Initialize(string filename)
        {
            try
            {
                if (File.Exists(filename))
                {
                    var rgb = new Regex("(\\d+)\\s*,\\s*(\\d+)\\s*,\\s*(\\d+)");
                    var argb = new Regex("(\\d+)\\s*,\\s*(\\d+)\\s*,\\s*(\\d+)\\s*,\\s*(\\d+)");
                    var sr = new StreamReader(filename);
                    var data = sr.ReadToEnd();
                    sr.Close();
                    var setting = new SettingReader(data);
                    var backfilename = setting.ReadString("BackGround");
                    if (File.Exists(backfilename))
                    {
                        BackGround = new Bitmap(backfilename);
                    }
                    var displaymode = int.Parse(setting.ReadString("DisplayMode"));
                    if (displaymode >= 0 && displaymode < 4)
                    {
                        DisplayMode = (BackGroundDisplayMode)displaymode;
                    }
                    var fillcolor = setting.ReadString("FillColor");
                    FillColor = GetColorFromString(rgb, argb, fillcolor);
                    var overraycolor = setting.ReadString("OverLayColor");
                    OverLayColor = GetColorFromString(rgb, argb, overraycolor);
                    var bordercolor = setting.ReadString("BorderColor");
                    BorderColor = GetColorFromString(rgb, argb, bordercolor);
                    var transparenttextcolor = setting.ReadString("TransparentTextColor");
                    TransparentTextColor = GetColorFromString(rgb, argb, transparenttextcolor);
                    var timelinetextcolor = setting.ReadString("TimeLineTextColor");
                    TimeLineTextColor = GetColorFromString(rgb, argb, timelinetextcolor);
                    var timelineareacolor = setting.ReadString("TimeLineSeekAreaColor");
                    TimeLineSeekAreaColor = GetColorFromString(rgb, argb, timelineareacolor);
                    var timelineseekareabordercolor = setting.ReadString("TimeLineSeekAreaBorderColor");
                    TimeLineSeekAreaBorderColor = GetColorFromString(rgb, argb, timelineseekareabordercolor);
                    var timelinehorizontallinecolor = setting.ReadString("TimeLineHorizontalLineColor");
                    TimeLineHorizontalLineColor = GetColorFromString(rgb, argb, timelinehorizontallinecolor);
                    var timelineverticallinecolor1 = setting.ReadString("TimeLineVerticalLineColor1");
                    TimeLineVerticalLineColor1 = GetColorFromString(rgb, argb, timelineverticallinecolor1);
                    var timelineverticallinecolor2 = setting.ReadString("TimeLineVerticalLineColor2");
                    TimeLineVerticalLineColor2 = GetColorFromString(rgb, argb, timelineverticallinecolor2);
                    var timelineverticallinecolor3 = setting.ReadString("TimeLineVerticalLineColor3");
                    TimeLineVerticalLineColor3 = GetColorFromString(rgb, argb, timelineverticallinecolor3);
                    var timelineverticallinecolor4 = setting.ReadString("TimeLineVerticalLineColor4");
                    TimeLineVerticalLineColor4 = GetColorFromString(rgb, argb, timelineverticallinecolor4);
                    var timelineverticallinecolor5 = setting.ReadString("TimeLineVerticalLineColor5");
                    TimeLineVerticalLineColor5 = GetColorFromString(rgb, argb, timelineverticallinecolor5);
                    var timelinecurrenttimecolor = setting.ReadString("TimeLineCurrentTimeColor");
                    TimeLineCurrentTimeColor = GetColorFromString(rgb, argb, timelinecurrenttimecolor);
                    var timelineselectionareacolor = setting.ReadString("TimeLineSelectAreaColor");
                    TimeLineSelectionAreaColor = GetColorFromString(rgb, argb, timelineselectionareacolor);
                    var timelineselectedmarkcolor = setting.ReadString("TimeLineSelectedMarkColor");
                    TimeLineSelectedMarkColor = GetColorFromString(rgb, argb, timelineselectedmarkcolor);
                    var timelinemarkcolor = setting.ReadString("TimeLineMarkColor");
                    TimeLineMarkColor = GetColorFromString(rgb, argb, timelinemarkcolor);
                    var timelineHoldExtentColor = setting.ReadString("TimeLineHoldExtentColor");
                    TimeLineHoldExtentColor = GetColorFromString(rgb, argb, timelineHoldExtentColor, TimeLineHoldExtentColor);
                    var dockPanelSkin = new DockPanelSkin();
                    var Name = new string[]{
                        "AutoHideDockStripGradient",
                        "AutoHideTabGradient",
                        "DocumentActiveTabGradient",
                        "DocumentDockStripGradient",
                        "DocumentInactiveTabGradient",
                        "ToolWindowActiveCaptionGradient",
                        "ToolWindowActiveTabGradient",
                        "ToolWindowDockStripGradient",
                        "ToolWindowInactiveCaptionGradient",
                        "ToolWindowInactiveTabGradient"
                    };
                    DockPanelGradient[] dpg = new DockPanelGradient[10];
                    dpg[0] = dockPanelSkin.AutoHideStripSkin.DockStripGradient;
                    dpg[1] = dockPanelSkin.AutoHideStripSkin.TabGradient;
                    dpg[2] = dockPanelSkin.DockPaneStripSkin.DocumentGradient.ActiveTabGradient;
                    dpg[3] = dockPanelSkin.DockPaneStripSkin.DocumentGradient.DockStripGradient;
                    dpg[4] = dockPanelSkin.DockPaneStripSkin.DocumentGradient.InactiveTabGradient;
                    dpg[5] = dockPanelSkin.DockPaneStripSkin.ToolWindowGradient.ActiveCaptionGradient;
                    dpg[6] = dockPanelSkin.DockPaneStripSkin.ToolWindowGradient.ActiveTabGradient;
                    dpg[7] = dockPanelSkin.DockPaneStripSkin.ToolWindowGradient.DockStripGradient;
                    dpg[8] = dockPanelSkin.DockPaneStripSkin.ToolWindowGradient.InactiveCaptionGradient;
                    dpg[9] = dockPanelSkin.DockPaneStripSkin.ToolWindowGradient.InactiveTabGradient;
                    for (int i = 0; i < dpg.Length; i++)
                    {
                        var mode = int.Parse(setting.ReadString(Name[i] + "Mode"));
                        LinearGradientMode Mode = LinearGradientMode.Horizontal;
                        if (mode >= 0 && mode < 4)
                        {
                            Mode = (LinearGradientMode)mode;
                        }
                        var start = setting.ReadString(Name[i] + "StartColor");
                        var StartColor = GetColorFromString(rgb, argb, start);
                        var end = setting.ReadString(Name[i] + "EndColor");
                        var EndColor = GetColorFromString(rgb, argb, end);
                        if (dpg[i] is TabGradient)
                        {
                            var tg = dpg[i] as TabGradient;
                            var text = setting.ReadString(Name[i] + "TextColor");
                            var TextColor = GetColorFromString(rgb, argb, text);
                            tg.TextColor = TextColor;
                            tg.StartColor = StartColor;
                            tg.EndColor = EndColor;
                            tg.LinearGradientMode = Mode;
                        }
                        else
                        {
                            dpg[i].StartColor = StartColor;
                            dpg[i].EndColor = EndColor;
                            dpg[i].LinearGradientMode = Mode;
                        }
                    }
                    DockPanelSkin = dockPanelSkin;
                    for (int i = 0; i < 10; i++)
                    {
                        TimeLineBackGroundColors[i].Dispose();
                    }
                    TimeLineBackGroundColors[0] = GetBrush(rgb, argb, setting, "TimeLineBackSquare", Color.White);
                    TimeLineBackGroundColors[1] = GetBrush(rgb, argb, setting, "TimeLineBackCross", Color.White);
                    TimeLineBackGroundColors[2] = GetBrush(rgb, argb, setting, "TimeLineBackCircle", Color.White);
                    TimeLineBackGroundColors[3] = GetBrush(rgb, argb, setting, "TimeLineBackTriangle", Color.White);
                    TimeLineBackGroundColors[4] = GetBrush(rgb, argb, setting, "TimeLineBackLeft", Color.White);
                    TimeLineBackGroundColors[5] = GetBrush(rgb, argb, setting, "TimeLineBackDown", Color.White);
                    TimeLineBackGroundColors[6] = GetBrush(rgb, argb, setting, "TimeLineBackRight", Color.White);
                    TimeLineBackGroundColors[7] = GetBrush(rgb, argb, setting, "TimeLineBackUp", Color.White);
                    TimeLineBackGroundColors[8] = GetBrush(rgb, argb, setting, "TimeLineBackR", Color.White);
                    TimeLineBackGroundColors[9] = GetBrush(rgb, argb, setting, "TimeLineBackL", Color.White);

                    smallcolorimagepaths[0] = setting.ReadString("TimeLineSquareImagePath");
                    smallcolorimagepaths[1] = setting.ReadString("TimeLineCrossImagePath");
                    smallcolorimagepaths[2] = setting.ReadString("TimeLineCircleImagePath");
                    smallcolorimagepaths[3] = setting.ReadString("TimeLineTriangleImagePath");
                    smallcolorimagepaths[4] = setting.ReadString("TimeLineLeftImagePath");
                    smallcolorimagepaths[5] = setting.ReadString("TimeLineDownImagePath");
                    smallcolorimagepaths[6] = setting.ReadString("TimeLineRightImagePath");
                    smallcolorimagepaths[7] = setting.ReadString("TimeLineUpImagePath");
                    smallcolorimagepaths[8] = setting.ReadString("TimeLineRImagePath");
                    smallcolorimagepaths[9] = setting.ReadString("TimeLineLImagePath");

                    if (int.TryParse(setting.ReadString("TimeLineHeight"), out int num))
                    {
                        TimeLineHeight = num;
                    }
                    if (int.TryParse(setting.ReadString("TimeLineRowHeight"), out num))
                    {
                        TimeLineRowHeight = num;
                    }

                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        private SolidBrush GetBrush(Regex rgb, Regex argb, SettingReader setting, string exp, Color ErrorColor)
        {
            return new SolidBrush(GetColor(rgb, argb, setting, exp, ErrorColor));
        }
        private Color GetColor(Regex rgb, Regex argb, SettingReader setting, string exp)
        {
            var color = setting.ReadString(exp);
            return GetColorFromString(rgb, argb, color);
        }
        private Color GetColor(Regex rgb, Regex argb, SettingReader setting, string exp, Color ErrorColor)
        {
            var color = setting.ReadString(exp);
            return GetColorFromString(rgb, argb, color, ErrorColor);
        }
        private Color GetColorFromString(Regex rgb, Regex argb, string data)
        {
            return GetColorFromString(rgb, argb, data, SystemColors.Control);
        }
        private Color GetColorFromString(Regex rgb, Regex argb, string data, Color ErrorColor)
        {
            Color ret = ErrorColor;
            var m = argb.Match(data);
            if (!m.Success)
            {
                m = rgb.Match(data);
            }
            if (m.Success)
            {
                if (m.Groups.Count == 4)
                {
                    ret = Color.FromArgb(int.Parse(m.Groups[1].Captures[0].Value), int.Parse(m.Groups[2].Captures[0].Value), int.Parse(m.Groups[3].Captures[0].Value));
                }
                else if (m.Groups.Count == 5)
                {
                    ret = Color.FromArgb(int.Parse(m.Groups[1].Captures[0].Value), int.Parse(m.Groups[2].Captures[0].Value), int.Parse(m.Groups[3].Captures[0].Value), int.Parse(m.Groups[4].Captures[0].Value));
                }
            }
            return ret;
        }
        private DockPanelGradient CreateDockPanelGradient(int sred, int sgreen, int sblue, int ered, int egreen, int eblue, LinearGradientMode gramode)
        {
            var gra = new DockPanelGradient
            {
                StartColor = Color.FromArgb(sred, sgreen, sblue),
                EndColor = Color.FromArgb(ered, egreen, eblue),
                LinearGradientMode = gramode
            };
            return gra;
        }
        private TabGradient CreateTabGradient(int sred, int sgreen, int sblue, int ered, int egreen, int eblue, LinearGradientMode gramode)
        {
            var tabgra = new TabGradient
            {
                StartColor = Color.FromArgb(sred, sgreen, sblue),
                EndColor = Color.FromArgb(ered, egreen, eblue),
                LinearGradientMode = gramode
            };
            return tabgra;
        }
        public Bitmap BackGround
        {
            get;
            set;
        }
        public Color OverLayColor
        {
            get;
            set;
        }
        public Color FillColor
        {
            get;
            set;
        }
        public Color BorderColor
        {
            get;
            set;
        }
        public BackGroundDisplayMode DisplayMode
        {
            get;
            set;
        }
        public DockPanelSkin DockPanelSkin
        {
            get;
            set;
        }
        public Color TransparentTextColor
        {
            get;
            set;
        }
        public Color TimeLineTextColor
        {
            get;
            set;
        }
        public Color TimeLineSeekAreaColor
        {
            get;
            set;
        }
        public Color TimeLineSeekAreaBorderColor
        {
            get;
            set;
        }
        public Color TimeLineHorizontalLineColor
        {
            get;
            set;
        }
        public Color TimeLineVerticalLineColor1
        {
            get;
            set;
        }
        public Color TimeLineVerticalLineColor2
        {
            get;
            set;
        }
        public Color TimeLineVerticalLineColor3
        {
            get;
            set;
        }
        public Color TimeLineVerticalLineColor4
        {
            get;
            set;
        }
        public Color TimeLineVerticalLineColor5
        {
            get;
            set;
        }
        public Color TimeLineCurrentTimeColor
        {
            get;
            set;
        }
        public Color TimeLineSelectionAreaColor
        {
            get;
            set;
        }
        public Color TimeLineSelectedMarkColor
        {
            get;
            set;
        }
        public Color TimeLineMarkColor
        {
            get;
            set;
        }
        public Dictionary<int, Brush> TimeLineBackGroundColors
        {
            get;
            set;
        }
        public Color TimeLineHoldExtentColor
        {
            get;
            set;
        }

        public int TimeLineHeight
        {
            get;
            set;
        }

        public int TimeLineRowHeight
        {
            get;
            set;
        }

        public PathObject GetMarkColorImagePath(ButtonType buttontype)
        {
            return colorimagepaths[(int)buttontype];
        }

        public PathObject GetMarkImagePath(ButtonType buttontype)
        {
            return imagepaths[(int)buttontype];
        }

        public PathObject GetTraceImagePath(ButtonType buttontype)
        {
            return traceimagepaths[(int)buttontype];
        }

        public string GetSmallMarkColorImagePath(ButtonType buttontype)
        {
            return smallcolorimagepaths[(int)buttontype];
        }

        public void GetLongNoteCircleInfo(out PathObject imagepath, out float innerradius, out float outerradius)
        {
            imagepath = longnotecirclepath;
            innerradius = _innerradius;
            outerradius = _outerradius;
        }

        public PathObject GetClockAxisImagePath()
        {
            return clockaxisimagepath;
        }

        public PathObject GetCircleAxisImagePath()
        {
            return axisimagepath;
        }

        public PathObject GetEvaluateEffectPath(MarkEvaluateType evatype)
        {
            return effectpaths[(int)evatype];
        }

        public PathObject GetAppearEffectPath()
        {
            return appeareffectpath;
        }

        public void GetHoldInfo(out PathObject imagepath, out float x, out float y)
        {
            imagepath = holdpath;
            x = holdx;
            y = holdy;
        }
    }
}
