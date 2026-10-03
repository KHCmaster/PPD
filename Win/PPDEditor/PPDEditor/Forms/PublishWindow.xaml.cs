using PPDEditor.ViewModels.Dialogs;
using System.Windows;

namespace PPDEditor.Forms
{
    /// <summary>
    /// 譜面フォルダーを発行するウィンドウです。処理は <see cref="PublishViewModel"/> にあります。
    /// </summary>
    public partial class PublishWindow : Window
    {
        public PublishWindow(PublishViewModel viewModel)
        {
            InitializeComponent();
            this.AttachDialog(viewModel);
        }
    }
}
