using BMSTOPPD.Models;
using BMSTOPPD.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace UnitTests.Bmstoppd
{
    public class MarkSettingTests
    {
        [Fact]
        public void Getters_ParseText()
        {
            var setting = new MarkSetting { X = "12.5", Y = "-3", Angle = "180" };

            Assert.Equal(12.5f, setting.GetX());
            Assert.Equal(-3f, setting.GetY());
            Assert.Equal((float)Math.PI, setting.GetAngleInRadians(), 5);
        }

        [Fact]
        public void Store_CreatesDefaultsWhenMissing()
        {
            using var folder = new TempFolder(setCurrentDirectory: true);

            var settings = MarkSettingStore.Load();

            Assert.Equal(["square", "cross", "circle", "triangle", "left", "down", "right", "up", "R", "L"], settings.Select(s => s.Name));
            Assert.All(settings, s => Assert.Equal(("400", "255", "0"), (s.X, s.Y, s.Angle)));
            Assert.True(File.Exists(folder.Combine("setting.ini")));
        }

        [Fact]
        public void Store_SaveThenLoad_KeepsValues()
        {
            using var folder = new TempFolder(setCurrentDirectory: true);
            var settings = MarkSettingStore.Load();
            settings[3].X = "1";
            settings[3].Y = "2";
            settings[3].Angle = "45";

            MarkSettingStore.Save(settings);
            var loaded = MarkSettingStore.Load();

            Assert.Equal(("1", "2", "45"), (loaded[3].X, loaded[3].Y, loaded[3].Angle));
            Assert.Equal("400", loaded[0].X);
        }
    }

    public class BmstoppdMainWindowViewModelTests
    {
        sealed class FakeDialogService : IDialogService
        {
            public string FileToOpen { get; set; }
            public string LastFilter { get; private set; }
            public List<string> Messages { get; } = [];
            public int SettingWindowCount { get; private set; }

            public string OpenFile(string filter)
            {
                LastFilter = filter;
                return FileToOpen;
            }

            public void ShowMessage(string text)
            {
                Messages.Add(text);
            }

            public void ShowSettingWindow()
            {
                SettingWindowCount++;
            }
        }

        static BmstoppdMainWindowViewModelTests()
        {
            // 譜面は Shift_JIS で読む（アプリの起動のときと同じ）
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        private static List<MarkSetting> Settings()
        {
            return [.. Enumerable.Range(0, 10).Select(i => new MarkSetting { Name = "b" + i, X = "0", Y = "0", Angle = "0" })];
        }

        [Fact]
        public void Open_SetsSelectedFile()
        {
            var dialog = new FakeDialogService { FileToOpen = @"C:\songs\a.bms" };
            var viewModel = new MainWindowViewModel(dialog, Settings());

            viewModel.OpenCommand.Execute(null);

            Assert.Equal(@"C:\songs\a.bms", viewModel.FileName);
            Assert.Contains("*.bms", dialog.LastFilter);
        }

        [Fact]
        public void Open_Cancelled_KeepsFile()
        {
            var dialog = new FakeDialogService();
            var viewModel = new MainWindowViewModel(dialog, Settings()) { FileName = "before" };

            viewModel.OpenCommand.Execute(null);

            Assert.Equal("before", viewModel.FileName);
        }

        [Fact]
        public void Convert_MissingFile_ShowsMessage()
        {
            var dialog = new FakeDialogService();
            var viewModel = new MainWindowViewModel(dialog, Settings()) { FileName = @"Z:\not\exist.bms" };

            viewModel.ConvertCommand.Execute(null);

            Assert.Equal(["file not exist"], dialog.Messages);
        }

        [Fact]
        public void Convert_WritesFileAndReportsPath()
        {
            using var folder = new TempFolder();
            var dialog = new FakeDialogService();
            var viewModel = new MainWindowViewModel(dialog, Settings())
            {
                FileName = folder.Write("song.bms", "#BPM 120\n#00011:01\n"),
                TimeOffset = "0.25",
            };

            viewModel.ConvertCommand.Execute(null);

            var output = folder.Combine("trans.ppd");
            Assert.Equal([output + "\nを生成しました"], dialog.Messages);
            Assert.Equal(0.25f, BitConverter.ToSingle(File.ReadAllBytes(output), 3));
        }

        [Fact]
        public void Convert_InvalidChart_ShowsError()
        {
            using var folder = new TempFolder();
            var dialog = new FakeDialogService();
            var viewModel = new MainWindowViewModel(dialog, Settings())
            {
                FileName = folder.Write("song.dtx", "#BPM: 120\n#0000C: 01\n"),
                IsDtx = true,
            };

            viewModel.ConvertCommand.Execute(null);

            Assert.Equal(["error"], dialog.Messages);
        }

        [Fact]
        public void ShowSetting_OpensSettingWindow()
        {
            var dialog = new FakeDialogService();
            var viewModel = new MainWindowViewModel(dialog, Settings());

            viewModel.ShowSettingCommand.Execute(null);

            Assert.Equal(1, dialog.SettingWindowCount);
        }
    }
}
