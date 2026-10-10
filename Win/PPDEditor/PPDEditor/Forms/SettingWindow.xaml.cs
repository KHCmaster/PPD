using PPDEditor.ViewModels.Dialogs;
using System.Windows;
using System.Windows.Input;
using Keys = System.Windows.Forms.Keys;

namespace PPDEditor.Forms
{
    /// <summary>
    /// エディターの設定のウィンドウです。処理は <see cref="SettingViewModel"/> にあります。
    /// </summary>
    /// <remarks>
    /// ショートカットキーの入力（押したキーを受け取る）だけは、キーボードの扱いなのでここで行う。
    /// </remarks>
    public partial class SettingWindow : Window
    {
        public SettingWindow(SettingViewModel viewModel)
        {
            InitializeComponent();
            this.AttachDialog(viewModel);
            viewModel.SelectCommand = SelectCommand;
            viewModel.KeyInputRequested += (sender, e) => StartKeyInput();
        }

        private SettingViewModel ViewModel
        {
            get { return (SettingViewModel)DataContext; }
        }

        private string SelectCommand()
        {
            var viewModel = new CommandSelectViewModel();
            return new CommandSelectWindow(viewModel) { Owner = this }.ShowDialog() == true ? viewModel.SelectedScriptPath : null;
        }

        // コマンドを選んだら、キーを入力できるようにする
        private void StartKeyInput()
        {
            shortcutGrid.ScrollIntoView(ViewModel.SelectedRow);
            keyBox.IsEnabled = true;
            keyBox.Focus();
            keyBox.SelectAll();
        }

        /// <summary>
        /// 押したキーを、選んでいるコマンドのショートカットにします。Enter と Space は入力をやめます。
        /// </summary>
        private void KeyBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;

            // Alt を押しているときは SystemKey に本当のキーが入る
            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            if (key == Key.LeftCtrl || key == Key.RightCtrl || key == Key.LeftShift || key == Key.RightShift || key == Key.LeftAlt || key == Key.RightAlt)
            {
                return;
            }

            if (key == Key.Return || key == Key.Space)
            {
                shortcutGrid.Focus();
                ViewModel.CancelKeyInput();
                return;
            }

            // ショートカットは WinForms のキーの値で保存しているので、仮想キーコードを経由して変換する（値は同じ）
            var modifiers = Keyboard.Modifiers;
            ViewModel.AssignKey((Keys)KeyInterop.VirtualKeyFromKey(key), modifiers.HasFlag(ModifierKeys.Shift), modifiers.HasFlag(ModifierKeys.Control), modifiers.HasFlag(ModifierKeys.Alt));
        }

        private void KeyBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            keyBox.IsEnabled = false;
        }
    }
}
