using PPDEditor.ViewModels.Dialogs;
using System.Windows;

namespace PPDEditor.Forms
{
    /// <summary>
    /// タイムラインの行の順番と表示を編集するウィンドウです。処理は <see cref="EditTimeLineRowViewModel"/> にあります。
    /// </summary>
    public partial class EditTimeLineRowWindow : Window
    {
        public EditTimeLineRowWindow(EditTimeLineRowViewModel viewModel)
        {
            InitializeComponent();
            this.AttachDialog(viewModel);
        }
    }
}
