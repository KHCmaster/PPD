using System;
using System.Windows;
using System.Windows.Media;

namespace FlowScriptDrawControl.Dialog
{
    /// <summary>
    /// ColorDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class ColorDialog : Window
    {
        public Color CurrentColor
        {
            get
            {
                return colorCanvas.SelectedColor;
            }
            set
            {
                colorCanvas.SelectedColor = value;
            }
        }

        public ColorDialog()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            NativeMethods.RemoveIcon(this);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
