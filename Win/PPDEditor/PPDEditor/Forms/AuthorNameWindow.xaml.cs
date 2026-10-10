using PPDEditor.ViewModels.Dialogs;
using System.Windows;

namespace PPDEditor.Forms
{
    /// <summary>
    /// 作者名を入力するウィンドウです。処理は <see cref="AuthorNameViewModel"/> にあります。
    /// </summary>
    public partial class AuthorNameWindow : Window
    {
        public AuthorNameWindow(AuthorNameViewModel viewModel)
        {
            InitializeComponent();
            this.AttachDialog(viewModel);
        }
    }
}
