using PPDEditor.ViewModels;
using System.Windows;

namespace PPDEditor.Forms
{
    /// <summary>
    /// ダイアログのウィンドウと ViewModel をつなぎます。
    /// </summary>
    static class DialogWindowExtensions
    {
        /// <summary>
        /// ViewModel を DataContext にして、ViewModel が閉じてほしいときにウィンドウを閉じるようにします。
        /// </summary>
        public static void AttachDialog(this Window window, DialogViewModel viewModel)
        {
            window.DataContext = viewModel;
            viewModel.View = new DialogView(window);
            viewModel.CloseRequested += result => window.DialogResult = result;
        }

        sealed class DialogView(Window window) : IDialogView
        {
            public void ShowMessage(string text)
            {
                MessageBox.Show(window, text);
            }
        }
    }
}
