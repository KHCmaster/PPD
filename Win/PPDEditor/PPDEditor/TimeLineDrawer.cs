using PPDEditor.ViewModels.Docks;
using PPDEditor.Controls;
using PPDEditor.Forms;
using PPDFramework;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;

namespace PPDEditor
{
    class TimeLineDrawer
    {
        public const int CircleWidth = 10;

        /// <summary>
        /// イベント、サウンド、歌詞の変化の印です。
        /// </summary>
        public static readonly ImageSource EventMark = LoadImage("eventmark");
        public static readonly ImageSource SoundMark = LoadImage("soundmark");
        public static readonly ImageSource LylicsMark = LoadImage("lylicsmark");

        static readonly Typeface numberTypeface = new(new FontFamily("ＭＳ ゴシック"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

        private static ImageSource LoadImage(string name)
        {
            var image = new BitmapImage(new Uri(String.Format("pack://application:,,,/PPDEditor;component/img/{0}.png", name)));
            image.Freeze();
            return image;
        }

        public static void Draw(TimeLineDrawParameter parameter)
        {
            if (parameter == null)
            {
                return;
            }

            var dc = parameter.DrawingContext;
            var skin = PPDEditorSkin.Skin;
            var area = ToBrush(skin.TimeLineSeekAreaColor);
            var border = ToPen(skin.TimeLineSeekAreaBorderColor);
            var text = ToBrush(skin.TimeLineTextColor);
            var hline = ToPen(skin.TimeLineHorizontalLineColor);
            var time = ToPen(skin.TimeLineCurrentTimeColor);
            var vline1 = ToPen(skin.TimeLineVerticalLineColor1);
            var vline2 = ToPen(skin.TimeLineVerticalLineColor2);
            var vline3 = ToPen(skin.TimeLineVerticalLineColor3);
            var vline4 = ToPen(skin.TimeLineVerticalLineColor4);
            var vline5 = ToPen(skin.TimeLineVerticalLineColor5);
            var selection = ToBrush(skin.TimeLineSelectionAreaColor);
            var selectedmark = ToBrush(skin.TimeLineSelectedMarkColor);
            var holdExtent = ToBrush(skin.TimeLineHoldExtentColor);
            var black = ToPen(Color.Black);

            dc.DrawRectangle(parameter.Background, null, new Rect(0, 0, parameter.Width, parameter.Height));
            dc.DrawRectangle(area, null, new Rect(0, 0, parameter.Width, skin.TimeLineHeight));
            DrawLine(dc, border, 0, skin.TimeLineHeight - 1, parameter.Width, skin.TimeLineHeight - 1);
            var rows = WindowUtility.TimeLineForm.RowManager.OrderedVisibleRows;
            var visibles = WindowUtility.TimeLineForm.RowManager.Visibilities;
            //hline
            int iter = 0;
            foreach (var row in rows)
            {
                dc.DrawRectangle(GetRowBrush(row), null,
                    new Rect(0, skin.TimeLineHeight + skin.TimeLineRowHeight * iter + 1, parameter.Width, skin.TimeLineRowHeight));
                iter++;
            }
            if (parameter.Sheet != null)
            {
                //areaselection
                if (parameter.Sheet.RecStart.X != -1 && parameter.Sheet.RecStart.Y != -1)
                {
                    float startx = (parameter.Sheet.RecStart.X <= parameter.Sheet.RecEnd.X ? parameter.Sheet.RecStart.X : parameter.Sheet.RecEnd.X) * parameter.BPM / 60 * parameter.Interval;
                    float width = Math.Abs(parameter.Sheet.RecStart.X - parameter.Sheet.RecEnd.X) * parameter.BPM / 60 * parameter.Interval;
                    int starty = (parameter.Sheet.RecStart.Y <= parameter.Sheet.RecEnd.Y ? parameter.Sheet.RecStart.Y : parameter.Sheet.RecEnd.Y);
                    var height = Math.Min(rows.Length, Math.Abs(parameter.Sheet.RecStart.Y - parameter.Sheet.RecEnd.Y) + 1);
                    dc.DrawRectangle(selection, null, new Rect(startx - parameter.LeftOffset,
                        starty * skin.TimeLineRowHeight + skin.TimeLineHeight,
                        width, height * skin.TimeLineRowHeight + 1));
                }
            }
            iter = 0;
            foreach (var row in rows)
            {
                var y = skin.TimeLineHeight + skin.TimeLineRowHeight * (iter + 1);
                DrawLine(dc, hline, 0, y, parameter.Width, y);
                iter++;
            }
            int offset = -(int)(parameter.BPMStart * parameter.Interval / parameter.BPM);
            int startpos = offset + parameter.LeftOffset;
            int startcount = startpos / parameter.Interval;
            int startdrawpos = startcount * parameter.Interval - startpos;
            // 以前の GDI の 10 ポイント
            const double fontSize = 10 * 96.0 / 72;
            void VLine(Pen pen, int x, int top)
            {
                DrawLine(dc, pen, x, top, x, parameter.Height);
            }
            for (int i = 0; i * parameter.Interval + startdrawpos < parameter.Width; i++)
            {
                if (parameter.DisplayMode <= DisplayLineMode.SixtyFourth)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        if (parameter.DisplayMode >= DisplayLineMode.Fourth && j == 0)
                        {
                            VLine(vline1, startdrawpos + parameter.Interval * (4 * i + j) / 4, 0);
                        }
                        if (parameter.DisplayMode >= DisplayLineMode.Eigth && j == 2)
                        {
                            VLine(vline2, startdrawpos + parameter.Interval * (4 * i + j) / 4, 0);
                        }
                        if (parameter.DisplayMode >= DisplayLineMode.Sixteenth && (j == 1 || j == 3))
                        {
                            VLine(vline3, startdrawpos + parameter.Interval * (4 * i + j) / 4, 0);
                        }
                        if (parameter.DisplayMode >= DisplayLineMode.ThirtySecond)
                        {
                            VLine(vline4, startdrawpos + parameter.Interval * (8 * i + j * 2 + 1) / 8, 10);
                        }
                        if (parameter.DisplayMode >= DisplayLineMode.SixtyFourth)
                        {
                            VLine(vline5, startdrawpos + parameter.Interval * (16 * i + j * 4 + 1) / 16, 10);
                            VLine(vline5, startdrawpos + parameter.Interval * (16 * i + j * 4 + 3) / 16, 10);
                        }
                    }
                }
                else if (parameter.DisplayMode >= DisplayLineMode.Twelfth && parameter.DisplayMode <= DisplayLineMode.FourthEighth)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (j == 0)
                        {
                            VLine(vline1, startdrawpos + parameter.Interval * (4 * i + j) / 4, 0);
                        }
                        if (parameter.DisplayMode >= DisplayLineMode.Twelfth && (j == 1 || j == 2))
                        {
                            VLine(vline2, startdrawpos + parameter.Interval * (3 * i + j) / 3, 0);
                        }
                        if (parameter.DisplayMode >= DisplayLineMode.TwentyFourth)
                        {
                            VLine(vline3, startdrawpos + parameter.Interval * (6 * i + j * 2 + 1) / 6, 0);
                        }
                        if (parameter.DisplayMode >= DisplayLineMode.FourthEighth)
                        {
                            VLine(vline4, startdrawpos + parameter.Interval * (12 * i + j * 4 + 1) / 12, 10);
                            VLine(vline4, startdrawpos + parameter.Interval * (12 * i + j * 4 + 3) / 12, 10);
                        }
                    }
                }
                int count = (startcount + i).ToString().Length;
                if ((startcount + i) % parameter.BeatSplitCount == 0)
                {
                    var formatted = CreateText(((startcount + i) / parameter.BeatSplitCount).ToString(), numberTypeface, fontSize, text);
                    dc.DrawText(formatted, new Point(i * parameter.Interval + startdrawpos - formatted.Height / 2 * count / 2, 0));
                }
            }
            float drawstarttime = (float)(parameter.LeftOffset - 10) / parameter.Interval / parameter.BPM * 60;
            float drawendtime = (float)(parameter.LeftOffset + parameter.Width + 10) / parameter.Interval / parameter.BPM * 60;
            LayerViewModel[] layers = WindowUtility.LayerManager.AllLayerDisplay;
            if (layers != null)
            {
                foreach (var layer in layers)
                {
                    var sheet = layer.PPDData;
                    var mark = ToBrush(layer.MarkColor);
                    var diffmark = ToBrush(Color.FromArgb(64 * layer.MarkColor.A / 255, layer.MarkColor));
                    bool isSelectedLayer = sheet == WindowUtility.LayerManager.SelectedPpdSheet;
                    iter = 0;
                    foreach (var row in rows)
                    {
                        var times = sheet.Data[row].Keys;
                        for (var i = 0; i < times.Count; i++)
                        {
                            var f = times[i];
                            sheet.Data[row].TryGetValue(f, out Mark mk);
                            if (!visibles[(int)mk.Type])
                            {
                                continue;
                            }
                            var isFocused = isSelectedLayer && sheet.FocusedMark[0] == row && sheet.FocusedMark[1] == sheet.Data[row].IndexOfKey(f);
                            var rowCenter = skin.TimeLineHeight + skin.TimeLineRowHeight * (iter + 0.5f);
                            if (mk is not ExMark exmk)
                            {
                                if (f >= drawstarttime && f <= drawendtime)
                                {
                                    var x = f * parameter.BPM / 60 * parameter.Interval - parameter.LeftOffset;
                                    dc.DrawEllipse(isFocused ? selectedmark : (isSelectedLayer ? mark : diffmark), null,
                                        new Point(x, rowCenter), CircleWidth / 2.0, CircleWidth / 2.0);
                                }
                            }
                            else
                            {
                                var drawLongNote = (f >= drawstarttime && f <= drawendtime) || (exmk.EndTime >= drawstarttime && exmk.EndTime <= drawendtime);
                                var holdExtentTime = f + 5 + PPDSetting.Setting.GoodArea;
                                var drawHoldExtent = holdExtentTime >= drawstarttime && holdExtentTime <= drawendtime;

                                if (WindowUtility.Editor.ShowHoldExtent && (drawLongNote || drawHoldExtent))
                                {
                                    if (!exmk.IsScratch && (exmk.NoteType == PPDFramework.NoteType.AC || exmk.NoteType == PPDFramework.NoteType.ACFT))
                                    {
                                        var hasNextTime = i + 1 < times.Count;
                                        var height = skin.TimeLineRowHeight - 3;
                                        var rectX = f * parameter.BPM / 60 * parameter.Interval - parameter.LeftOffset;
                                        var rectY = rowCenter - height / 2;
                                        var width = (5 + PPDSetting.Setting.GoodArea) * parameter.BPM / 60 * parameter.Interval;
                                        var nextX = float.MaxValue;
                                        if (hasNextTime)
                                        {
                                            nextX = times[i + 1] * parameter.BPM / 60 * parameter.Interval - parameter.LeftOffset;
                                            if (rectX + width >= nextX)
                                            {
                                                width = nextX - rectX;
                                            }
                                        }
                                        int top = (int)rectY, bottom = (int)rectY + height;
                                        dc.DrawRectangle(holdExtent, null, new Rect((int)rectX, top, Math.Max(0, (int)width), height));
                                        void HoldLine(float lineTime, int inset)
                                        {
                                            var x = (int)(lineTime * parameter.BPM / 60 * parameter.Interval - parameter.LeftOffset);
                                            if (nextX > x)
                                            {
                                                DrawLine(dc, black, x, top + inset, x, bottom - inset);
                                            }
                                        }
                                        HoldLine(f + 5 + PPDSetting.Setting.GoodArea, 8);
                                        HoldLine(f + 5 + PPDSetting.Setting.CoolArea, 4);
                                        HoldLine(f + 5, 0);
                                        HoldLine(f + 5 - PPDSetting.Setting.CoolArea, 4);
                                        HoldLine(f + 5 - PPDSetting.Setting.GoodArea, 8);
                                    }
                                }

                                if (drawLongNote)
                                {
                                    var rect = new Rect((int)(f * parameter.BPM / 60 * parameter.Interval - parameter.LeftOffset),
                                        (int)(rowCenter - CircleWidth / 2),
                                        Math.Max(0, (int)((exmk.EndTime - f) * parameter.BPM / 60 * parameter.Interval)), CircleWidth);
                                    dc.DrawRectangle(isFocused ? selectedmark : (isSelectedLayer ? mark : diffmark), null, rect);
                                }
                            }
                        }
                        iter++;
                    }
                }
            }
            var events = WindowUtility.EventManager.GetEventsWithinTime(drawstarttime, drawendtime);
            foreach (float eventtime in events)
            {
                DrawChange(parameter, eventtime, 0, rows.Length, EventMark);
            }
            events = WindowUtility.SoundManager.GetSoundChangeWithinTime(drawstarttime, drawendtime);
            foreach (float eventtime in events)
            {
                DrawChange(parameter, eventtime, 14, rows.Length, SoundMark);
            }
            events = WindowUtility.KasiEditor.GetKasiChangeWithinTime(drawstarttime, drawendtime);
            foreach (float eventtime in events)
            {
                DrawChange(parameter, eventtime, 28, rows.Length, LylicsMark);
            }
            var pos = (float)(parameter.CurrentTime * parameter.BPM / 60 * parameter.Interval);
            if (pos - parameter.LeftOffset >= -5 && pos - parameter.LeftOffset <= parameter.Width + 5)
            {
                var x = pos - parameter.LeftOffset;
                dc.DrawLine(time, new Point(x + 0.5, skin.TimeLineHeight), new Point(x + 0.5, parameter.Height));
                dc.DrawRectangle(null, time, new Rect(x - 5 + 0.5, 0.5, 10, skin.TimeLineHeight - 1));
            }
        }

        private static void DrawChange(TimeLineDrawParameter parameter, float time, int offset, int rowCount, ImageSource image)
        {
            DrawIcon(parameter.DrawingContext, image, (int)((time * parameter.BPM / 60 * parameter.Interval - parameter.LeftOffset) - 8),
                PPDEditorSkin.Skin.TimeLineHeight + PPDEditorSkin.Skin.TimeLineRowHeight * rowCount + offset);
        }

        /// <summary>
        /// 印の画像を、画素の数どおりの大きさで書く。
        /// </summary>
        public static void DrawIcon(DrawingContext dc, ImageSource image, double x, double y)
        {
            var bitmap = (BitmapSource)image;
            dc.DrawImage(image, new Rect(x, y, bitmap.PixelWidth, bitmap.PixelHeight));
        }

        /// <summary>
        /// 1 ピクセルの線を、画素からはみ出さないように書く。
        /// </summary>
        public static void DrawLine(DrawingContext dc, Pen pen, double x1, double y1, double x2, double y2)
        {
            dc.DrawLine(pen, new Point(x1 + 0.5, y1 + 0.5), new Point(x2 + 0.5, y2 + 0.5));
        }

        /// <summary>
        /// スキンで決めた行の背景のブラシです。
        /// </summary>
        public static Brush GetRowBrush(int row)
        {
            var brush = PPDEditorSkin.Skin.TimeLineBackGroundColors[row] as System.Drawing.SolidBrush;
            return ToBrush(brush?.Color ?? Color.White);
        }

        public static FormattedText CreateText(string value, Typeface typeface, double size, Brush brush)
        {
            return new FormattedText(value, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, size, brush, 1);
        }

        public static Brush ToBrush(Color color)
        {
            return WpfSkin.ToBrush(color);
        }

        public static Pen ToPen(Color color)
        {
            var pen = new Pen(ToBrush(color), 1);
            pen.Freeze();
            return pen;
        }
    }
}
