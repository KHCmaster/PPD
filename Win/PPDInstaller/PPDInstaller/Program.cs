using ErrorHandle;
using PPDInstaller.ViewModels;
using System;
using System.IO;
using System.Windows;

namespace PPDInstaller
{
    static class Program
    {
        const string DataFilePath = "Data.pak";
        const string CacheFolder = "Cache";
        const string LangListPath = "Lang\\LangList.ini";

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            var errorHandler = new ErrorHandler();
            errorHandler.Initialize();
            System.Environment.CurrentDirectory = AppContext.BaseDirectory;
            try
            {
                var application = new Application
                {
                    // メインウィンドウの前に言語の選択と展開のウィンドウを出すので、それらを閉じても終了しないようにする
                    ShutdownMode = ShutdownMode.OnExplicitShutdown
                };
                Directory.CreateDirectory(CacheFolder);
                var texts = SelectLanguage();
                if (!File.Exists(DataFilePath))
                {
                    MessageBox.Show(texts["NonExistDataFile"]);
                    Environment.Exit(-1);
                    return;
                }
                var dataDirectory = Extract();

                var window = new MainWindow(texts, dataDirectory);
                application.ShutdownMode = ShutdownMode.OnMainWindowClose;
                application.Run(window);
            }
            catch (Exception e)
            {
                errorHandler.ProcessError(e);
            }
        }

        // 言語の一覧があれば、言語を選ばせて読み込む
        private static LanguageTexts SelectLanguage()
        {
            var texts = new LanguageTexts();
            if (File.Exists(LangListPath))
            {
                var window = new LangSelectWindow(File.ReadAllLines(LangListPath));
                window.ShowDialog();
                texts.Load(String.Format("Lang\\{0}.ini", window.SelectedIndex));
            }
            return texts;
        }

        // Data.pak を一時フォルダーに展開して、展開したデータのフォルダーを返す
        private static string Extract()
        {
            var extractDialog = new ExtractWindow
            {
                ExtractDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()),
                UnzipFilePath = DataFilePath
            };
            Directory.CreateDirectory(extractDialog.ExtractDir);
            extractDialog.ShowDialog();
            return Path.Combine(extractDialog.ExtractDir, "Data");
        }
    }
}
