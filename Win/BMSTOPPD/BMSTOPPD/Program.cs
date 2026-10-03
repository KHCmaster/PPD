using BMSTOPPD.Models;
using BMSTOPPD.ViewModels;
using System;
using System.Text;
using System.Windows;

namespace BMSTOPPD
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            // 譜面は Shift_JIS で読むので、コードページのエンコーディングを使えるようにする
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var settings = MarkSettingStore.Load();
            var settingWindow = new SettingWindow { DataContext = settings };
            var mainWindow = new MainWindow(settingWindow);
            mainWindow.DataContext = new MainWindowViewModel(mainWindow, settings);
            // 設定のウィンドウは閉じても隠れるだけなので、メインのウィンドウを閉じたら終了する
            var application = new Application { ShutdownMode = ShutdownMode.OnMainWindowClose };
            application.Run(mainWindow);
            MarkSettingStore.Save(settings);
        }
    }
}
