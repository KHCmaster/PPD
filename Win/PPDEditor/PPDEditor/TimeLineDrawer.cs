using PPDEditor.Controls;
using PPDFramework;
using System;
using System.Drawing;

namespace PPDEditor
{
    class TimeLineDrawer
    {
        public const int CircleWidth = 10;

        //Pens and Brush
        Brush area = Brushes.LightGray;
        Pen border = Pens.Gray;
        Brush text = Brushes.Black;
        Pen hline = Pens.Gray;
        Pen time = Pens.Red;
        Pen vline1 = Pens.DarkSlateGray;
        Pen vline2 = Pens.Silver;
        Pen vline3 = new Pen(Color.FromArgb(204, 204, 204));
        Pen vline4 = Pens.Green;
        Pen vline5 = Pens.LightGreen;
        Brush selection = Brushes.SkyBlue;
        Brush mark = Brushes.Black;
        Brush selectedmark = Brushes.Blue;
        Brush diffmark = new SolidBrush(Color.FromArgb(128, 0, 0, 0));
        Brush holdExtent = new SolidBrush(Color.Orange);

        public void Draw(TimeLineDrawParameter parameter)
        {
            if (parameter == null)
            {
                return;
            }

            area = new SolidBrush(PPDEditorSkin.Skin.TimeLineSeekAreaColor);
            border = new Pen(PPDEditorSkin.Skin.TimeLineSeekAreaBorderColor);
            text = new SolidBrush(PPDEditorSkin.Skin.TimeLineTextColor);
            hline = new Pen(PPDEditorSkin.Skin.TimeLineHorizontalLineColor);
            time = new Pen(PPDEditorSkin.Skin.TimeLineCurrentTimeColor);
            vline1 = new Pen(PPDEditorSkin.Skin.TimeLineVerticalLineColor1);
            vline2 = new Pen(PPDEditorSkin.Skin.TimeLineVerticalLineColor2);
            vline3 = new Pen(PPDEditorSkin.Skin.TimeLineVerticalLineColor3);
            vline4 = new Pen(PPDEditorSkin.Skin.TimeLineVerticalLineColor4);
            vline5 = new Pen(PPDEditorSkin.Skin.TimeLineVerticalLineColor5);
            selection = new SolidBrush(PPDEditorSkin.Skin.TimeLineSelectionAreaColor);
            selectedmark = new SolidBrush(PPDEditorSkin.Skin.TimeLineSelectedMarkColor);
            holdExtent = new SolidBrush(PPDEditorSkin.Skin.TimeLineHoldExtentColor);

            parameter.Graphics.FillRectangle(Brushes.White, new Rectangle(0, 0, parameter.Width, parameter.Height));
            if (parameter.BackGroundImage != null && parameter.Width > 0 && parameter.Height > 0)
            {
                parameter.Graphics.DrawImage(parameter.BackGroundImage, 0, 0);
            }
            parameter.Graphics.FillRectangle(area, 0, 0, parameter.Width, PPDEditorSkin.Skin.TimeLineHeight);
            parameter.Graphics.DrawLine(border, 0, PPDEditorSkin.Skin.TimeLineHeight - 1, parameter.Width, PPDEditorSkin.Skin.TimeLineHeight - 1);
            var rows = WindowUtility.TimeLineForm.RowManager.OrderedVisibleRows;
            var visibles = WindowUtility.TimeLineForm.RowManager.Visibilities;
            //hline
            int iter = 0;
            foreach (var row in rows)
            {
                parameter.Graphics.FillRectangle(PPDEditorSkin.Skin.TimeLineBackGroundColors[row],
                       new Rectangle(0, PPDEditorSkin.Skin.TimeLineHeight + PPDEditorSkin.Skin.TimeLineRowHeight * iter + 1,
                           parameter.Width, PPDEditorSkin.Skin.TimeLineRowHeight));
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
                    parameter.Graphics.FillRectangle(selection, startx - parameter.LeftOffset,
                          starty * (PPDEditorSkin.Skin.TimeLineRowHeight) + PPDEditorSkin.Skin.TimeLineHeight,
                          width, height * (PPDEditorSkin.Skin.TimeLineRowHeight) + 1);
                }
            }
            iter = 0;
            foreach (var row in rows)
            {
                parameter.Graphics.DrawLine(hline, 0,
                    PPDEditorSkin.Skin.TimeLineHeight + PPDEditorSkin.Skin.TimeLineRowHeight * (iter + 1),
                    parameter.Width, PPDEditorSkin.Skin.TimeLineHeight + PPDEditorSkin.Skin.TimeLineRowHeight * (iter + 1));
                iter++;
            }
            int offset = -(int)(parameter.BPMStart * parameter.Interval / parameter.BPM);
            int startpos = offset + parameter.LeftOffset;
            int startcount = startpos / parameter.Interval;
            int startdrawpos = startcount * parameter.Interval - startpos;
            var font = new System.Drawing.Font("ＭＳ ゴシック", 10);
            for (int i = 0; i * parameter.Interval + startdrawpos < parameter.Width; i++)
            {
                if (parameter.DisplayMode <= DisplayLineMode.SixtyFourth)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        if (parameter.DisplayMode >= DisplayLineMode.Fourth && j == 0)
                        {
                            parameter.Graphics.DrawLine(vline1, startdrawpos + parameter.Interval * (4 * i + j) / 4, 0, startdrawpos + parameter.Interval * (4 * i + j) / 4, parameter.Height);
                        }
                        if (parameter.DisplayMode >= DisplayLineMode.Eigth && j == 2)
                        {
                            parameter.Graphics.DrawLine(vline2, startdrawpos + parameter.Interval * (4 * i + j) / 4, 0, startdrawpos + parameter.Interval * (4 * i + j) / 4, parameter.Height);
                        }
                        if (parameter.DisplayMode >= DisplayLineMode.Sixteenth && (j == 1 || j == 3))
                        {
                            parameter.Graphics.DrawLine(vline3, startdrawpos + parameter.Interval * (4 * i + j) / 4, 0, startdrawpos + parameter.Interval * (4 * i + j) / 4, parameter.Height);
                        }
                        if (parameter.DisplayMode >= DisplayLineMode.ThirtySecond)
                        {
                            parameter.Graphics.DrawLine(vline4, startdrawpos + parameter.Interval * (8 * i + j * 2 + 1) / 8, 10, startdrawpos + parameter.Interval * (8 * i + j * 2 + 1) / 8, parameter.Height);
                        }
                        if (parameter.DisplayMode >= DisplayLineMode.SixtyFourth)
                        {
                            parameter.Graphics.DrawLine(vline5, startdrawpos + parameter.Interval * (16 * i + j * 4 + 1) / 16, 10, startdrawpos + parameter.Interval * (16 * i + j * 4 + 1) / 16, parameter.Height);
                            parameter.Graphics.DrawLine(vline5, startdrawpos + parameter.Interval * (16 * i + j * 4 + 3) / 16, 10, startdrawpos + parameter.Interval * (16 * i + j * 4 + 3) / 16, parameter.Height);
                        }
                    }
                }
                else if (parameter.DisplayMode >= DisplayLineMode.Twelfth && parameter.DisplayMode <= DisplayLineMode.FourthEighth)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (j == 0)
                        {
                            parameter.Graphics.DrawLine(vline1, startdrawpos + parameter.Interval * (4 * i + j) / 4, 0, startdrawpos + parameter.Interval * (4 * i + j) / 4, parameter.Height);
                        }
                        if (parameter.DisplayMode >= DisplayLineMode.Twelfth && (j == 1 || j == 2))
                        {
                            parameter.Graphics.DrawLine(vline2, startdrawpos + parameter.Interval * (3 * i + j) / 3, 0, startdrawpos + parameter.Interval * (3 * i + j) / 3, parameter.Height);
                        }
                        if (parameter.DisplayMode >= DisplayLineMode.TwentyFourth)
                        {
                            parameter.Graphics.DrawLine(vline3, startdrawpos + parameter.Interval * (6 * i + j * 2 + 1) / 6, 0, startdrawpos + parameter.Interval * (6 * i + j * 2 + 1) / 6, parameter.Height);
                        }
                        if (parameter.DisplayMode >= DisplayLineMode.FourthEighth)
                        {
                            parameter.Graphics.DrawLine(vline4, startdrawpos + parameter.Interval * (12 * i + j * 4 + 1) / 12, 10, startdrawpos + parameter.Interval * (12 * i + j * 4 + 1) / 12, parameter.Height);
                            parameter.Graphics.DrawLine(vline4, startdrawpos + parameter.Interval * (12 * i + j * 4 + 3) / 12, 10, startdrawpos + parameter.Interval * (12 * i + j * 4 + 3) / 12, parameter.Height);
                        }
                    }
                }
                int count = (startcount + i).ToString().Length;
                if ((startcount + i) % parameter.BeatSplitCount == 0)
                {
                    parameter.Graphics.DrawString(((startcount + i) / parameter.BeatSplitCount).ToString(), font, text, new PointF(i * parameter.Interval + startdrawpos - font.GetHeight() / 2 * count / 2, 0));
                }
            }
            font.Dispose();
            float drawstarttime = (float)(parameter.LeftOffset - 10) / parameter.Interval / parameter.BPM * 60;
            float drawendtime = (float)(parameter.LeftOffset + parameter.Width + 10) / parameter.Interval / parameter.BPM * 60;
            LayerDisplay[] layers = WindowUtility.LayerManager.AllLayerDisplay;
            if (layers != null)
            {
                foreach (var layer in layers)
                {
                    var sheet = layer.PPDData;
                    mark = new SolidBrush(layer.MarkColor);
                    diffmark = new SolidBrush(Color.FromArgb(64 * layer.MarkColor.A / 255, layer.MarkColor));
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
                            var exmk = mk as ExMark;
                            if (exmk == null)
                            {
                                if (f >= drawstarttime && f <= drawendtime)
                                {
                                    if (isSelectedLayer && sheet.FocusedMark[0] == row && sheet.FocusedMark[1] == sheet.Data[row].IndexOfKey(f))
                                    {
                                        parameter.Graphics.FillEllipse(selectedmark, (f * parameter.BPM / 60 * parameter.Interval - parameter.LeftOffset) - CircleWidth / 2,
                                            PPDEditorSkin.Skin.TimeLineHeight + PPDEditorSkin.Skin.TimeLineRowHeight * (iter + 0.5f) - CircleWidth / 2, CircleWidth, CircleWidth);
                                    }
                                    else
                                    {
                                        parameter.Graphics.FillEllipse(isSelectedLayer ? mark : diffmark, (f * parameter.BPM / 60 * parameter.Interval - parameter.LeftOffset) - CircleWidth / 2,
                                            PPDEditorSkin.Skin.TimeLineHeight + PPDEditorSkin.Skin.TimeLineRowHeight * (iter + 0.5f) - CircleWidth / 2,
                                            CircleWidth, CircleWidth);
                                    }
                                }
                            }
                            else
                            {
                                var drawLongNote = (f >= drawstarttime && f <= drawendtime) || (exmk.EndTime >= drawstarttime && exmk.EndTime <= drawendtime);
                                var holdExtentTime = f + 5 + PPDSetting.Setting.GoodArea;
                                var drawHoldExtent = holdExtentTime >= drawstarttime && holdExtentTime <= drawendtime;

                                if (WindowUtility.MainForm.ShowHoldExtent && (drawLongNote || drawHoldExtent))
                                {
                                    if (!exmk.IsScratch && (exmk.NoteType == PPDFramework.NoteType.AC || exmk.NoteType == PPDFramework.NoteType.ACFT))
                                    {
                                        var hasNextTime = i + 1 < times.Count;
                                        var height = PPDEditorSkin.Skin.TimeLineRowHeight - 3;
                                        var rectX = f * parameter.BPM / 60 * parameter.Interval - parameter.LeftOffset;
                                        var rectY = PPDEditorSkin.Skin.TimeLineHeight + PPDEditorSkin.Skin.TimeLineRowHeight * (iter + 0.5f) - height / 2;
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
                                        var rect = new Rectangle((int)rectX, (int)rectY, (int)width, height);
                                        parameter.Graphics.FillRectangle(holdExtent, rect);
                                        var x = (int)((f + 5 + PPDSetting.Setting.GoodArea) * parameter.BPM / 60 * parameter.Interval - parameter.LeftOffset);
                                        if (nextX > x)
                                        {
                                            parameter.Graphics.DrawLine(Pens.Black, x, rect.Top + 8, x, rect.Bottom - 8);
                                        }
                                        x = (int)((f + 5 + PPDSetting.Setting.CoolArea) * parameter.BPM / 60 * parameter.Interval - parameter.LeftOffset);
                                        if (nextX > x)
                                        {
                                            parameter.Graphics.DrawLine(Pens.Black, x, rect.Top + 4, x, rect.Bottom - 4);
                                        }
                                        x = (int)((f + 5) * parameter.BPM / 60 * parameter.Interval - parameter.LeftOffset);
                                        if (nextX > x)
                                        {
                                            parameter.Graphics.DrawLine(Pens.Black, x, rect.Top, x, rect.Bottom);
                                        }
                                        x = (int)((f + 5 - PPDSetting.Setting.CoolArea) * parameter.BPM / 60 * parameter.Interval - parameter.LeftOffset);
                                        if (nextX > x)
                                        {
                                            parameter.Graphics.DrawLine(Pens.Black, x, rect.Top + 4, x, rect.Bottom - 4);
                                        }
                                        x = (int)((f + 5 - PPDSetting.Setting.GoodArea) * parameter.BPM / 60 * parameter.Interval - parameter.LeftOffset);
                                        if (nextX > x)
                                        {
                                            parameter.Graphics.DrawLine(Pens.Black, x, rect.Top + 8, x, rect.Bottom - 8);
                                        }
                                    }
                                }

                                if (drawLongNote)
                                {
                                    var rect = new Rectangle((int)(f * parameter.BPM / 60 * parameter.Interval - parameter.LeftOffset),
                                            (int)(PPDEditorSkin.Skin.TimeLineHeight + PPDEditorSkin.Skin.TimeLineRowHeight * (iter + 0.5f) - CircleWidth / 2),
                                            (int)((exmk.EndTime - f) * parameter.BPM / 60 * parameter.Interval), CircleWidth);
                                    if (isSelectedLayer && sheet.FocusedMark[0] == row && sheet.FocusedMark[1] == sheet.Data[row].IndexOfKey(f))
                                    {
                                        parameter.Graphics.FillRectangle(selectedmark, rect);
                                    }
                                    else
                                    {
                                        parameter.Graphics.FillRectangle(isSelectedLayer ? mark : diffmark, rect);
                                    }
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
                DrawChange(parameter, eventtime, 0, rows.Length, PPDEditor.Properties.Resources.eventmark);
            }
            events = WindowUtility.SoundManager.GetSoundChangeWithinTime(drawstarttime, drawendtime);
            foreach (float eventtime in events)
            {
                DrawChange(parameter, eventtime, 14, rows.Length, PPDEditor.Properties.Resources.soundmark);
            }
            events = WindowUtility.KasiEditor.GetKasiChangeWithinTime(drawstarttime, drawendtime);
            foreach (float eventtime in events)
            {
                DrawChange(parameter, eventtime, 28, rows.Length, PPDEditor.Properties.Resources.lylicsmark);
            }
            var pos = (float)(parameter.CurrentTime * parameter.BPM / 60 * parameter.Interval);
            if (pos - parameter.LeftOffset >= -5 && pos - parameter.LeftOffset <= parameter.Width + 5)
            {
                parameter.Graphics.DrawLine(time, new PointF(pos - parameter.LeftOffset, PPDEditorSkin.Skin.TimeLineHeight), new PointF(pos - parameter.LeftOffset, parameter.Height));
                parameter.Graphics.DrawRectangle(time, pos - 5 - parameter.LeftOffset, 0, 10, PPDEditorSkin.Skin.TimeLineHeight - 1);
            }
        }

        private void DrawChange(TimeLineDrawParameter parameter, float time, int offset, int rowCount, Image image)
        {
            parameter.Graphics.DrawImage(image, new Point((int)((time * parameter.BPM / 60 * parameter.Interval - parameter.LeftOffset) - 8),
                PPDEditorSkin.Skin.TimeLineHeight + PPDEditorSkin.Skin.TimeLineRowHeight * rowCount + offset));
        }
    }
}
