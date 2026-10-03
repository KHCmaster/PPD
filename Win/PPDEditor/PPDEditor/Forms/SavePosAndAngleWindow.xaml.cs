using System.IO;
using System.Windows;

namespace PPDEditor.Forms
{
    public partial class SavePosAndAngleWindow : Window
    {
        string NameError1 = "名称には何かを入力してください";
        string NameError2 = "入力された名称はすでに使用されております";

        public bool WithAngle
        {
            get
            {
                return radioButton2.IsChecked == true;
            }
        }

        public string FileName
        {
            get;
            private set;
        }

        public SavePosAndAngleWindow(string text)
        {
            InitializeComponent();
            this.label2.Text = text;
        }

        public void SetLang()
        {
            this.label1.Text = Utility.Language["PAALSDialogLabel1"];
            this.radioButton1.Content = Utility.Language["PAALSDialogRadio1"];
            this.radioButton2.Content = Utility.Language["PAALSDialogRadio2"];
            NameError1 = Utility.Language["PAALSNameError1"];
            NameError2 = Utility.Language["PAALSNameError2"];
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            FileName = this.textBox1.Text;
            if (FileName == "")
            {
                MessageBox.Show(this, NameError1);
            }
            else if (File.Exists("posdat\\" + FileName + ".txt"))
            {
                MessageBox.Show(this, NameError2);
                this.textBox1.Focus();
                this.textBox1.SelectAll();
            }
            else
            {
                this.DialogResult = true;
            }
        }
    }
}
