using System.Windows;

namespace PPDEditor.Forms
{
    public partial class NewLayerWindow : Window
    {
        public NewLayerWindow()
        {
            InitializeComponent();
        }
        public void SetLang()
        {
            this.Title = Utility.Language["LMNewLayer"];
            this.label1.Text = Utility.Language["LMNewLayerName"];
        }
        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
        public string LayerName
        {
            get
            {
                return this.textBox1.Text;
            }
            set
            {
                this.textBox1.Text = value;
                this.textBox1.SelectAll();
            }
        }
    }
}
