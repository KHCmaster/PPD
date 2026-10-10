using PPDEditor.ViewModels.Dialogs;
using System.Windows;

namespace PPDEditor.Forms
{
    /// <summary>
    /// プロジェクトをマージするウィンドウです。処理は <see cref="MergeViewModel"/> にあります。
    /// </summary>
    public partial class MergeWindow : Window
    {
        public MergeWindow(MergeViewModel viewModel)
        {
            InitializeComponent();
            this.AttachDialog(viewModel);
        }
    }
}
