using System;
using System.Windows;
using System.Windows.Controls;

namespace PPDEditor.Controls
{
    /// <summary>
    /// マージするプロジェクトと難易度を選ぶ部分です。
    /// </summary>
    public partial class MergeGroupBox : UserControl
    {
        public event EventHandler ButtonPressed;
        public MergeGroupBox()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            ButtonPressed?.Invoke(this, EventArgs.Empty);
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
                return groupBox1.Header as string;
            }
            set
            {
                groupBox1.Header = value;
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
