using System.Windows;

namespace PPDEditor.Forms
{
    public partial class AuthorNameWindow : Window
    {
        public AuthorNameWindow()
        {
            InitializeComponent();
        }
        public void SetLang()
        {
            this.label1.Text = Utility.Language["AuthorNameLabel"];
        }
        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
        public string AuthorName
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
    }
}
