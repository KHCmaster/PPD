using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace UITests.Bmstoppd
{
    /// <summary>
    /// BMSTOPPD（BMS や DTX の譜面を PPD の譜面に変換するツール）の画面のテストです。
    /// </summary>
    public sealed class BmstoppdTests : IDisposable
    {
        readonly AppSession session = AppSession.Start(Path.Combine("BMSTOPPD", "BMSTOPPD"), "BMSTOPPD.exe");

        public void Dispose()
        {
            session.Dispose();
        }

        private AutomationElement Find(string automationId)
        {
            return session.MainWindow.FindFirstDescendant(cf => cf.ByAutomationId(automationId))
                ?? throw new InvalidOperationException(automationId + " が見つかりません。");
        }

        /// <summary>
        /// メッセージボックスを待って、表示している文字を返し、閉じます。
        /// </summary>
        private string ReadAndCloseMessage()
        {
            return MessageBoxes.ReadAndClose(session.WaitForModal());
        }

        [Fact]
        public void MainWindow_ShowsInputsAndButtons()
        {
            Assert.Equal("BMSTOPPD", session.MainWindow.Title);
            Assert.Equal("", Find("FileNameBox").AsTextBox().Text);
            Assert.Equal("convert", Find("ConvertButton").Name);
            Assert.Equal("setting", Find("SettingButton").Name);
            Assert.False(Find("DtxCheckBox").AsCheckBox().IsChecked);
        }

        [Fact]
        public void Convert_WithoutFile_ShowsMessage()
        {
            Find("FileNameBox").AsTextBox().Text = Path.Combine(session.WorkDirectory, "not-exist.bms");
            AppSession.InvokeAsync(Find("ConvertButton").AsButton());

            Assert.Contains("file not exist", ReadAndCloseMessage());
        }

        [Fact]
        public void Convert_SampleBms_WritesPpdNextToIt()
        {
            var bms = Path.Combine(session.WorkDirectory, "sample.bms");
            File.Copy(Path.Combine(AppContext.BaseDirectory, "Bmstoppd", "TestData", "sample.bms"), bms);
            Find("FileNameBox").AsTextBox().Text = bms;
            Find("TimeOffsetBox").AsTextBox().Text = "0.5";
            AppSession.InvokeAsync(Find("ConvertButton").AsButton());

            var output = Path.Combine(session.WorkDirectory, "trans.ppd");
            Assert.Contains(output, ReadAndCloseMessage());
            Assert.True(new FileInfo(output).Length > 0);
        }

        /// <summary>
        /// テストのデータの譜面を作業のフォルダにコピーして変換し、できた譜面の中身を返します。
        /// </summary>
        private byte[] Convert(string sampleName, string timeOffset)
        {
            var source = Path.Combine(session.WorkDirectory, sampleName);
            File.Copy(Path.Combine(AppContext.BaseDirectory, "Bmstoppd", "TestData", sampleName), source, true);
            Find("FileNameBox").AsTextBox().Text = source;
            Find("TimeOffsetBox").AsTextBox().Text = timeOffset;
            AppSession.InvokeAsync(Find("ConvertButton").AsButton());
            var output = Path.Combine(session.WorkDirectory, "trans.ppd");
            Assert.Contains(output, ReadAndCloseMessage());
            return File.ReadAllBytes(output);
        }

        [Fact]
        public void Convert_TimeOffsetChangesOutput()
        {
            var withoutOffset = Convert("sample.bms", "0");
            var withOffset = Convert("sample.bms", "1.5");

            Assert.NotEmpty(withoutOffset);
            Assert.NotEqual(withoutOffset, withOffset);
        }

        [Fact]
        public void Convert_DtxFormat_WritesPpd()
        {
            Find("DtxCheckBox").AsCheckBox().IsChecked = true;

            Assert.NotEmpty(Convert("sample.dtx", "0"));
        }

        [Fact]
        public void SettingWindow_ListsAllButtons()
        {
            Find("SettingButton").AsButton().Invoke();
            var settingWindow = session.WaitForWindow("setting");
            var grid = settingWindow.FindFirstDescendant(cf => cf.ByAutomationId("SettingGrid")).AsGrid();

            Assert.Equal(10, grid.RowCount);
            var headers = grid.Rows.Select(row => row.FindFirstDescendant(cf => cf.ByControlType(ControlType.Text))?.Name).ToArray();
            Assert.Equal(["square", "cross", "circle", "triangle", "left", "down", "right", "up", "R", "L"], headers);
        }

        [Fact]
        public void SettingWindow_KeepsEditedValueAfterReopen()
        {
            var settingButton = Find("SettingButton").AsButton();
            settingButton.Invoke();
            var settingWindow = session.WaitForWindow("setting");
            var cell = settingWindow.FindFirstDescendant(cf => cf.ByAutomationId("SettingGrid")).AsGrid().GetRowByIndex(0).Cells[0];
            cell.Patterns.Value.Pattern.SetValue("123");

            // 閉じても隠すだけなので、もう一度開くと同じ値が出る
            settingWindow.Close();
            Assert.True(AppSession.WaitUntil(() => settingWindow.IsOffscreen || !settingWindow.IsAvailable));
            settingButton.Invoke();
            settingWindow = session.WaitForWindow("setting");
            var reopened = settingWindow.FindFirstDescendant(cf => cf.ByAutomationId("SettingGrid")).AsGrid().GetRowByIndex(0).Cells[0];

            Assert.Equal("123", reopened.Value);
        }
    }
}
