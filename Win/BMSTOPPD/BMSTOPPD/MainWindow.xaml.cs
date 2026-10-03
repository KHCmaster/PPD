using BMSTOPPD.ViewModels;
using System.Windows;

namespace BMSTOPPD
{
    /// <summary>
    /// 変換の画面です。処理は <see cref="MainWindowViewModel"/> にあり、ここではダイアログを出すだけです。
    /// </summary>
    public partial class MainWindow : Window, IDialogService
    {
        readonly SettingWindow settingWindow;

        public MainWindow(SettingWindow settingWindow)
        {
            this.settingWindow = settingWindow;
            InitializeComponent();
        }

        public string OpenFile(string filter)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = filter,
                RestoreDirectory = true,
            };
            return dialog.ShowDialog(this) == true ? dialog.FileName : null;
        }

        public void ShowMessage(string text)
        {
            MessageBox.Show(this, text);
        }

        public void ShowSettingWindow()
        {
            settingWindow.Owner ??= this;
            settingWindow.Show();
            settingWindow.Activate();
        }
    }
}
