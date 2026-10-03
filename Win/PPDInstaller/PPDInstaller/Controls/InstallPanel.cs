using System.Windows.Controls;

namespace PPDInstaller.Controls
{
    /// <summary>
    /// インストールするフォルダーを選ぶページです。処理は <see cref="ViewModels.InstallFolderPageViewModel"/> にあります。
    /// </summary>
    public partial class InstallPanel : UserControl
    {
        public InstallPanel()
        {
            InitializeComponent();
        }
    }
}
