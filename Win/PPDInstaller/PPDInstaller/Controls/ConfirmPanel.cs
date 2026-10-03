using System.Windows.Controls;

namespace PPDInstaller.Controls
{
    /// <summary>
    /// インストールするものを確かめ、インストールの進み具合を表示するページです。処理は <see cref="ViewModels.ConfirmPageViewModel"/> にあります。
    /// </summary>
    public partial class ConfirmPanel : UserControl
    {
        public ConfirmPanel()
        {
            InitializeComponent();
        }
    }
}
