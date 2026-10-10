using System.Windows.Controls;

namespace PPDInstaller.Controls
{
    /// <summary>
    /// 最初のページ（かんたんインストールかカスタムインストールかを選ぶ）です。処理は <see cref="ViewModels.StartPageViewModel"/> にあります。
    /// </summary>
    public partial class StartPanel : UserControl
    {
        public StartPanel()
        {
            InitializeComponent();
        }
    }
}
