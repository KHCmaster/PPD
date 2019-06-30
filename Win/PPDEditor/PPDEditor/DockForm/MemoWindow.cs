using PPDEditor.Forms;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace PPDEditor
{
    public partial class MemoWindow : ChangableDockContent
    {
        public MemoWindow()
        {
            InitializeComponent();
            textBox1.KeyDown += textBox1_KeyDown;
            textBox1.MouseWheel += textBox1_MouseWheel;
        }

        void textBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                MemoFontSize++;
            }
            else if (e.Delta < 0)
            {
                MemoFontSize--;
            }

        }

        void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.Oemplus:
                        MemoFontSize++;
                        break;
                    case Keys.OemMinus:
                        MemoFontSize--;
                        break;
                }
            }
        }

        public void SetLang()
        {
            this.TabText = this.Text = Utility.Language["Memo"];
        }

        public void Clear()
        {
            MemoText = "";
        }

        public string MemoText
        {
            get
            {
                return textBox1.Text;
            }
            set
            {
                textBox1.Text = value;
            }
        }

        public float MemoFontSize
        {
            get
            {
                return textBox1.Font.Size;
            }
            set
            {
                if (value >= 5 && value <= 100)
                {
                    textBox1.Font = new Font(textBox1.Font.FontFamily, value);
                    PPDStaticSetting.MemoFontSize = value;
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            ContentChanged();
        }
    }
}
