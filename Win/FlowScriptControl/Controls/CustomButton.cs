using System;
using System.Drawing;
using System.Windows.Forms;

namespace FlowScriptControl.Controls
{
    public partial class CustomButton : UserControl
    {
        public event Action ButtonClick;

        public CustomButton()
        {
            InitializeComponent();

            textBox1.TextChanged += textBox1_TextChanged;
            VisibleChanged += CustomButton_VisibleChanged;
            button1.Click += button1_Click;
        }

        void button1_Click(object sender, EventArgs e)
        {
            ButtonClick?.Invoke();
            textBox1.Focus();
        }

        void CustomButton_VisibleChanged(object sender, EventArgs e)
        {
            textBox1.Focus();
        }

        void textBox1_TextChanged(object sender, EventArgs e)
        {
            this.Text = textBox1.Text;
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            this.textBox1.Text = this.Text;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            this.textBox1.Location = new Point(3, (this.Height - textBox1.Height) / 2);
            this.textBox1.Size = new Size(this.Width - button1.Width - 3, this.Height);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            textBox1.Font = Font;
        }
    }
}
