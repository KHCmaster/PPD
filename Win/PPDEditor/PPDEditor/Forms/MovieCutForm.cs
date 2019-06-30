using PPDFramework;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace PPDEditor.Forms
{
    public partial class MovieCutForm : Form
    {
        float left;
        float right;
        float top;
        float bottom;
        string numbererror = "数字以外が書かれていましたので０にしました";

        public bool ApplyToIni
        {
            get
            {
                return applyIniCheckBox.Checked;
            }
        }

        public MovieTrimmingData TrimmingData
        {
            get
            {
                return new MovieTrimmingData(top, left, right, bottom);
            }
        }

        public MovieCutForm(MovieTrimmingData trimmingdata)
        {
            InitializeComponent();
            if (trimmingdata != null)
            {
                this.textBox1.Text = trimmingdata.Left.ToString();
                this.textBox2.Text = trimmingdata.Right.ToString();
                this.textBox3.Text = trimmingdata.Top.ToString();
                this.textBox4.Text = trimmingdata.Bottom.ToString();
            }
            else
            {
                this.textBox1.Text = this.textBox2.Text = this.textBox3.Text = this.textBox4.Text = "0";
            }

            applyIniCheckBox.Checked = PPDStaticSetting.ApplyToIniMovieTrimming;
        }
        public void SetLang()
        {
            this.Text = Utility.Language["Cut"];
            this.label1.Text = Utility.Language["CutLabel1"];
            this.label2.Text = Utility.Language["CutLabel2"];
            this.label3.Text = Utility.Language["CutLabel3"];
            this.label4.Text = Utility.Language["CutLabel4"];
            this.label5.Text = Utility.Language["CutLabel5"];
            this.label1.Location = new Point(this.textBox1.Location.X - 5 - this.label1.Width, this.label1.Location.Y);
            this.label2.Location = new Point(this.textBox2.Location.X - 5 - this.label2.Width, this.label2.Location.Y);
            this.label3.Location = new Point(this.textBox3.Location.X - 5 - this.label3.Width, this.label3.Location.Y);
            this.label4.Location = new Point(this.textBox4.Location.X - 5 - this.label4.Width, this.label4.Location.Y);
            numbererror = Utility.Language["CutNumberError"];
        }
        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            if (float.TryParse(this.textBox1.Text, out float num))
            {
                left = num;
            }
            else
            {
                MessageBox.Show(numbererror + ":" + this.label1.Text);
            }
            if (float.TryParse(this.textBox2.Text, out num))
            {
                right = num;
            }
            else
            {
                MessageBox.Show(numbererror + ":" + this.label1.Text);
            }
            if (float.TryParse(this.textBox3.Text, out num))
            {
                top = num;
            }
            else
            {
                MessageBox.Show(numbererror + ":" + this.label1.Text);
            }
            if (float.TryParse(this.textBox4.Text, out num))
            {
                bottom = num;
            }
            else
            {
                MessageBox.Show(numbererror + ":" + this.label1.Text);
            }
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void textBox1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                e.IsInputKey = true;
                this.textBox2.Focus();
                this.textBox2.SelectAll();
            }
        }

        private void textBox2_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                e.IsInputKey = true;
                this.textBox3.Focus();
                this.textBox3.SelectAll();
            }
        }

        private void textBox3_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                e.IsInputKey = true;
                this.textBox4.Focus();
                this.textBox4.SelectAll();
            }
        }

        private void textBox4_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                this.button1_Click(sender, e);
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter || e.KeyChar == (char)Keys.Escape)
            {
                e.Handled = true;
            }
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter || e.KeyChar == (char)Keys.Escape)
            {
                e.Handled = true;
            }
        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter || e.KeyChar == (char)Keys.Escape)
            {
                e.Handled = true;
            }
        }

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter || e.KeyChar == (char)Keys.Escape)
            {
                e.Handled = true;
            }
        }
    }
}
