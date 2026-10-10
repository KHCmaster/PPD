using PPDEditor.ViewModels.Dialogs;
using System.Windows;

namespace PPDEditor.Forms
{
    /// <summary>
    /// 譜面をコピーする先の難易度を選ぶウィンドウです。処理は <see cref="CopyDifficultyViewModel"/> にあります。
    /// </summary>
    public partial class CopyDifficultyWindow : Window
    {
        public CopyDifficultyWindow(CopyDifficultyViewModel viewModel)
        {
            InitializeComponent();
            this.AttachDialog(viewModel);
        }
    }
}
