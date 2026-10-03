using PPDEditor.ViewModels.Dialogs;
using BezierCaliculator;
using BezierDrawControl;
using PPDEditor.Command.PPDSheet;
using PPDEditor.Controls;
using PPDEditor.Forms;
using System.Numerics;
using PPDMath;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DrawingColor = System.Drawing.Color;
using PointF = System.Drawing.PointF;
using RectangleF = System.Drawing.RectangleF;

namespace PPDEditor
{
    public partial class GeometryCreator : DockView
    {
        const int StartX = 100;
        const int StartY = 50;

        string error1 = "整数ではありません";
        string error2 = "3以上の数値を入力してください";
        Vector2[] poses = [];
        Vector2[] dirs = [];

        string gchintnormal = @"クリック  アンカー追加
Ctrl+クリック  アンカー選択
Ctrl+ドラッグ  アンカー,ハンドル移動
Shift+ドラッグ　アンカー制限付き移動
Delete  (選択後)アンカー削除
Alt+ドラッグ(アンカー)  ハンドル再定義
Alt+ドラッグ(ハンドル)  ハンドル移動
Ctrl+T  変形モード
Ctrl+Z  アンドゥ
Ctrl+Y  リドゥ";
        string gchinttrans = @"ドラッグ  回転
ドラッグ(□上)  拡大(縮小)
Shift+ドラッグ(□上)  アス比固定拡大(縮小)
Ctrl+ドラッグ  移動
Enter  変形終了";

        bool ignore;
        public GeometryCreator()
        {
            InitializeComponent();
            bezierControl1.Controller.BeforePaint += BezierControl1_BeforePaint;
            bezierControl1.Controller.AfterPaint += BezierControl1_AfterPaint;
            bezierControl1.Controller.PreprocessPoint += BezierControl1_PreprocessPoint;
            SquareGrid = new SquareGrid
            {
                GridColor = System.Drawing.Color.Green,
                Height = 25,
                Width = 25,
                OffsetX = 0,
                OffsetY = 0
            };
        }

