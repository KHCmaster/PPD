using Effect2D;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace PPDEditor.Controls.Resource
{
    public partial class EffectPanel : UserControl
    {
        const int checksize = 10;

        private Bitmap CheckBack;
        private Color backColor = Color.White;
        private EffectManager effectManager;
        private Dictionary<string, Image> dict;
        private Timer timer;
        private Matrix moveMatrix;

        private Dictionary<int, int> sizeDict = new Dictionary<int, int>
        {
            {0,256},
            {1,384},
            {2,512},
            {3,768},
            {4,1024}
        };

        bool ignore;

        public EffectPanel()
        {
            InitializeComponent();
            canvasPanel.InitializeBuffer();
            CanvasSizeChanged();
            Disposed += EffectPanel_Disposed;
            timer = new Timer
            {
                Interval = 1000 / 60
            };
            timer.Tick += timer_Tick;
            canvasPanel.CustomPaint += canvasPanel_CustomPaint;
            canvasPanel.SizeChanged += canvasPanel_SizeChanged;
            SizeChanged += EffectPanel_SizeChanged;
            toolStripSplitButton1.Click += toolStripSplitButton1_Click;
            Dock = DockStyle.Fill;

            toolStrip1.Renderer = new CustomToolStripRenderer();
            toolStripComboBox2.ComboBox.DrawItem += ComboBox_DrawItem;
            toolStripComboBox2.ComboBox.DrawMode = DrawMode.OwnerDrawFixed;
            ignore = true;
            toolStripComboBox1.SelectedIndex = PPDStaticSetting.CanvasSizeIndex;
            toolStripComboBox2.SelectedIndex = PPDStaticSetting.CanvasColorIndex;
            ignore = false;
        }

        public void SetLang()
        {
            playToolStripMenuItem.Text = Utility.Language["PlayOnce"];
            reversePlayToolStripMenuItem.Text = Utility.Language["PlayReverce"];
            loopPlayToolStripMenuItem.Text = Utility.Language["PlayLoop"];
            reverseLoopPlayToolStripMenuItem.Text = Utility.Language["PlayLoopReverse"];
            toolStripButton1.Text = Utility.Language["Stop"];
            toolStripComboBox1.Text = Utility.Language["CanvasSize"];
            toolStripComboBox2.Text = Utility.Language["CanvasBackColor"];
            int prevSelect = toolStripComboBox2.SelectedIndex;
            ignore = true;
            toolStripComboBox2.Items.Clear();
            toolStripComboBox2.Items.AddRange(new string[]{
                Utility.Language["Default"],                Utility.Language["Black"],                Utility.Language["White"],                Utility.Language["Custom"]            });
            toolStripComboBox2.SelectedIndex = prevSelect;
            ignore = false;
        }

        void EffectPanel_Disposed(object sender, EventArgs e)
        {
            timer.Stop();
        }

        void ComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            switch (e.Index)
            {
                case 0:
                    e.Graphics.FillRectangle(Brushes.LightGray, new Rectangle(e.Bounds.X + 2, e.Bounds.Y + 2, 8, 8));
                    e.Graphics.FillRectangle(Brushes.LightGray, new Rectangle(e.Bounds.X + 10, e.Bounds.Y + 10, 8, 8));
                    break;
                case 1:
                    e.Graphics.FillRectangle(Brushes.Black, new Rectangle(e.Bounds.X + 2, e.Bounds.Y + 2, 16, 16));
                    break;
                case 2:
                    e.Graphics.FillRectangle(Brushes.White, new Rectangle(e.Bounds.X + 2, e.Bounds.Y + 2, 16, 16));
                    break;
                case 3:
                    using (SolidBrush brush = new SolidBrush(PPDStaticSetting.CustomCanvasBackColor))
                    {
                        e.Graphics.FillRectangle(brush, new Rectangle(e.Bounds.X + 2, e.Bounds.Y + 2, 16, 16));
                    }
                    break;
            }
            e.Graphics.DrawRectangle(Pens.Black, new Rectangle(e.Bounds.X + 2, e.Bounds.Y + 2, 16, 16));
            using (SolidBrush brush = new SolidBrush(e.ForeColor))
            {
                e.Graphics.DrawString(toolStripComboBox2.Items[e.Index].ToString(), e.Font, brush,
                    new Point(e.Bounds.X + 20, e.Bounds.Y));
            }
        }

        void toolStripSplitButton1_Click(object sender, EventArgs e)
        {
            toolStripSplitButton1.Checked = !toolStripSplitButton1.Checked;
            if (toolStripSplitButton1.Checked)
            {
                Play();
            }
            else
            {
                Pause();
                ChangeImage();
            }
        }

        private void ChangeImage()
        {
            var tsmi = GetCheckedMenu();
            if (tsmi != null)
            {
                toolStripSplitButton1.Image = tsmi.Image;
            }
        }

        private ToolStripMenuItem GetCheckedMenu()
        {
            if (playToolStripMenuItem.Checked) return playToolStripMenuItem;
            else if (reversePlayToolStripMenuItem.Checked) return reversePlayToolStripMenuItem;
            else if (loopPlayToolStripMenuItem.Checked) return loopPlayToolStripMenuItem;
            else if (reverseLoopPlayToolStripMenuItem.Checked) return reverseLoopPlayToolStripMenuItem;
            return null;
        }

        void EffectPanel_SizeChanged(object sender, EventArgs e)
        {
            toolStrip1.Location = new Point((this.Width - toolStrip1.Width) / 2, this.Height - 30 - toolStrip1.Height);
            ChangeCanvasLocation();
        }

        private void ChangeCanvasLocation()
        {
            int posX = 0, posY = 0;
            if (canvasPanel.Width < this.Width)
            {
                posX = (this.Width - canvasPanel.Width) / 2;
            }
            if (canvasPanel.Height < this.Height)
            {
                posY = (this.Height - canvasPanel.Height) / 2;
            }
            canvasPanel.Location = new Point(posX, posY);
        }

        void canvasPanel_SizeChanged(object sender, EventArgs e)
        {
            CanvasSizeChanged();
            ChangeCanvasLocation();
            canvasPanel.DrawAndRefresh();
        }

        private void CanvasSizeChanged()
        {
            CreateCheckBack(canvasPanel.Width);
            moveMatrix = new Matrix();
            moveMatrix.Translate(canvasPanel.Width / 2, canvasPanel.Height / 2);
        }

        private void CreateCheckBack(int width)
        {
            if (CheckBack != null)
            {
                CheckBack.Dispose();
                CheckBack = null;
            }
            CheckBack = new Bitmap(width, checksize * 2);
            using (Graphics g = Graphics.FromImage(CheckBack))
            {
                for (int i = 0; i <= width / (checksize * 2); i++)
                {
                    g.FillRectangle(Brushes.White, new Rectangle(2 * i * checksize, 0, checksize, checksize));
                    g.FillRectangle(Brushes.LightGray, new Rectangle((2 * i + 1) * checksize, 0, checksize, checksize));
                    g.FillRectangle(Brushes.White, new Rectangle((2 * i + 1) * checksize, checksize, checksize, checksize));
                    g.FillRectangle(Brushes.LightGray, new Rectangle(2 * i * checksize, checksize, checksize, checksize));
                }
            }
        }

        void canvasPanel_CustomPaint(object sender, PaintEventArgs e)
        {
            DrawBack(e.Graphics);
            e.Graphics.Transform = moveMatrix;
            if (effectManager != null)
            {
                effectManager.Draw((fn, state) =>
                    {
                        Draw(e.Graphics, fn, state);
                    });
            }
            DrawCenter(e.Graphics);
        }
        private void DrawBack(Graphics g)
        {
            float x = g.Transform.OffsetX;
            float y = g.Transform.OffsetY;
            g.TranslateTransform(-x, -y);
            switch (toolStripComboBox2.SelectedIndex)
            {
                case 0:
                    for (int i = 0; i <= canvasPanel.Height / (checksize * 2); i++)
                    {
                        g.DrawImage(CheckBack, new Point(0, checksize * 2 * i));
                    }
                    break;
                case 1:
                    g.Clear(Color.Black);
                    break;
                case 2:
                    g.Clear(Color.White);
                    break;
                case 3:
                    g.Clear(PPDStaticSetting.CustomCanvasBackColor);
                    break;
            }
            g.TranslateTransform(x, y);
        }
        private void DrawCenter(Graphics g)
        {
            g.DrawLine(Pens.Black, new Point(0, -canvasPanel.Height / 2), new Point(0, canvasPanel.Height / 2));
            g.DrawLine(Pens.Black, new Point(-canvasPanel.Width / 2, 0), new Point(canvasPanel.Width / 2, 0));
        }
        private void Draw(Graphics g, string filename, EffectStateStructure state)
        {
            if (state == null || state.Alpha <= 0 || state.ScaleX == 0 || state.ScaleY == 0) return;
            var temp = g.Transform.Clone();
            Image image = dict[filename];
            if (state.ScaleX * image.Width >= 1 && state.ScaleY * image.Height >= 1)
            {
                g.TranslateTransform(state.X, state.Y);
                g.RotateTransform(state.Rotation);
                g.ScaleTransform(state.ScaleX, state.ScaleY);
                g.TranslateTransform(-image.Width / 2, -image.Height / 2);
                var cm = new System.Drawing.Imaging.ColorMatrix
                {
                    Matrix00 = 1,
                    Matrix11 = 1,
                    Matrix22 = 1,
                    Matrix33 = state.Alpha,
                    Matrix44 = 1
                };
                var ia = new System.Drawing.Imaging.ImageAttributes();
                ia.SetColorMatrix(cm);
                g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, ia);
            }
            g.Transform = temp;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (effectManager.State == EffectManager.PlayState.Stop)
            {
                timer.Stop();
            }
            effectManager.Update();
            canvasPanel.DrawAndRefresh();
        }

        public void OpenFile(string path)
        {
            dict = new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);
            var dir = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
            effectManager = EffectLoader.Load(path, false, (fn) =>
            {
                var filename = Path.Combine(dir, fn);
                using (Image image = Image.FromFile(filename))
                {
                    if (!dict.ContainsKey(filename))
                    {
                        dict.Add(filename, new Bitmap(image));
                    }
                }
            });
            effectManager.Finish += effectManager_Finish;
            canvasPanel.DrawAndRefresh();
        }

        void effectManager_Finish(object sender, EventArgs e)
        {
            this.toolStripSplitButton1.Checked = false;
            ChangeImage();
        }

        private void Stop()
        {
            timer.Stop();
            effectManager.Update(0, null);
            effectManager.Stop();
            canvasPanel.DrawAndRefresh();
        }

        private void Pause()
        {
            timer.Stop();
            effectManager.Pause();
        }

        private void Play()
        {
            string tooltiptext = "";
            EffectManager.PlayType playType = Effect2D.EffectManager.PlayType.Once;
            if (playToolStripMenuItem.Checked)
            {
                playType = EffectManager.PlayType.Once;
                tooltiptext = playToolStripMenuItem.Text;
            }
            else if (reversePlayToolStripMenuItem.Checked)
            {
                playType = EffectManager.PlayType.ReverseOnce;
                tooltiptext = reversePlayToolStripMenuItem.Text;
            }
            else if (loopPlayToolStripMenuItem.Checked)
            {
                playType = EffectManager.PlayType.Loop;
                tooltiptext = loopPlayToolStripMenuItem.Text;
            }
            else if (reverseLoopPlayToolStripMenuItem.Checked)
            {
                playType = EffectManager.PlayType.ReverseLoop;
                tooltiptext = reversePlayToolStripMenuItem.Text;
            }
            toolStripSplitButton1.Image = Properties.Resources.pause;


            timer.Stop();
            effectManager.CheckFrameLength();
            effectManager.Play(playType);
            timer.Start();
        }

        private void playToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TurnOffExcept(playToolStripMenuItem);
            Play();
        }

        private void reversePlayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TurnOffExcept(reversePlayToolStripMenuItem);
            Play();
        }

        private void loopPlayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TurnOffExcept(loopPlayToolStripMenuItem);
            Play();
        }

        private void reverseLoopPlayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TurnOffExcept(reverseLoopPlayToolStripMenuItem);
            Play();
        }

        private void TurnOffExcept(ToolStripMenuItem tsmi)
        {
            foreach (ToolStripMenuItem ttsmi in this.toolStripSplitButton1.DropDownItems)
            {
                if (tsmi != ttsmi) ttsmi.Checked = false;
                else ttsmi.Checked = true;
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Stop();
            toolStripSplitButton1.Checked = false;
            ChangeImage();
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int length = sizeDict[toolStripComboBox1.SelectedIndex];
            canvasPanel.Size = new Size(length, length);
            PPDStaticSetting.CanvasSizeIndex = toolStripComboBox1.SelectedIndex;
        }

        private void toolStripComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (toolStripComboBox2.SelectedIndex == 3 && !ignore)
            {
                colorDialog1.Color = PPDStaticSetting.CustomCanvasBackColor;
                if (colorDialog1.ShowDialog() == DialogResult.OK)
                {
                    PPDStaticSetting.CustomCanvasBackColor = colorDialog1.Color;
                }
            }
            canvasPanel.DrawAndRefresh();
            PPDStaticSetting.CanvasColorIndex = toolStripComboBox2.SelectedIndex;
        }
    }
}
