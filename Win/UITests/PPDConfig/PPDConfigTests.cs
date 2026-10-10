using FlaUI.Core.AutomationElements;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace UITests.PPDConfig
{
    /// <summary>
    /// PPDConfig（PPD の設定 PPD.ini を編集するツール）の画面のテストです。
    /// </summary>
    /// <remarks>
    /// 本物の PPD.ini（アカウントの情報がある）はコピーせず、テスト用の PPD.ini を作る。
    /// BlueSky のタブは開かない。OK（保存）は押さない。
    /// </remarks>
    public sealed class PPDConfigTests : IDisposable
    {
        /// <summary>
        /// テスト用の PPD.ini です（BlueSky の項目は入れない）。
        /// </summary>
        const string dummyPpdIni = "[width]1280\n[height]720\n[adjustgaptime]0.5\n[fontname]TestFont\n[fontsize]24\n[movielatency]0.25\n"
            + "[movieextensions]mp4\n[musicextensions]ogg\n[fullscreen]1\n[textboxdisabled]1\n[runexpansion]1\n[expansionwaitport]12345\n";

        readonly AppSession session = AppSession.Start("PPDConfig", "PPDConfig.exe", new AppStartOptions
        {
            // 言語の設定もコピーしない（既定の言語で始める）
            ExcludedFiles = ["PPDConfig.ini"],
            Prepare = workDirectory =>
            {
                var path = Path.Combine(workDirectory, "PPD.ini");
                Assert.False(File.Exists(path), "本物の PPD.ini をコピーしてはいけない");
                File.WriteAllText(path, dummyPpdIni);
            },
        });

        public void Dispose()
        {
            session.Dispose();
        }

        private string PpdIniPath
        {
            get { return Path.Combine(session.WorkDirectory, "PPD.ini"); }
        }

        private string TextOf(string automationId)
        {
            return session.Find(automationId).AsTextBox().Text;
        }

        private bool? CheckedOf(string automationId)
        {
            return session.Find(automationId).AsCheckBox().IsChecked;
        }

        private void SelectTab(string automationId)
        {
            session.Find(automationId).AsTabItem().Select();
        }

        [Fact]
        public void Startup_ShowsValuesOfPpdIni()
        {
            Assert.Equal("1280,720", TextOf("WindowSizeBox"));
            Assert.True(CheckedOf("FullScreenCheck"));
            Assert.Equal("0.5", TextOf("MonitorLatencyBox"));
            Assert.Equal("0.25", TextOf("MovieLatencyBox"));
            Assert.Equal("mp4", TextOf("MovieExtensionsBox"));
            Assert.Equal("ogg", TextOf("MusicExtensionsBox"));
            Assert.Equal("TestFont", TextOf("FontNameBox"));
            Assert.Equal("24", TextOf("FontSizeBox"));

            SelectTab("DetailTab");
            Assert.True(CheckedOf("TextBoxDisabledCheck"));

            SelectTab("ExpansionTab");
            Assert.True(CheckedOf("RunExpansionCheck"));
            Assert.Equal("12345", TextOf("ExpansionPortBox"));
        }

        [Fact]
        public void Default_ResetsValuesWithoutSaving()
        {
            session.Find("DefaultButton").AsButton().Invoke();

            Assert.True(AppSession.WaitUntil(() => TextOf("WindowSizeBox") == "800,450"));
            Assert.False(CheckedOf("FullScreenCheck"));
            Assert.Equal("IPAGothic", TextOf("FontNameBox"));
            Assert.Equal("30", TextOf("FontSizeBox"));
            SelectTab("ExpansionTab");
            Assert.False(CheckedOf("RunExpansionCheck"));
            Assert.Equal(dummyPpdIni, File.ReadAllText(PpdIniPath));
        }

        [Fact]
        public void DetailSettings_CanBeChangedAndReset()
        {
            SelectTab("DetailTab");
            var combo = session.Find("SleepTimeCombo").AsComboBox();
            combo.Select(3);
            Assert.True(AppSession.WaitUntil(() => combo.SelectedItem?.Text == "3"));
            var textBoxDisabled = session.Find("TextBoxDisabledCheck").AsCheckBox();
            textBoxDisabled.IsChecked = false;

            session.Find("DefaultButton").AsButton().Invoke();

            Assert.True(AppSession.WaitUntil(() => combo.SelectedItem?.Text == "0"));
            Assert.False(textBoxDisabled.IsChecked);
            Assert.Equal(dummyPpdIni, File.ReadAllText(PpdIniPath));
        }

        [Fact]
        public void Cancel_ClosesWithoutChangingPpdIni()
        {
            session.Find("WindowSizeBox").AsTextBox().Text = "1920,1080";
            session.Find("CancelButton").AsButton().Invoke();

            Assert.True(session.WaitForExit());
            Assert.Equal(dummyPpdIni, File.ReadAllText(PpdIniPath));
        }

        [Fact]
        public void Language_SwitchToEnglish_ChangesTexts()
        {
            var menu = session.FindMenuItem("LanguageMenu");
            menu.Expand();
            menu.Items.First(i => i.Name == "English").Invoke();

            Assert.True(AppSession.WaitUntil(() => session.Find("LanguageMenu").Name == "Language"));
            Assert.Equal("Apply default", session.Find("DefaultButton").Name);
            Assert.Equal("General Setting", session.Find("GeneralTab").Name);
        }
    }
}
