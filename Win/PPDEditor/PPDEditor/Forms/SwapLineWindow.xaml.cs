using PPDEditor.ViewModels.Dialogs;
using System.Windows;

namespace PPDEditor.Forms
{
    /// <summary>
    /// 入れ替えるラインを選ぶウィンドウです。処理は <see cref="SwapLineViewModel"/> にあります。
    /// </summary>
    public partial class SwapLineWindow : Window
    {
        public SwapLineWindow(SwapLineViewModel viewModel)
        {
            InitializeComponent();
            this.AttachDialog(viewModel);
        }
    }
}
