using PPDEditor.Command.PPDSheet;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace PPDEditor.Controls
{
    public partial class LayerDisplay : UserControl
    {
        public event EventHandler VisibleStateChanged;
        public event EventHandler SelectStateChanged;
        public event EventHandler Deleted;
        public event EventHandler Duplicated;
        bool selected;
        bool visible = true;
        int rightmargin = 5;
        PPDSheet data;
        Color markColor;
        string deleteconfirm = "削除してもよろしいですか?";
        string confirm = "確認";
        public LayerDisplay()
        {
            data = new PPDSheet();
            InitializeComponent();
            this.textBox1.LostFocus += textBox1_LostFocus;
            MarkColor = PPDEditorSkin.Skin.TimeLineMarkColor;
        }
        public void SetLang()
        {
            this.このレイヤーを複製ToolStripMenuItem.Text = Utility.Language["LMMenu2"];
            this.このレイヤーを削除ToolStripMenuItem.Text = Utility.Language["LMMenu3"];
            deleteconfirm = Utility.Language["LMDeleteConfirm"];
            confirm = Utility.Language["LMConfirm"];
        }
        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            this.pictureBox1.Visible = !this.pictureBox1.Visible;
            visible = this.pictureBox1.Visible;
            VisibleChange();
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            this.pictureBox1.Visible = !this.pictureBox1.Visible;
            visible = this.pictureBox1.Visible;
            VisibleChange();
        }
        private void VisibleChange()
        {
            if (VisibleStateChanged != null)
            {
                VisibleStateChanged.Invoke(this, EventArgs.Empty);
            }
        }
        private void label2_MouseClick(object sender, MouseEventArgs e)
        {
            selected = true;
            this.Focus();
            if (SelectStateChanged != null)
            {
                SelectStateChanged.Invoke(this, EventArgs.Empty);
            }
            ChangeDisplayMode();
        }

        private void label3_MouseClick(object sender, MouseEventArgs e)
        {
            selected = true;
            this.Focus();
            if (SelectStateChanged != null)
            {
                SelectStateChanged.Invoke(this, EventArgs.Empty);
            }
            ChangeDisplayMode();
        }

        private void label4_MouseClick(object sender, MouseEventArgs e)
        {
            selected = true;
            this.Focus();
            if (SelectStateChanged != null)
            {
                SelectStateChanged.Invoke(this, EventArgs.Empty);
            }
            ChangeDisplayMode();
        }
        private void layerdisplay_MouseDown(object sender, MouseEventArgs e)
        {
            selected = true;
            this.Focus();
            if (SelectStateChanged != null)
            {
                SelectStateChanged.Invoke(this, EventArgs.Empty);
            }
            ChangeDisplayMode();

        }
        private void layerdisplay_MouseClick(object sender, MouseEventArgs e)
        {
            /*selected = true;
            this.Focus();
            if (SelectStateChanged != null)
            {
                SelectStateChanged.Invoke(this, EventArgs.Empty);
            }
            changedisplaymode();*/
        }
        private void ChangeDisplayMode()
        {
            if (selected)
            {
                this.BackColor = Color.LightBlue;
            }
            else
            {
                this.BackColor = Color.LightGray;
            }
        }
        private void label1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.textBox1.Text = DisplayName;
            this.textBox1.Visible = true;
            this.textBox1.Focus();
        }
        private void textBox1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                e.IsInputKey = true;
                hidetextbox();
            }
        }
        void textBox1_LostFocus(object sender, EventArgs e)
        {
            hidetextbox();
        }
        private void hidetextbox()
        {
            if (this.textBox1.Visible)
            {
                DisplayName = this.textBox1.Text == "" ? "no text" : this.textBox1.Text;
                data.DisplayName = this.DisplayName;
                this.textBox1.Visible = false;
            }
        }

        private void contextMenuStrip1_Opened(object sender, EventArgs e)
        {
            selected = true;
            this.Focus();
            if (SelectStateChanged != null)
            {
                SelectStateChanged.Invoke(this, EventArgs.Empty);
            }
            ChangeDisplayMode();
        }

        private void このレイヤーを削除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.Deleted != null)
            {
                if (MessageBox.Show(deleteconfirm, confirm, MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    this.Deleted.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void このレイヤーを複製ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.Duplicated != null)
            {
                this.Duplicated.Invoke(this, EventArgs.Empty);
            }
        }
        public PPDSheet PPDData
        {
            get
            {
                return data;
            }
        }
        public bool Selected
        {
            get
            {
                return selected;
            }
            set
            {
                selected = value;
                ChangeDisplayMode();
            }
        }
        public bool SelectedWithEvent
        {
            get
            {
                return selected;
            }
            set
            {
                selected = value;
                if (selected)
                {
                    this.Focus();
                    if (SelectStateChanged != null)
                    {
                        SelectStateChanged.Invoke(this, EventArgs.Empty);
                    }
                }
                ChangeDisplayMode();
            }
        }
        public bool DisplayVisible
        {
            get
            {
                return visible;
            }
            set
            {
                this.pictureBox1.Visible = value;
                visible = value;
                VisibleChange();
            }
        }
        public string DisplayName
        {
            get
            {
                return this.label1.Text;
            }
            set
            {
                this.label1.Text = value;
            }
        }
        public string BPM
        {
            get
            {
                return this.label2.Text;
            }
            set
            {
                this.label2.Text = value;
                this.label2.Location = new Point(this.ClientSize.Width - this.label2.Width - rightmargin, this.label2.Location.Y);
            }
        }

        public Color MarkColor
        {
            get
            {
                return markColor;
            }
            set
            {
                panel2.BackColor = markColor = value;
            }
        }

        private void panel2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            using (ColorDialog cd = new ColorDialog())
            {
                cd.Color = panel2.BackColor;
                if (cd.ShowDialog() == DialogResult.OK)
                {
                    MarkColor = cd.Color;
                    WindowUtility.TimeLineForm.Seekmain.DrawAndRefresh();
                }
            }
        }
    }
}
