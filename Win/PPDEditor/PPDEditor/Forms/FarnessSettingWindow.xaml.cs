using System;
using System.Windows;

namespace PPDEditor.Forms
{
    public partial class FarnessSettingWindow : Window
    {
        public FarnessSettingWindow()
        {
            InitializeComponent();
        }
        public float Farness
        {
            get
            {
                return (float)(Math.Round(trackBar1.Value) / trackBar1.Maximum);
            }
            set
            {
                trackBar1.Value = (int)Math.Min(value * 100, trackBar1.Maximum);
            }
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
