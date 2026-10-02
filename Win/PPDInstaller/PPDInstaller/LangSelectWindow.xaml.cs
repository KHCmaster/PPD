using System;
using System.Windows;

namespace PPDInstaller
{
    public partial class LangSelectWindow : Window
    {
        public LangSelectWindow()
        {
            InitializeComponent();
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            NativeMethods.HideSystemMenu(this);
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void SetLang(string[] langs)
        {
            foreach (var s in langs)
            {
                comboBox1.Items.Add(s);
            }
            comboBox1.SelectedIndex = 0;
        }

        public int SelectedIndex
        {
            get
            {
                return comboBox1.SelectedIndex;
            }
        }
    }
}
