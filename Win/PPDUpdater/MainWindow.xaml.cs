using PPDUpdater.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace PPDUpdater
{
    /// <summary>
    /// アップデートの確認、ダウンロード、インストールを行うウィンドウです。処理は <see cref="MainWindowViewModel"/> にあります。
    /// </summary>
    /// <remarks>
    /// 引数（-d、-i）でダウンロードだけ・インストールだけを指定したときは、ウィンドウを表示せずに処理して終了する。
    /// </remarks>
    public partial class MainWindow : Window, IWindowService
    {
        public MainWindow(WorkState workState)
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(this, workState);
        }

        private MainWindowViewModel ViewModel
        {
            get { return (MainWindowViewModel)DataContext; }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel.IsInteractive)
            {
                ShowInTaskbar = true;
                WindowState = WindowState.Normal;
                Activate();
            }
            else
            {
                Hide();
            }

            // 表示の処理が終わってから始める（処理の中でウィンドウを閉じることがあるため）
            Dispatcher.BeginInvoke(DispatcherPriority.Background, ViewModel.Start);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ViewModel.SaveOnClosing();
        }

        public void ShowMessage(string message)
        {
            MessageBox.Show(this, message);
        }

        public bool Confirm(string message, string caption)
        {
            return MessageBox.Show(this, message, caption, MessageBoxButton.YesNo) == MessageBoxResult.Yes;
        }
    }
}
