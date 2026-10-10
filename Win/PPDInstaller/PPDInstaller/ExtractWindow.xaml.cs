using System;
using System.Threading;
using System.Windows;

namespace PPDInstaller
{
    /// <summary>
    /// データファイルを展開している間に表示するウィンドウです。展開が終わると閉じます。
    /// </summary>
    public partial class ExtractWindow : Window
    {
        public string UnzipFilePath
        {
            get;
            set;
        }

        public string ExtractDir
        {
            get;
            set;
        }

        public ExtractWindow()
        {
            InitializeComponent();
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            NativeMethods.HideSystemMenu(this);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            new Thread(() =>
            {
                Utility.Unzip(UnzipFilePath, ExtractDir);
                Dispatcher.Invoke(Close);
            }).Start();
        }
    }
}
