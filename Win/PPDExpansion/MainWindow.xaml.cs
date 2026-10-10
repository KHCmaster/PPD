using CommunityToolkit.Mvvm.Messaging;
using PPDExpansion.Message;
using PPDExpansion.ViewModel;
using System.Windows;

namespace PPDExpansion
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            WeakReferenceMessenger.Default.Register<CloseWindowMessage>(this, (r, m) => CloseWindowMessage_Received(m));
            Loaded += MainWindow_Loaded;
        }

        private void CloseWindowMessage_Received(CloseWindowMessage message)
        {
            if (message.Sender != DataContext)
            {
                return;
            }

            Close();
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = new MainWindowViewModel();
        }
    }
}
