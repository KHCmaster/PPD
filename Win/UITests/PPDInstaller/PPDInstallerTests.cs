using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Xunit;

namespace UITests.PPDInstaller
{
    /// <summary>
    /// PPDInstaller（PPD のインストーラー）の画面のテストです。
    /// </summary>
    /// <remarks>
    /// インストールはしない。「かんたんインストール」と、確認のページの「インストール」（次へ）は押さない（<see cref="PressNext"/> が止める）。
    /// 本物のデータ（Data.pak）はコピーせず、中身が空に近いものを作って使う。
    /// 管理者として実行するアプリだが、昇格すると UI Automation で操作できないので、昇格しないで起動する。
    /// </remarks>
    public sealed class PPDInstallerTests : IDisposable
    {
        readonly AppSession session = AppSession.Start(Path.Combine("PPDInstaller", "PPDInstaller"), "PPDInstaller.exe", new AppStartOptions
        {
            Prepare = workDirectory =>
            {
                CreateDummyData(Path.Combine(workDirectory, "Data.pak"));
                Assert.True(new FileInfo(Path.Combine(workDirectory, "Data.pak")).Length < 10000, "本物のデータを使ってはいけない");
                Directory.CreateDirectory(Path.Combine(workDirectory, "tmp"));
            },
            Environment =
            {
                // マニフェストの管理者の指定を無視して、昇格しないで起動する
                ["__COMPAT_LAYER"] = "RunAsInvoker",
                // データを展開する一時フォルダを、作業のフォルダの中にする（テストの後に一緒に消す）
                ["TMP"] = @"{WorkDirectory}\tmp",
                ["TEMP"] = @"{WorkDirectory}\tmp",
            },
        });

        public void Dispose()
        {
            session.Dispose();
        }

        /// <summary>
        /// テスト用の、データのフォルダに 1 つのファイルがあるだけのデータを作ります。
        /// </summary>
        private static void CreateDummyData(string path)
        {
            using var archive = ZipFile.Open(path, ZipArchiveMode.Create);
            var entry = archive.CreateEntry("Data/readme.txt");
            using var writer = new StreamWriter(entry.Open());
            writer.Write("dummy data for UI tests");
        }

        /// <summary>
        /// 言語を選んで、インストーラーのウィンドウを返します。
        /// </summary>
        private Window SelectLanguage(string language)
        {
            var languageWindow = session.MainWindow;
            Assert.Equal("Language", languageWindow.Title);
            var combo = languageWindow.FindFirstDescendant(cf => cf.ByAutomationId("LanguageCombo")).AsComboBox();
            combo.Select(language);
            AppSession.InvokeAsync(languageWindow.FindFirstDescendant(cf => cf.ByAutomationId("OkButton")));
            return session.WaitForWindow("PPDInstaller");
        }

        private static AutomationElement Find(Window window, string automationId)
        {
            AutomationElement element = null;
            AppSession.WaitUntil(() => (element = window.FindFirstDescendant(cf => cf.ByAutomationId(automationId))) != null);
            return element ?? throw new InvalidOperationException(automationId + " が見つかりません。");
        }

        private static bool IsShown(Window window, string pageId)
        {
            return AppSession.WaitUntil(() => window.FindFirstDescendant(cf => cf.ByAutomationId(pageId)) != null);
        }

        /// <summary>
        /// 次へを押します。確認のページでは、インストールが始まるので押さない。
        /// </summary>
        private static void PressNext(Window window)
        {
            if (window.FindFirstDescendant(cf => cf.ByAutomationId("ConfirmPage")) != null)
            {
                throw new InvalidOperationException("確認のページで次へを押すとインストールが始まるので、押してはいけない。");
            }
            Find(window, "NextButton").AsButton().Invoke();
        }

        /// <summary>
        /// カスタムインストールで、確認のページまで進めます。
        /// </summary>
        private static void GoToConfirmPage(Window window)
        {
            Find(window, "CustomInstallButton").AsButton().Invoke();
            Assert.True(IsShown(window, "InstallFolderPage"));
            PressNext(window);
            Assert.True(IsShown(window, "ComponentPage"));
            PressNext(window);
            Assert.True(IsShown(window, "LinkCreatePage"));
            PressNext(window);
            Assert.True(IsShown(window, "ConfirmPage"));
        }

        [Fact]
        public void CustomInstall_GoesThroughPagesToConfirmation()
        {
            var window = SelectLanguage("English");
            Assert.True(IsShown(window, "StartPage"));
            Assert.Equal("Custom Install", Find(window, "CustomInstallButton").Name);

            Find(window, "CustomInstallButton").AsButton().Invoke();
            Assert.True(IsShown(window, "InstallFolderPage"));
            Assert.NotEqual("", Find(window, "InstallFolderBox").AsTextBox().Text);
            PressNext(window);
            Assert.True(IsShown(window, "ComponentPage"));
            Assert.True(Find(window, "InstallPPDCheck").AsCheckBox().IsChecked);
            PressNext(window);
            Assert.True(IsShown(window, "LinkCreatePage"));
            PressNext(window);
            Assert.True(IsShown(window, "ConfirmPage"));

            // 確認のページで次へを押すと、インストールが始まる（押さない）
            Assert.True(Find(window, "NextButton").IsEnabled);
            Assert.NotEmpty(Find(window, "ConfirmTree").FindAllDescendants(cf => cf.ByControlType(ControlType.TreeItem)));
        }

        [Fact]
        public void Previous_ReturnsToPreviousPage()
        {
            var window = SelectLanguage("English");
            GoToConfirmPage(window);

            Find(window, "PreviousButton").AsButton().Invoke();

            Assert.True(IsShown(window, "LinkCreatePage"));
            Assert.True(Find(window, "StartMenuCheck").AsCheckBox().IsChecked.HasValue);
        }

        [Fact]
        public void Cancel_Confirmed_ShowsAbortedPageThenFinishes()
        {
            var window = SelectLanguage("English");
            Assert.True(IsShown(window, "StartPage"));

            AppSession.InvokeAsync(Find(window, "CancelButton"));
            MessageBoxes.Press(session.WaitForModal(window), MessageBoxButton.Yes);

            Assert.True(IsShown(window, "AbortedPage"));
            Assert.Equal("Finish", Find(window, "NextButton").Name);
            Find(window, "NextButton").AsButton().Invoke();
            Assert.True(session.WaitForExit());
        }

        [Fact]
        public void Cancel_Declined_StaysOnPage()
        {
            var window = SelectLanguage("English");
            Assert.True(IsShown(window, "StartPage"));

            AppSession.InvokeAsync(Find(window, "CancelButton"));
            MessageBoxes.Press(session.WaitForModal(window), MessageBoxButton.No);

            Assert.True(AppSession.WaitUntil(() => window.ModalWindows.Length == 0));
            Assert.True(IsShown(window, "StartPage"));
            Assert.False(session.HasExited);
        }

        [Fact]
        public void Language_Japanese_ShowsJapaneseTexts()
        {
            var window = SelectLanguage("日本語");

            Assert.True(IsShown(window, "StartPage"));
            Assert.NotEqual("Custom Install", Find(window, "CustomInstallButton").Name);
            Assert.Equal("キャンセル", Find(window, "CancelButton").Name);
        }
    }
}
