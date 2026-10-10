using PPDEditor.ViewModels.Dialogs;
using System.Windows;

namespace PPDEditor.Forms
{
    /// <summary>
    /// 歌詞を修正するウィンドウです。処理は <see cref="ChangeKasiViewModel"/> にあります。
    /// </summary>
    public partial class ChangeKasiWindow : Window
    {
        public ChangeKasiWindow(ChangeKasiViewModel viewModel)
        {
            InitializeComponent();
            this.AttachDialog(viewModel);
        }
    }
}
