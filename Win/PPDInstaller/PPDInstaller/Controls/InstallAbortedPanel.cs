using System.Windows.Controls;

namespace PPDInstaller.Controls
{
    /// <summary>
    /// インストールを中断したページです。処理は <see cref="ViewModels.InstallAbortedPageViewModel"/> にあります。
    /// </summary>
    public partial class InstallAbortedPanel : UserControl
    {
        public InstallAbortedPanel()
        {
            InitializeComponent();
        }
    }
}
