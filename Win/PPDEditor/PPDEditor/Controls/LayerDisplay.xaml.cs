using PPDEditor.ViewModels.Docks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PPDEditor.Controls
{
    /// <summary>
    /// レイヤーの一覧の 1 行です。DataContext は <see cref="LayerViewModel"/> です。
    /// </summary>
    /// <remarks>
    /// クリックで選び（ドラッグでの並べ替えはレイヤーの一覧が扱う）、表示の切り替えと色の部分ではクリックを止める。
    /// </remarks>
    public partial class LayerDisplay : UserControl
    {
        public LayerDisplay()
        {
            InitializeComponent();
        }

        private LayerViewModel ViewModel
        {
            get { return (LayerViewModel)DataContext; }
        }

        private void LayerDisplay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Focus();
            ViewModel.SelectCommand.Execute(null);
        }

        // 右クリックのメニューを出すときも選ぶ
        private void LayerDisplay_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            Focus();
            ViewModel.SelectCommand.Execute(null);
        }

        private void VisibleToggle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 選択やドラッグにならないように、ここで止める
            e.Handled = true;
            ViewModel.ToggleVisibleCommand.Execute(null);
        }

        private void Name_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                e.Handled = true;
                ViewModel.BeginRenameCommand.Execute(null);
            }
        }

        // 名前の編集を始めたら、すぐ入力できるようにする
        private void NameBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (nameBox.IsVisible)
            {
                nameBox.Focus();
                nameBox.SelectAll();
            }
        }

        private void NameBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                e.Handled = true;
                ViewModel.EndRenameCommand.Execute(null);
            }
        }

        private void NameBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ViewModel.EndRenameCommand.Execute(null);
        }

        private void MarkColor_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 選択やドラッグにならないように、ここで止める
            e.Handled = true;
            if (e.ClickCount == 2)
            {
                ViewModel.ChangeColorCommand.Execute(null);
            }
        }
    }
}
