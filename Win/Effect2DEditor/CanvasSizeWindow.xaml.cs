using PPDConfiguration;
using System.Windows;

namespace Effect2DEditor
{
    public partial class CanvasSizeWindow : Window
    {
        public CanvasSizeWindow()
        {
            InitializeComponent();
        }
        public void SetLang(SettingReader setting)
        {
            if (setting != null)
            {
                this.Title = setting["CSFText"];
                this.label1.Text = setting["CSFLabel1"];
                this.label2.Text = setting["CSFLabel2"];
            }
        }
        public int CanvasWidth
        {
            get
            {
                if (!int.TryParse(this.textBox1.Text, out int val)) val = 256;
                return val;
            }
            set
            {
                this.textBox1.Text = value.ToString();
            }
        }
        public int CanvasHeight
        {
            get
            {
                if (!int.TryParse(this.textBox2.Text, out int val)) val = 256;
                return val;
            }
            set
            {
                this.textBox2.Text = value.ToString();
            }
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
