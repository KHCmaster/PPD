using PPDEditor.ViewModels.Dialogs;
using System.Windows;

namespace PPDEditor.Forms
{
    /// <summary>
    /// 新しいレイヤーの名前を入力するウィンドウです。処理は <see cref="NewLayerViewModel"/> にあります。
    /// </summary>
    public partial class NewLayerWindow : Window
    {
        public NewLayerWindow(NewLayerViewModel viewModel)
        {
            InitializeComponent();
            this.AttachDialog(viewModel);
            // 既定の名前をすぐ書き換えられるように、全部選んでおく
            nameTextBox.SelectAll();
        }
    }
}
