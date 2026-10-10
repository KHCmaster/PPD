using PPDEditor.ViewModels.Dialogs;
using System.Windows;

namespace PPDEditor.Forms
{
    /// <summary>
    /// マークのパラメーターを編集するウィンドウです。処理は <see cref="EditParameterViewModel"/> にあります。
    /// </summary>
    public partial class EditParameterWindow : Window
    {
        public EditParameterWindow(EditParameterViewModel viewModel)
        {
            InitializeComponent();
            this.AttachDialog(viewModel);
        }

        // 編集中のセルの値を確定してから OK にする
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            parameterGrid.CommitEdit();
            parameterGrid.CommitEdit();
            ((EditParameterViewModel)DataContext).OkCommand.Execute(null);
        }
    }
}
