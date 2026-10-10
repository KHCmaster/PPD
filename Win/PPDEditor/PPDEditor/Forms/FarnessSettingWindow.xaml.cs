using PPDEditor.ViewModels.Dialogs;
using System.Windows;

namespace PPDEditor.Forms
{
    /// <summary>
    /// 遠さを設定するウィンドウです。処理は <see cref="FarnessSettingViewModel"/> にあります。
    /// </summary>
    public partial class FarnessSettingWindow : Window
    {
        public FarnessSettingWindow(FarnessSettingViewModel viewModel)
        {
            InitializeComponent();
            this.AttachDialog(viewModel);
        }
    }
}
