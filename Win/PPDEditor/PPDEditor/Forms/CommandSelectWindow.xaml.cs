using PPDEditor.ViewModels.Dialogs;
using System.Windows;

namespace PPDEditor.Forms
{
    /// <summary>
    /// 実行するコマンドを選ぶウィンドウです。処理は <see cref="CommandSelectViewModel"/> にあります。
    /// </summary>
    public partial class CommandSelectWindow : Window
    {
        public CommandSelectWindow(CommandSelectViewModel viewModel)
        {
            InitializeComponent();
            this.AttachDialog(viewModel);
        }

        // TreeView の選択は結び付けられないので、ここで ViewModel に渡す
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ((CommandSelectViewModel)DataContext).SelectedNode = e.NewValue as CommandNode;
        }
    }
}
