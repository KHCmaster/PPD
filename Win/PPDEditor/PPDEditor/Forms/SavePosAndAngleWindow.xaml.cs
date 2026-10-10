using PPDEditor.ViewModels.Dialogs;
using System.Windows;

namespace PPDEditor.Forms
{
    /// <summary>
    /// マークの位置（と角度）を保存する名前を入力するウィンドウです。処理は <see cref="SavePosAndAngleViewModel"/> にあります。
    /// </summary>
    public partial class SavePosAndAngleWindow : Window
    {
        public SavePosAndAngleWindow(SavePosAndAngleViewModel viewModel)
        {
            InitializeComponent();
            this.AttachDialog(viewModel);
            // 使えない名前だったら、入力し直しやすいように全部選ぶ
            viewModel.FileNameRejected += (sender, e) =>
            {
                nameTextBox.Focus();
                nameTextBox.SelectAll();
            };
        }
    }
}
