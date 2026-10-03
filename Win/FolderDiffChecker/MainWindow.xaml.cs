using FolderDiffChecker.ViewModels;
using System.IO;
using System.Reflection;
using System.Windows;

namespace FolderDiffChecker
{
    /// <summary>
    /// 2 つのリリースフォルダーの差分を調べるウィンドウです。処理は <see cref="MainWindowViewModel"/> にあります。
    /// </summary>
    public partial class MainWindow : Window, IDialogService
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(this, GetReleasesDir());
        }

        // FolderDiffChecker\bin\x64\Debug\FolderDiffChecker.dll から 5 つ上が Win フォルダー
        private static string GetReleasesDir()
        {
            string path = Assembly.GetExecutingAssembly().Location;
            for (int i = 0; i < 5; i++)
            {
                path = Path.GetDirectoryName(path);
            }
            return Path.Combine(path, "release", "releases");
        }

        public string SelectFolder(string initialDirectory)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog { InitialDirectory = initialDirectory };
            return dialog.ShowDialog(this) == true ? dialog.FolderName : null;
        }

        public string AskUserName(string lastUserName)
        {
            var window = new UserWindow { Owner = this, UserName = lastUserName };
            return window.ShowDialog() == true ? window.UserName : null;
        }

        public void ShowMessage(string text)
        {
            MessageBox.Show(this, text);
        }
    }
}
