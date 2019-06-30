using System.Windows.Forms;

namespace PPDEditor.Forms
{
    public partial class NewLayerForm : Form
    {
        public NewLayerForm()
        {
            InitializeComponent();
        }
        public void SetLang()
        {
            this.Text = Utility.Language["LMNewLayer"];
            this.label1.Text = Utility.Language["LMNewLayerName"];
        }
        private void button1_MouseClick(object sender, MouseEventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_MouseClick(object sender, MouseEventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
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
            }
        }

        private void textBox1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                e.IsInputKey = true;
                DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
