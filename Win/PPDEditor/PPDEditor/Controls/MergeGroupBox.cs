using System;
using System.Windows.Forms;

namespace PPDEditor.Controls
{
    public partial class MergeGroupBox : UserControl
    {
        public event EventHandler ButtonPressed;
        public MergeGroupBox()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (ButtonPressed != null)
            {
                ButtonPressed.Invoke(this, EventArgs.Empty);
            }
        }

        public int ProjectVersion
        {
            get;
            set;
        }

        public string ProjectPath
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

        public string Label
        {
            get
            {
                return label1.Text;
            }
            set
            {
                label1.Text = value;
            }
        }

        public string GroupName
        {
            get
            {
                return groupBox1.Text;
            }
            set
            {
                groupBox1.Text = value;
            }
        }

        public ComboBox ComboBox
        {
            get
            {
                return comboBox1;
            }
        }
    }
}
