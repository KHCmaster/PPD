using PPDEditor.ViewModels.Dialogs;
using System.Windows;

namespace PPDEditor.Forms
{
    /// <summary>
    /// MOD を発行するときの情報を入力するウィンドウです。処理は <see cref="ModPublishViewModel"/> にあります。
    /// </summary>
    public partial class ModPublishWindow : Window
    {
        public ModPublishWindow(ModPublishViewModel viewModel)
        {
            InitializeComponent();
            this.AttachDialog(viewModel);
        }
    }
}
