using PPDEditor.ViewModels.Dialogs;
using System.Windows;

namespace PPDEditor.Forms
{
    /// <summary>
    /// 値の候補から 1 つを選ぶウィンドウです。処理は <see cref="SuggestViewModel"/> にあります。
    /// </summary>
    public partial class SuggestWindow : Window
    {
        public SuggestWindow(SuggestViewModel viewModel)
        {
            InitializeComponent();
            this.AttachDialog(viewModel);
        }
    }
}
