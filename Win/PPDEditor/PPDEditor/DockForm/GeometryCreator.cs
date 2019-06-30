using BezierCaliculator;
using BezierDrawControl;
using PPDEditor.Command.PPDSheet;
using PPDEditor.Controls;
using PPDEditor.Forms;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace PPDEditor
{
    public partial class GeometryCreator : ScrollableForm
    {
        const int StartX = 100;
        const int StartY = 50;

        string error1 = "整数ではありません";
        string error2 = "3以上の数値を入力してください";
        Vector2[] poses = new Vector2[0];
        Vector2[] dirs = new Vector2[0];

        Timer hinttimergc;
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
            radioButton3.CheckedChanged += radioButtongroup2_CheckedChanged;
            radioButton4.CheckedChanged += radioButtongroup2_CheckedChanged;
            radioButton5.CheckedChanged += radioButtongroup2_CheckedChanged;
            radioButton6.CheckedChanged += radioButtongroup2_CheckedChanged;
            radioButton7.CheckedChanged += radioButtongroup2_CheckedChanged;
            radioButton8.CheckedChanged += radioButtongroup2_CheckedChanged;
            radioButton9.CheckedChanged += radioButtongroup2_CheckedChanged;
            radioButton10.CheckedChanged += radioButtongroup2_CheckedChanged;
            this.toolStripTextBox1.TextChanged += toolStripTextBox1_TextChanged;
            this.toolStripTextBox2.TextChanged += toolStripTextBox1_TextChanged;
            this.toolStripTextBox3.TextChanged += toolStripTextBox1_TextChanged;
            this.toolStripTextBox4.TextChanged += toolStripTextBox1_TextChanged;
            this.toolStripTextBox5.TextChanged += toolStripTextBox1_TextChanged;
            hinttimergc = new Timer
            {
                Interval = 2000
            };
            hinttimergc.Tick += hinttimergc_Tick;
            SquareGrid = new SquareGrid
            {
                GridColor = System.Drawing.Color.Green,
                Height = 25,
                Width = 25,
                OffsetX = 0,
                OffsetY = 0
            };
        }

        void toolStripTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (!ignore)
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
        private float GetValue(string data, string enddata)
        {
            if (data == null || enddata == null) return 0;
            float ret = 0;
            if (data.EndsWith(enddata))
            {
                var temp = data.Substring(0, data.Length - enddata.Length);
                if (!float.TryParse(temp, out ret)) ret = 0;
            }
            else
            {
                if (!float.TryParse(data, out ret)) ret = 0;
            }
            return ret;
        }

        void radioButtongroup2_CheckedChanged(object sender, EventArgs e)
        {
            bezierControl1.DrawAndRefresh();
        }
        public void SetLang()
        {
            this.label1.Text = Utility.Language["GCLabel1"];
            this.label2.Text = Utility.Language["GCLabel2"];
            this.checkBox1.Text = Utility.Language["GCCheckBox1"];
            this.checkBox2.Text = Utility.Language["GCCheckBox2"];
            this.checkBox3.Text = Utility.Language["GCCheckBox3"];
            this.checkBox4.Text = Utility.Language["GCCheckBox4"];
            this.groupBox1.Text = Utility.Language["GCGroup1"];
            this.groupBox2.Text = Utility.Language["GCGroup2"];
            this.groupBox3.Text = Utility.Language["GCGroup3"];
            this.radioButton1.Text = Utility.Language["GCRadioButton1"];
            this.radioButton2.Text = Utility.Language["GCRadioButton2"];
            this.radioButton3.Text = Utility.Language["GCRadioButton3"];
            this.radioButton4.Text = Utility.Language["GCRadioButton4"];
            this.radioButton5.Text = Utility.Language["GCRadioButton5"];
            this.radioButton6.Text = Utility.Language["GCRadioButton6"];
            this.radioButton7.Text = Utility.Language["GCRadioButton7"];
            this.radioButton8.Text = Utility.Language["GCRadioButton8"];
            this.radioButton9.Text = Utility.Language["GCRadioButton9"];
            this.radioButton10.Text = Utility.Language["GCRadioButton10"];
            this.radioButton11.Text = Utility.Language["GCRadioButton11"];
            this.button1.Text = Utility.Language["GCButton1"];
            this.button2.Text = Utility.Language["GCButton2"];
            this.button3.Text = Utility.Language["GCButton3"];
            error1 = Utility.Language["GCError1"];
            error2 = Utility.Language["GCError2"];
            gchintnormal = Utility.Language["GCHint1"];
            gchinttrans = Utility.Language["GCHint2"];
            label1.Location = new System.Drawing.Point(textBox1.Location.X - label1.Width - 10, textBox1.Location.Y);
            this.toolStripButton4.Text = Utility.Language["GCSetDefault"];
            this.toolStripButton4.ToolTipText = this.toolStripButton4.Text;
        }
        public void SetSkin()
        {
            this.label1.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.checkBox1.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.checkBox2.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.checkBox3.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.checkBox4.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.checkBox5.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.groupBox1.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.groupBox2.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.groupBox3.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.radioButton1.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.radioButton2.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.radioButton3.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.radioButton4.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.radioButton5.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.radioButton6.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.radioButton7.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.radioButton8.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.radioButton9.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.radioButton10.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
        }

        private void bezierControl1_BeforePaint(object sender, UserPaintEventArgs e)
        {
            var graphics = ((BezierControl.Context)e.Context).Graphics;
            if (SquareGrid != null && SquareGridEnabled)
            {
                using (Pen pen = new Pen(SquareGrid.GridColor))
                {
                    for (int i = (-SquareGrid.NormalizedOffsetX - StartX) / SquareGrid.Width; i <= bezierControl1.Width / SquareGrid.Width; i++)
                    {
                        graphics.DrawLine(pen, new System.Drawing.Point(i * SquareGrid.Width + SquareGrid.OffsetX + StartX, 0),
                            new System.Drawing.Point(i * SquareGrid.Width + SquareGrid.OffsetX + StartX, bezierControl1.Height));
                    }
                    for (int i = (-SquareGrid.NormalizedOffsetY - StartY) / SquareGrid.Height; i <= bezierControl1.Height / SquareGrid.Height; i++)
                    {
                        graphics.DrawLine(pen, new System.Drawing.Point(0, i * SquareGrid.Height + SquareGrid.OffsetY + StartY),
                            new System.Drawing.Point(bezierControl1.Width, i * SquareGrid.Height + SquareGrid.OffsetY + StartY));
                    }
                }
            }
        }

        private void bezierControl1_AfterPaint(object sender, BezierDrawControl.UserPaintEventArgs e)
        {
            var graphics = ((BezierControl.Context)e.Context).Graphics;
            CheckData();
            if (bezierControl1.Controller.BCPSCount < 2 || bezierControl1.Controller.IsTransformMode)
            {
                if (bezierControl1.Controller.IsTransformMode) CheckTransformValue();
            }
            else
            {
                if (checkBox2.Checked)
                {
                    foreach (var pos in poses)
                    {
                        if (pos == poses[0])
                        {
                            graphics.DrawEllipse(Pens.Green, pos.X - 5, pos.Y - 5, 10, 10);
                        }
                        else
                        {
                            graphics.DrawEllipse(Pens.Black, pos.X - 5, pos.Y - 5, 10, 10);
                        }
                    }
                }
                if (checkBox3.Checked && !radioButton7.Checked)
                {
                    for (int i = 0; i < dirs.Length; i++)
                    {
                        DrawAllow(new System.Drawing.PointF(poses[i].X, poses[i].Y),
                            new System.Drawing.PointF(dirs[i].X, dirs[i].Y), graphics);
                    }
                }
            }

            graphics.DrawRectangle(Pens.Red, new System.Drawing.Rectangle(StartX, StartY, 400, 225));
        }
        private void DrawAllow(PointF pos, PointF dir, Graphics g)
        {
            float length = 100;
            float length2 = 30;
            g.DrawLine(Pens.Blue, pos, new PointF(pos.X - dir.X * length, pos.Y - dir.Y * length));
            var r1 = Rotate(new Vector2(dir.X, dir.Y), (float)(Math.PI / 6));
            var r2 = Rotate(new Vector2(dir.X, dir.Y), (float)(-Math.PI / 6));
            g.DrawLine(Pens.Blue, pos, new PointF(pos.X - r1.X * length2, pos.Y - r1.Y * length2));
            g.DrawLine(Pens.Blue, pos, new PointF(pos.X - r2.X * length2, pos.Y - r2.Y * length2));
        }
        private Vector2 Rotate(Vector2 p, float r)
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
                poses = new Vector2[0];
                dirs = new Vector2[0];
                return;
            }
            //areaselection
            var mks = data.GetAreaData();
            poses = new Vector2[mks.Length];
            dirs = new Vector2[mks.Length];
            float[] ratios = new float[mks.Length];
            if (bezierControl1.Controller.BCPSCount >= 2 && poses.Length >= 2)
            {
                if (radioButton8.Checked)
                {
                    float all = mks[mks.Length - 1].Time - mks[0].Time;
                    if (all == 0)
                    {
                        poses = new Vector2[0];
                        dirs = new Vector2[0];
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
                    if (radioButton8.Checked)
                    {
                        ba.GetPoint(ratios[i], out pos, out dir);
                    }
                    else if (radioButton9.Checked)
                    {
                        ba.GetPoint(i * 99.9999f / (poses.Length - 1), out pos, out dir);
                    }
                    else if (radioButton10.Checked)
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
            if (checkBox4.Checked)
            {
                Apply();
            }
        }

        private Vector2 TransDirection(Vector2 dir)
        {
            if (radioButton3.Checked)
            {
                return dir;
            }
            else if (radioButton4.Checked)
            {
                return Rotate(dir, (float)Math.PI / 2);
            }
            else if (radioButton5.Checked)
            {
                return Rotate(dir, (float)Math.PI);
            }
            else if (radioButton6.Checked)
            {
                return Rotate(dir, (float)Math.PI / 2 * 3);
            }
            return dir;
        }
        private void GeometryCreator_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.Z:
                        bezierControl1.Controller.Undo();
                        break;
                    case Keys.Y:
                        bezierControl1.Controller.Redo();
                        break;
                    case Keys.T:
                        toolStrip1.Visible = true;
                        bezierControl1.Controller.StartTransform();
                        break;
                }
            }
            else
            {
                if (e.KeyCode == Keys.Delete)
                {
                    bezierControl1.Controller.Delete();
                }
                if (e.KeyCode == Keys.Return)
                {
                    bezierControl1.Controller.EndTransform();
                    toolStrip1.Visible = false;
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

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            bezierControl1.AntialiasEnabled = checkBox1.Checked;
            bezierControl1.DrawAndRefresh();
        }
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            bezierControl1.DrawAndRefresh();
        }
        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            bezierControl1.DrawAndRefresh();
        }
        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            button1.Enabled = !checkBox4.Checked;
        }

        private void label1_ValueChange(object sender, ValueChangeEventArgs e)
        {
            textBox1.Text = (int.Parse(textBox1.Text) + e.ChangedValue).ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                CreateCircle();
            }
            else if (radioButton2.Checked)
            {
                if (int.TryParse(textBox1.Text, out int num))
                {
                    if (num < 3)
                    {
                        MessageBox.Show(error2);
                    }
                    else
                    {
                        CreatePolygon(num);
                    }
                }
                else
                {
                    MessageBox.Show(error1);
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
            bezierControl1.Controller.BCPS = data.ToArray();
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
            bezierControl1.Controller.BCPS = data.ToArray();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!bezierControl1.Controller.IsTransformMode) bezierControl1.Controller.Clear();
        }

        private void button1_Click(object sender, EventArgs e)
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
            data.ApplyTrans(poses, dirs, !radioButton7.Checked);
            return true;
        }

        public bool Antialias
        {
            get
            {
                return checkBox1.Checked;
            }
            set
            {
                checkBox1.Checked = value;
            }
        }
        public bool DrawPos
        {

            get
            {
                return checkBox2.Checked;
            }
            set
            {
                checkBox2.Checked = value;
            }
        }
        public bool DrawAngle
        {

            get
            {
                return checkBox3.Checked;
            }
            set
            {
                checkBox3.Checked = value;
            }
        }
        public bool ApplyMoved
        {
            get
            {
                return checkBox4.Checked;
            }
            set
            {
                checkBox4.Checked = value;
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
                return checkBox5.Checked;
            }
            set
            {
                checkBox5.Checked = value;
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (FixedDistance != 0) bezierControl1.DrawAndRefresh();
        }

        private void radioButton11_CheckedChanged(object sender, EventArgs e)
        {
            textBox2.Enabled = radioButton11.Checked;
        }

        void hinttimergc_Tick(object sender, EventArgs e)
        {
            hinttimergc.Stop();
            toolTip1.Show(bezierControl1.Controller.IsTransformMode ? gchinttrans : gchintnormal, gettarget(), getpoint());
        }
        private void bezierControl1_MouseLeave(object sender, EventArgs e)
        {
            hinttimergc.Stop();
            toolTip1.Hide(gettarget());
        }

        private void bezierControl1_MouseMove(object sender, MouseEventArgs e)
        {
            hinttimergc.Stop();
            var p = getpoint();
            if (toolTip1.Active)
            {
                toolTip1.Hide(gettarget());
            }
            hinttimergc.Start();
        }

        private IWin32Window gettarget()
        {
            if (this.DockState == DockState.Float) return this;
            return WindowUtility.MainForm;
        }
        private System.Drawing.Point getpoint()
        {
            var p = new System.Drawing.Point(Cursor.Position.X + 10, Cursor.Position.Y + 10);
            if (this.DockState == DockState.Float) return this.PointToClient(p);
            return WindowUtility.MainForm.PointToClient(p);
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            if (bezierControl1.Controller.IsTransformMode)
            {
                bezierControl1.Controller.TransformRotation = 0;
                bezierControl1.Controller.TransformScale = new PointF(1, 1);
                bezierControl1.Controller.TransformTranslation = new PointF(0, 0);
                bezierControl1.DrawAndRefresh();
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
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

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            bezierControl1.DrawAndRefresh();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var gsf = new GridSettingForm();
            gsf.SetLang();
            gsf.Grid = new PPDEditor.SquareGrid
            {
                Width = SquareGrid.Width * 2,
                Height = SquareGrid.Height * 2,
                GridColor = SquareGrid.GridColor,
                OffsetX = SquareGrid.OffsetX * 2,
                OffsetY = SquareGrid.OffsetY * 2
            };
            if (gsf.ShowDialog() == DialogResult.OK)
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

        private PointF bezierControl1_PreprocessPoint(PointF arg)
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