        void ToolStripTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!ignore && IsInitialized)
            {
                var scale = new PointF(GetValue(this.toolStripTextBox1.Text, "%") / 100, GetValue(this.toolStripTextBox2.Text, "%") / 100);
                var move = new PointF(GetValue(this.toolStripTextBox4.Text, "px"), GetValue(this.toolStripTextBox5.Text, "px"));
                var rotation = GetValue(toolStripTextBox3.Text, "");
                bezierControl1.Controller.TransformRotation = rotation;
                bezierControl1.Controller.TransformScale = scale;
                bezierControl1.Controller.TransformTranslation = move;
                bezierControl1.DrawAndRefresh();
            }
        }
        private static float GetValue(string data, string enddata)
        {
            if (data == null || enddata == null) return 0;
            float ret;
            if (data.EndsWith(enddata))
            {
                var temp = data[..^enddata.Length];
                if (!float.TryParse(temp, out ret)) ret = 0;
            }
            else
            {
                if (!float.TryParse(data, out ret)) ret = 0;
            }
            return ret;
        }

        private void Redraw(object sender, RoutedEventArgs e)
        {
            bezierControl1?.DrawAndRefresh();
        }
        public void SetLang()
        {
            this.label1.Text = Utility.Language["GCLabel1"];
            this.label2.Text = Utility.Language["GCLabel2"];
            this.checkBox1.Content = Utility.Language["GCCheckBox1"];
            this.checkBox2.Content = Utility.Language["GCCheckBox2"];
            this.checkBox3.Content = Utility.Language["GCCheckBox3"];
            this.checkBox4.Content = Utility.Language["GCCheckBox4"];
            this.groupBox1.Header = Utility.Language["GCGroup1"];
            this.groupBox2.Header = Utility.Language["GCGroup2"];
            this.groupBox3.Header = Utility.Language["GCGroup3"];
            this.radioButton1.Content = Utility.Language["GCRadioButton1"];
            this.radioButton2.Content = Utility.Language["GCRadioButton2"];
            this.radioButton3.Content = Utility.Language["GCRadioButton3"];
            this.radioButton4.Content = Utility.Language["GCRadioButton4"];
            this.radioButton5.Content = Utility.Language["GCRadioButton5"];
            this.radioButton6.Content = Utility.Language["GCRadioButton6"];
            this.radioButton7.Content = Utility.Language["GCRadioButton7"];
            this.radioButton8.Content = Utility.Language["GCRadioButton8"];
            this.radioButton9.Content = Utility.Language["GCRadioButton9"];
            this.radioButton10.Content = Utility.Language["GCRadioButton10"];
            this.radioButton11.Content = Utility.Language["GCRadioButton11"];
            this.button1.Content = Utility.Language["GCButton1"];
            this.button2.Content = Utility.Language["GCButton2"];
            this.button3.Content = Utility.Language["GCButton3"];
            error1 = Utility.Language["GCError1"];
            error2 = Utility.Language["GCError2"];
            gchintnormal = Utility.Language["GCHint1"];
            gchinttrans = Utility.Language["GCHint2"];
            this.toolStripButton4.Content = Utility.Language["GCSetDefault"];
            this.toolStripButton4.ToolTip = Utility.Language["GCSetDefault"];
        }
        public void SetSkin()
        {
            var brush = WpfSkin.TransparentTextBrush;
            foreach (var control in new Control[] { checkBox1, checkBox2, checkBox3, checkBox4, checkBox5, groupBox1, groupBox2, groupBox3,
                radioButton1, radioButton2, radioButton3, radioButton4, radioButton5, radioButton6, radioButton7, radioButton8, radioButton9, radioButton10 })
            {
                control.Foreground = brush;
            }
            this.label1.Foreground = brush;
        }

        private void BezierControl1_BeforePaint(object sender, UserPaintEventArgs e)
        {
            var context = e.Context;
            if (SquareGrid != null && SquareGridEnabled)
            {
                var width = (int)bezierControl1.ActualWidth;
                var height = (int)bezierControl1.ActualHeight;
                for (int i = (-SquareGrid.NormalizedOffsetX - StartX) / SquareGrid.Width; i <= width / SquareGrid.Width; i++)
                {
                    context.DrawLine(SquareGrid.GridColor, new PointF(i * SquareGrid.Width + SquareGrid.OffsetX + StartX, 0),
                        new PointF(i * SquareGrid.Width + SquareGrid.OffsetX + StartX, height));
                }
                for (int i = (-SquareGrid.NormalizedOffsetY - StartY) / SquareGrid.Height; i <= height / SquareGrid.Height; i++)
                {
                    context.DrawLine(SquareGrid.GridColor, new PointF(0, i * SquareGrid.Height + SquareGrid.OffsetY + StartY),
                        new PointF(width, i * SquareGrid.Height + SquareGrid.OffsetY + StartY));
                }
            }
        }

        private void BezierControl1_AfterPaint(object sender, UserPaintEventArgs e)
        {
            var context = e.Context;
            CheckData();
            if (bezierControl1.Controller.BCPSCount < 2 || bezierControl1.Controller.IsTransformMode)
            {
                if (bezierControl1.Controller.IsTransformMode) CheckTransformValue();
            }
            else
            {
                if (checkBox2.IsChecked == true)
                {
                    foreach (var pos in poses)
                    {
                        context.DrawEllipse(pos == poses[0] ? DrawingColor.Green : DrawingColor.Black, new RectangleF(pos.X - 5, pos.Y - 5, 10, 10));
                    }
                }
                if (checkBox3.IsChecked == true && radioButton7.IsChecked != true)
                {
                    for (int i = 0; i < dirs.Length; i++)
                    {
                        DrawAllow(new PointF(poses[i].X, poses[i].Y), new PointF(dirs[i].X, dirs[i].Y), context);
                    }
                }
            }

            context.DrawRectangle(DrawingColor.Red, StartX, StartY, 400, 225);
        }
        private static void DrawAllow(PointF pos, PointF dir, IBezierDrawContext context)
        {
            float length = 100;
            float length2 = 30;
            context.DrawLine(DrawingColor.Blue, pos, new PointF(pos.X - dir.X * length, pos.Y - dir.Y * length));
            var r1 = Rotate(new Vector2(dir.X, dir.Y), (float)(Math.PI / 6));
            var r2 = Rotate(new Vector2(dir.X, dir.Y), (float)(-Math.PI / 6));
            context.DrawLine(DrawingColor.Blue, pos, new PointF(pos.X - r1.X * length2, pos.Y - r1.Y * length2));
            context.DrawLine(DrawingColor.Blue, pos, new PointF(pos.X - r2.X * length2, pos.Y - r2.Y * length2));
        }
        private static Vector2 Rotate(Vector2 p, float r)
        {
            return new Vector2((float)(Math.Cos(r) * p.X - Math.Sin(r) * p.Y), (float)(Math.Sin(r) * p.X + Math.Cos(r) * p.Y));
        }
        private void CheckData()
        {
            if (WindowUtility.LayerManager == null)
            {
                return;
            }

            PPDSheet data = WindowUtility.LayerManager.SelectedPpdSheet;
            if (data == null)
            {
                poses = [];
                dirs = [];
                return;
            }
            //areaselection
            var mks = data.GetAreaData();
            poses = new Vector2[mks.Length];
            dirs = new Vector2[mks.Length];
            float[] ratios = new float[mks.Length];
            if (bezierControl1.Controller.BCPSCount >= 2 && poses.Length >= 2)
            {
                if (radioButton8.IsChecked == true)
                {
                    float all = mks[^1].Time - mks[0].Time;
                    if (all == 0)
                    {
                        poses = [];
                        dirs = [];
                        return;
                    }
                    for (int i = 0; i < mks.Length; i++)
                    {
                        ratios[i] = 99.9999f * (mks[i].Time - mks[0].Time) / all;
                    }
                }
                BezierControlPoint[] bcps = bezierControl1.Controller.BCPS;
                var ba = new BezierAnalyzer(bcps);
                for (int i = 0; i < poses.Length; i++)
                {
                    PointF pos;
                    PointF dir;
                    if (radioButton8.IsChecked == true)
                    {
                        ba.GetPoint(ratios[i], out pos, out dir);
                    }
                    else if (radioButton9.IsChecked == true)
                    {
                        ba.GetPoint(i * 99.9999f / (poses.Length - 1), out pos, out dir);
                    }
                    else if (radioButton10.IsChecked == true)
                    {
                        ba.GetPoint(i * 99.9999f / poses.Length, out pos, out dir);
                    }
                    else
                    {
                        float num = (float)60 / (WindowUtility.MainForm.BPM <= 10 ? 10 : WindowUtility.MainForm.BPM);
                        float ratio = ((mks[i].Time - mks[0].Time) / num * FixedDistance) / ba.Length * BezierCaliculator.BezierAnalyzer.MaxRatio;
                        ratio = ratio <= 0 ? 0 : ratio;
                        ba.GetPoint(ratio >= BezierCaliculator.BezierAnalyzer.MaxRatio ? BezierCaliculator.BezierAnalyzer.MaxRatio : ratio, out pos, out dir);
                    }
                    poses[i] = new Vector2(pos.X, pos.Y);
                    dirs[i] = TransDirection(new Vector2(dir.X, dir.Y));
                }
            }
            if (checkBox4.IsChecked == true)
            {
                Apply();
            }
        }

        private Vector2 TransDirection(Vector2 dir)
        {
            if (radioButton3.IsChecked == true)
            {
                return dir;
            }
            else if (radioButton4.IsChecked == true)
            {
                return Rotate(dir, (float)Math.PI / 2);
            }
            else if (radioButton5.IsChecked == true)
            {
                return Rotate(dir, (float)Math.PI);
            }
            else if (radioButton6.IsChecked == true)
            {
                return Rotate(dir, (float)Math.PI / 2 * 3);
            }
            return dir;
        }
        private void GeometryCreator_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // 文字を入力しているときは、その入力を優先する
            if (e.OriginalSource is TextBox)
            {
                return;
            }
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                switch (e.Key)
                {
                    case Key.Z:
                        bezierControl1.Controller.Undo();
                        e.Handled = true;
                        break;
                    case Key.Y:
                        bezierControl1.Controller.Redo();
                        e.Handled = true;
                        break;
                    case Key.T:
                        toolStrip1.Visibility = Visibility.Visible;
                        bezierControl1.Controller.StartTransform();
                        e.Handled = true;
                        break;
                }
            }
            else
            {
                if (e.Key == Key.Delete)
                {
                    bezierControl1.Controller.Delete();
                    e.Handled = true;
                }
                if (e.Key == Key.Return)
                {
                    bezierControl1.Controller.EndTransform();
                    toolStrip1.Visibility = Visibility.Collapsed;
                    e.Handled = true;
                }
            }
        }
        private void CheckTransformValue()
        {
            ignore = true;
            PointF scale = bezierControl1.Controller.TransformScale;
            PointF move = bezierControl1.Controller.TransformTranslation;
            float rotation = bezierControl1.Controller.TransformRotation;
            this.toolStripTextBox1.Text = (scale.X * 100) + "%";
            this.toolStripTextBox2.Text = (scale.Y * 100) + "%";
            this.toolStripTextBox3.Text = rotation.ToString();
            this.toolStripTextBox4.Text = move.X + "px";
            this.toolStripTextBox5.Text = move.Y + "px";
            ignore = false;
        }

        private void CheckBox1_CheckedChanged(object sender, RoutedEventArgs e)
        {
            bezierControl1.AntialiasEnabled = checkBox1.IsChecked == true;
            bezierControl1.DrawAndRefresh();
        }
        private void CheckBox4_CheckedChanged(object sender, RoutedEventArgs e)
        {
            button1.IsEnabled = checkBox4.IsChecked != true;
        }

        private void Label1_ValueChange(object sender, ValueChangeEventArgs e)
        {
            if (int.TryParse(textBox1.Text, out int value))
            {
                textBox1.Text = (value + e.ChangedValue).ToString();
            }
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            if (radioButton1.IsChecked == true)
            {
                CreateCircle();
            }
            else if (radioButton2.IsChecked == true)
            {
                if (int.TryParse(textBox1.Text, out int num))
                {
                    if (num < 3)
                    {
                        MessageBox.Show(Window.GetWindow(this), error2);
                    }
                    else
                    {
                        CreatePolygon(num);
                    }
                }
                else
                {
                    MessageBox.Show(Window.GetWindow(this), error1);
                }
            }
        }
        private void CreateCircle()
        {
            float temp = 0.5522847f;
            var data = new List<BezierControlPoint>();
            float radius = 60;
            float centerx = 300;
            float centery = 162;
            var bcp = new BezierControlPoint
            {
                Second = new PointF(centerx + radius, centery),
                First = new PointF(centerx + radius, centery - radius * temp),
                Third = new PointF(centerx + radius, centery + radius * temp),
                ValidFirst = true,
                ValidThird = true
            };
            data.Add(bcp);
            bcp = new BezierControlPoint
            {
                Second = new PointF(centerx, centery + radius),
                First = new PointF(centerx + radius * temp, centery + radius),
                Third = new PointF(centerx + -radius * temp, centery + radius),
                ValidFirst = true,
                ValidThird = true
            };
            data.Add(bcp);
            bcp = new BezierControlPoint
            {
                Second = new PointF(centerx + -radius, centery),
                First = new PointF(centerx + -radius, centery + radius * temp),
                Third = new PointF(centerx + -radius, centery - radius * temp),
                ValidFirst = true,
                ValidThird = true
            };
            data.Add(bcp);
            bcp = new BezierControlPoint
            {
                Second = new PointF(centerx, centery - radius),
                First = new PointF(centerx - radius * temp, centery - radius),
                Third = new PointF(centerx + radius * temp, centery - radius),
                ValidFirst = true,
                ValidThird = true
            };
            data.Add(bcp);
            data.Add(data[0]);
            bezierControl1.Controller.BCPS = [.. data];
        }
        private void CreatePolygon(int num)
        {
            var data = new List<BezierControlPoint>();
            float radius = 60;
            float centerx = 300;
            float centery = 162;
            for (int i = 0; i < num; i++)
            {
                var bcp = new BezierControlPoint();
                var cos = (float)Math.Cos(Math.PI * 2 * i / num);
                var sin = (float)Math.Sin(Math.PI * 2 * i / num);
                bcp.Second = new PointF(centerx + radius * cos, centery + radius * sin);
                data.Add(bcp);
            }
            data.Add(data[0]);
            bezierControl1.Controller.BCPS = [.. data];
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            if (!bezierControl1.Controller.IsTransformMode) bezierControl1.Controller.Clear();
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            Apply();
        }
        private bool Apply()
        {
            if (bezierControl1.Controller.BCPSCount < 2) return false;
            PPDSheet data = WindowUtility.LayerManager.SelectedPpdSheet;
            if (data == null)
            {
                return false;
            }
            //areaselection
            data.ApplyTrans(poses, dirs, !radioButton7.IsChecked == true);
            return true;
        }

        public bool Antialias
        {
            get
            {
                return checkBox1.IsChecked == true;
            }
            set
            {
                checkBox1.IsChecked = value;
            }
        }
        public bool DrawPos
        {

            get
            {
                return checkBox2.IsChecked == true;
            }
            set
            {
                checkBox2.IsChecked = value;
            }
        }
        public bool DrawAngle
        {

            get
            {
                return checkBox3.IsChecked == true;
            }
            set
            {
                checkBox3.IsChecked = value;
            }
        }
        public bool ApplyMoved
        {
            get
            {
                return checkBox4.IsChecked == true;
            }
            set
            {
                checkBox4.IsChecked = value;
            }
        }
        public int FixedDistance
        {
            get
            {
                if (!int.TryParse(this.textBox2.Text, out int ret)) ret = 0;
                return ret;
            }
            set
            {
                this.textBox2.Text = value.ToString();
            }
        }

        public int AngleRestriction
        {
            get
            {
                return (int)numericUpDown1.Value;
            }
            set
            {
                numericUpDown1.Value = value;
            }
        }

        public SquareGrid SquareGrid
        {
            get;
            set;
        }

        public bool SquareGridEnabled
        {
            get
            {
                return checkBox5.IsChecked == true;
            }
            set
            {
                checkBox5.IsChecked = value;
            }
        }

        private void TextBox2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (FixedDistance != 0) bezierControl1?.DrawAndRefresh();
        }

        private void RadioButton11_CheckedChanged(object sender, RoutedEventArgs e)
        {
            textBox2.IsEnabled = radioButton11.IsChecked == true;
            bezierControl1.DrawAndRefresh();
        }

        private void BezierControl1_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            // マウスを止めてしばらくすると、今のモードの操作方法を出す
            toolTip1.Content = bezierControl1.Controller.IsTransformMode ? gchinttrans : gchintnormal;
        }

        private void ToolStripButton4_Click(object sender, RoutedEventArgs e)
        {
            if (bezierControl1.Controller.IsTransformMode)
            {
                bezierControl1.Controller.TransformRotation = 0;
                bezierControl1.Controller.TransformScale = new PointF(1, 1);
                bezierControl1.Controller.TransformTranslation = new PointF(0, 0);
                bezierControl1.DrawAndRefresh();
            }
        }

        private void NumericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDown1.Value == 0)
            {
                bezierControl1.Controller.RestrictAngleSplit = 0;
            }
            else
            {
                bezierControl1.Controller.RestrictAngleSplit = 90 / (int)numericUpDown1.Value;
            }
        }

        private void Button4_Click(object sender, RoutedEventArgs e)
        {
            var gsf = new GridSettingViewModel(new PPDEditor.SquareGrid
            {
                Width = SquareGrid.Width * 2,
                Height = SquareGrid.Height * 2,
                GridColor = SquareGrid.GridColor,
                OffsetX = SquareGrid.OffsetX * 2,
                OffsetY = SquareGrid.OffsetY * 2
            });
            if (WpfDialog.ShowDialog(new GridSettingWindow(gsf)))
            {
                SquareGrid = new SquareGrid
                {
                    Width = gsf.Grid.Width / 2,
                    Height = gsf.Grid.Height / 2,
                    GridColor = gsf.Grid.GridColor,
                    OffsetX = gsf.Grid.OffsetX / 2,
                    OffsetY = gsf.Grid.OffsetY / 2
                };
                bezierControl1.DrawAndRefresh();
            }
        }

        private PointF BezierControl1_PreprocessPoint(PointF arg)
        {
            if (SquareGrid == null || !SquareGridEnabled)
            {
                return arg;
            }

            arg.X -= StartX;
            arg.Y -= StartY;

            int offsetx = SquareGrid.NormalizedOffsetX;
            int offsety = SquareGrid.NormalizedOffsetY;
            var nearest = new PointF((int)((arg.X + SquareGrid.Width / 2f) / SquareGrid.Width) * SquareGrid.Width + offsetx,
                (int)((arg.Y + SquareGrid.Height / 2f) / SquareGrid.Height) * SquareGrid.Height + offsety);
            var diff = new PointF(nearest.X - arg.X, nearest.Y - arg.Y);

            if (Math.Sqrt(diff.X * diff.X + diff.Y * diff.Y) >= 10)
            {
                return new PointF(arg.X + StartX, arg.Y + StartY);
            }

            return new PointF(nearest.X + StartX, nearest.Y + StartY);
        }
    }
}
