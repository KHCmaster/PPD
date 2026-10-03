using PPDConfiguration;
using System.Windows;

namespace Effect2DEditor
{
    public partial class AskFrameCountWindow : Window
    {
        public AskFrameCountWindow()
        {
            InitializeComponent();
        }
        public void SetLang(SettingReader setting)
        {
            if (setting != null)
            {
                this.Title = setting["AFCText"];
                this.label1.Text = setting["AFCLabel1"];
                this.label2.Text = setting["AFCLabel2"];
                this.checkBox1.Content = setting["AFCCheckBox1"];
            }
        }
        public int FrameCount
        {
            get
            {
                if (!int.TryParse(this.textBox1.Text, out int val)) val = 1;
                if (val < 1) val = 1;
                return val;
            }
        }
        public bool WithReverse
        {
            get
            {
                return checkBox1.IsChecked == true;
            }
        }
        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
