using Effect2D;
using PPDConfiguration;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Effect2DEditor.DockForm
{
    public partial class TimeLineDock : DockContent
    {
        public event Action<float> FpsChanged;
        public event Action<string> EffectLoaded;
        public event Action EffectDeleted;
        public event Action EffectReversed;
        public event Action EffectCopyed;
        public event Action EffectCleared;

        public MainForm MainForm
        {
            get;
            set;
        }

        public float CurrentFrame
        {
            get { return timeLine1.CurrentFrame; }
        }

        public TimeLineDock()
        {
            InitializeComponent();
        }

        public void DrawAndRefresh()
        {
            timeLine1.DrawAndRefresh();
            timeLineItem1.DrawAndRefresh();
        }

        public void AdjustHScroll()
        {
            timeLine1.AdjustHScrollBar();
        }

        public void MoveVScroll(int val)
        {
            timeLine1.MoveVScroll(val);
        }

        public void Initialize()
        {
            if (!(toolStrip1.Renderer is CustomToolStripRenderer))
            {
                this.toolStrip1.Renderer = new CustomToolStripRenderer();
            }

            toolStripTextBox1.Text = MainForm.CanvasDock.EffectManager.FPS.ToString();
            timeLine1.EffectManager = MainForm.CanvasDock.EffectManager;
            timeLine1.MainForm = MainForm;
            timeLineItem1.EffectManager = MainForm.CanvasDock.EffectManager;
            timeLineItem1.MainForm = MainForm;
            timeLine1.SetUtility(MainForm.CanvasDock.SelectedManager);
            timeLineItem1.SetUtility(MainForm.CanvasDock.SelectedManager);
            timeLineItem1.TimeLine = timeLine1;
        }

        public void SetLang(SettingReader lang)
        {
            toolStripButton1.ToolTipText = lang["Add"];
            toolStripButton2.ToolTipText = lang["Delete"];
            toolStripButton3.ToolTipText = lang["SwapPermutation"];
            toolStripButton4.ToolTipText = lang["Copy"];
            toolStripButton5.ToolTipText = lang["Stop"];
            toolStripButton6.ToolTipText = lang["Clear"];
            exToolStripSplitButton1.ToolTipText = lang["PlayOnce"];
            回再生ToolStripMenuItem.Text = lang["PlayOnce"];
            リバース1回再生ToolStripMenuItem.Text = lang["PlayOnceReverse"];
            ループ再生ToolStripMenuItem.Text = lang["PlayLoop"];
            リバースループ再生ToolStripMenuItem.Text = lang["PlayReverseLoop"];
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "JPEG, GIF, PNG, ETD|*.jpg;*.jpeg;*.gif;*.png;*.etd|JPEG|*.jpg;*.jpeg|GIF|*.gif|PNG|*.png|ETD|*.etd";
            openFileDialog1.FileName = "";
            openFileDialog1.Multiselect = true;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                foreach (string fn in openFileDialog1.FileNames)
                {
                    OnEffectLoaded(fn);
                }
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (EffectDeleted != null)
            {
                EffectDeleted.Invoke();
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (EffectReversed != null)
            {
                EffectReversed.Invoke();
            }
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            if (EffectCopyed != null)
            {
                EffectCopyed.Invoke();
            }
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            MainForm.CanvasDock.Stop();
            exToolStripSplitButton1.Checked = false;
            ChangeImage();
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            EffectCleared?.Invoke();
        }

        private void 回再生ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            exToolStripSplitButton1.Checked = true;
            TurnOffExcept(sender as ToolStripMenuItem);
            Play();
        }

        private void ループ再生ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            exToolStripSplitButton1.Checked = true;
            TurnOffExcept(sender as ToolStripMenuItem);
            Play();
        }

        private void リバースループ再生ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            exToolStripSplitButton1.Checked = true;
            TurnOffExcept(sender as ToolStripMenuItem);
            Play();
        }

        private void リバース1回再生ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            exToolStripSplitButton1.Checked = true;
            TurnOffExcept(sender as ToolStripMenuItem);
            Play();
        }

        public void EffectManagerPlayFinished()
        {
            this.exToolStripSplitButton1.Checked = false;
            ChangeImage();
        }

        private void TurnOffExcept(ToolStripMenuItem tsmi)
        {
            foreach (ToolStripMenuItem ttsmi in this.exToolStripSplitButton1.DropDownItems)
            {
                if (tsmi != ttsmi) ttsmi.Checked = false;
                else ttsmi.Checked = true;
            }
        }

        private void exToolStripSplitButton1_ButtonClick(object sender, EventArgs e)
        {
            exToolStripSplitButton1.Checked = !exToolStripSplitButton1.Checked;
            if (exToolStripSplitButton1.Checked)
            {
                Play();
            }
            else
            {
                MainForm.CanvasDock.Pause();
                ChangeImage();
            }
        }

        private void ChangeImage()
        {
            Image image = null;
            if (回再生ToolStripMenuItem.Checked) image = Effect2DEditor.Properties.Resources.playonce;
            else if (リバース1回再生ToolStripMenuItem.Checked) image = Effect2DEditor.Properties.Resources.playreverse;
            else if (ループ再生ToolStripMenuItem.Checked) image = Effect2DEditor.Properties.Resources.playloop;
            else if (リバースループ再生ToolStripMenuItem.Checked) image = Effect2DEditor.Properties.Resources.playreverseloop;
            exToolStripSplitButton1.Image = image;
        }

        private void Play()
        {
            string tooltiptext = "";
            EffectManager.PlayType playType = Effect2D.EffectManager.PlayType.Once;
            if (回再生ToolStripMenuItem.Checked)
            {
                playType = EffectManager.PlayType.Once;
                tooltiptext = 回再生ToolStripMenuItem.Text;
            }
            else if (リバース1回再生ToolStripMenuItem.Checked)
            {
                playType = EffectManager.PlayType.ReverseOnce;
                tooltiptext = リバース1回再生ToolStripMenuItem.Text;
            }
            else if (ループ再生ToolStripMenuItem.Checked)
            {
                playType = EffectManager.PlayType.Loop;
                tooltiptext = ループ再生ToolStripMenuItem.Text;
            }
            else if (リバースループ再生ToolStripMenuItem.Checked)
            {
                playType = EffectManager.PlayType.ReverseLoop;
                tooltiptext = リバース1回再生ToolStripMenuItem.Text;
            }
            exToolStripSplitButton1.Image = Effect2DEditor.Properties.Resources.pause;
            exToolStripSplitButton1.ToolTipText = tooltiptext;
            MainForm.CanvasDock.PlayType = playType;
            MainForm.CanvasDock.Play();
        }

        private void toolStripTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (float.TryParse(toolStripTextBox1.Text, out float val))
            {
                if (FpsChanged != null)
                {
                    FpsChanged.Invoke(val);
                }
            }
        }

        private void TimeLineDock_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
            else e.Effect = DragDropEffects.None;
        }

        private void TimeLineDock_DragDrop(object sender, DragEventArgs e)
        {
            var fileName = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            foreach (string fn in fileName)
            {
                var extension = Path.GetExtension(fn);
                if (CheckValidExtension(extension))
                {
                    OnEffectLoaded(fn);
                }
            }
        }

        private bool CheckValidExtension(string extension)
        {
            extension = extension.ToLower();
            return extension == ".jpg" || extension == ".jpeg" || extension == ".gif" || extension == ".png" || extension == ".etd";
        }

        private void OnEffectLoaded(string fileName)
        {
            if (EffectLoaded != null)
            {
                EffectLoaded.Invoke(fileName);
            }
        }
    }
}
