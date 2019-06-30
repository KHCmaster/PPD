using PPDEditor.Forms;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PPDEditor
{
    public partial class BPMMeasure : ScrollableForm
    {
        [DllImport("winmm.dll")]
        static extern long timeGetTime();
        ArrayList data;
        public BPMMeasure()
        {
            InitializeComponent();
            data = new ArrayList(10);
        }
        public void SetLang()
        {
            this.label1.Text = Utility.Language["BMLabel1"];
            this.label2.Text = Utility.Language["BMLabel2"];
            this.button1.Text = Utility.Language["BMButton1"];
            this.button2.Text = Utility.Language["BMButton2"];
        }
        public void SetSkin()
        {
            this.label1.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.label2.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.label3.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.label4.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
        }
        private void updateText()
        {
            if (data.Count <= 1) return;
            long tempdata = (long)data[data.Count - 1] - (long)data[0];
            this.label3.Text = ((long)data[data.Count - 1] - (long)data[data.Count - 2]).ToString();
            this.label4.Text = (60f / (tempdata / 1000f) * data.Count).ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            data.Clear();
            this.label3.Text = "0";
            this.label4.Text = "0";
        }

        private void button2_Enter(object sender, EventArgs e)
        {
            button1.Focus();
        }

        private void button1_MouseDown(object sender, MouseEventArgs e)
        {
            data.Add(timeGetTime());
            updateText();

        }

        bool down;
        private void BPMMeasure_KeyDown(object sender, KeyEventArgs e)
        {
            if (!down)
            {
                down = true;
                if (e.KeyData == Keys.Return || e.KeyCode == Keys.Space)
                {
                    e.Handled = true;
                    data.Add(timeGetTime());
                    updateText();
                }
            }
        }

        private void BPMMeasure_KeyUp(object sender, KeyEventArgs e)
        {
            down = false;
        }
    }
}
