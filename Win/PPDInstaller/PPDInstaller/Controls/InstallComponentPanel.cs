using System.Windows.Controls;

namespace PPDInstaller.Controls
{
    /// <summary>
    /// インストールするものを選ぶページです。処理は <see cref="ViewModels.InstallComponentPageViewModel"/> にあります。
    /// </summary>
    public partial class InstallComponentPanel : UserControl
    {
        public InstallComponentPanel()
        {
            InitializeComponent();
        }
    }
}
