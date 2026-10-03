using PPDConfig.ViewModels;
using System;
using System.Windows;

namespace PPDConfig
{
    /// <summary>
    /// PPD の設定（PPD.ini）を編集するウィンドウです。処理は <see cref="MainWindowViewModel"/> にあります。
    /// </summary>
    public partial class MainWindow : Window, IWindowService
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContextChanged += (sender, e) =>
            {
                if (e.NewValue is MainWindowViewModel viewModel)
                {
                    blueSkyPasswordBox.Password = viewModel.BlueSkyPassword;
                }
            };
        }

        private void BlueSkyPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.BlueSkyPassword = blueSkyPasswordBox.Password;
            }
        }

        public void ShowMessage(string text)
        {
            MessageBox.Show(this, text);
        }

        void IWindowService.Close()
        {
            Close();
        }
    }
}
