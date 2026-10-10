using Effect2DEditor.ViewModels.Dialogs;
using System.Windows;

namespace Effect2DEditor
{
    /// <summary>
    /// カンバスの大きさを変えるウィンドウです。処理は <see cref="CanvasSizeViewModel"/> にあります。
    /// </summary>
    public partial class CanvasSizeWindow : Window
    {
        public CanvasSizeWindow()
        {
            InitializeComponent();
        }
    }
}
