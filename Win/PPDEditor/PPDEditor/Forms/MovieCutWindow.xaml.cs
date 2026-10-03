using PPDFramework;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PPDEditor.Forms
{
    public partial class MovieCutWindow : Window
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
                return applyIniCheckBox.IsChecked == true;
            }
        }

        public MovieTrimmingData TrimmingData
        {
            get
            {
                return new MovieTrimmingData(top, left, right, bottom);
            }
        }

        public MovieCutWindow(MovieTrimmingData trimmingdata)
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

            applyIniCheckBox.IsChecked = PPDStaticSetting.ApplyToIniMovieTrimming;
        }
        public void SetLang()
        {
            this.Title = Utility.Language["Cut"];
            this.label1.Text = Utility.Language["CutLabel1"];
            this.label2.Text = Utility.Language["CutLabel2"];
            this.label3.Text = Utility.Language["CutLabel3"];
            this.label4.Text = Utility.Language["CutLabel4"];
            this.label5.Text = Utility.Language["CutLabel5"];
            numbererror = Utility.Language["CutNumberError"];
        }
        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            left = Parse(textBox1);
            right = Parse(textBox2);
            top = Parse(textBox3);
            bottom = Parse(textBox4);
            this.DialogResult = true;
        }

        private float Parse(TextBox textBox)
        {
            if (float.TryParse(textBox.Text, out float num))
            {
                return num;
            }
            // 以前と同じく、どの欄でも「左」の名前で知らせる
            MessageBox.Show(this, numbererror + ":" + this.label1.Text);
            return 0;
        }

        /// <summary>
        /// Enter で次の欄に移ります。最後の欄では OK と同じです（以前と同じ）。
        /// </summary>
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return)
            {
                return;
            }
            e.Handled = true;
            TextBox next = null;
            if (sender == textBox1) next = textBox2;
            else if (sender == textBox2) next = textBox3;
            else if (sender == textBox3) next = textBox4;
            if (next != null)
            {
                next.Focus();
                next.SelectAll();
            }
            else
            {
                Button1_Click(sender, e);
            }
        }
    }
}
