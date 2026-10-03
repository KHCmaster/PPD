using System.Windows;
using System.Windows.Media;

namespace Effect2DEditor
{
    /// <summary>
    /// 色を選ぶウィンドウです。
    /// </summary>
    public partial class ColorPickWindow : Window
    {
        public ColorPickWindow()
        {
            InitializeComponent();
        }

        public Color SelectedColor
        {
            get { return colorCanvas.SelectedColor; }
            set { colorCanvas.SelectedColor = value; }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
