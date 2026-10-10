using PPDEditor.ViewModels.Dialogs;
using System.Windows;

namespace PPDEditor.Forms
{
    /// <summary>
    /// ボタンごとに鳴らす音を変えるウィンドウです。処理は <see cref="SoundChangeEditViewModel"/> にあります。
    /// </summary>
    public partial class SoundChangeEditWindow : Window
    {
        public SoundChangeEditWindow(SoundChangeEditViewModel viewModel)
        {
            InitializeComponent();
            this.AttachDialog(viewModel);
        }
    }
}
