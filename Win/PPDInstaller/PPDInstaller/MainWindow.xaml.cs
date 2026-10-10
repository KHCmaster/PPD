using PPDInstaller.ViewModels;
using System.ComponentModel;
using System.Windows;

namespace PPDInstaller
{
    /// <summary>
    /// インストーラーのウィンドウです。処理は <see cref="MainWindowViewModel"/> にあります。
    /// </summary>
    public partial class MainWindow : Window, IWindowService
    {
        public MainWindow(LanguageTexts texts, string dataDirectory)
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(this, texts, dataDirectory);
        }

        private MainWindowViewModel ViewModel
        {
            get { return (MainWindowViewModel)DataContext; }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = !ViewModel.CanClose();
        }

        public string SelectFolder(string initialDirectory)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog { InitialDirectory = initialDirectory };
            return dialog.ShowDialog(this) == true ? dialog.FolderName : null;
        }

        public void ShowMessage(string text)
        {
            MessageBox.Show(this, text);
        }

        public bool Confirm(string text, string caption)
        {
            return MessageBox.Show(this, text, caption, MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes;
        }
    }
}
