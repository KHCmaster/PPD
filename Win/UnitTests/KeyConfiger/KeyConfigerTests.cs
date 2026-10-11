using KeyConfiger;
using KeyConfiger.Models;
using KeyConfiger.ViewModels;
using PPDFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace UnitTests.KeyConfiger
{
    public class KeyConfigStoreTests
    {
        [Fact]
        public void Load_MissingFile_ReturnsDefault()
        {
            using var folder = new TempFolder();

            var configs = KeyConfigStore.Load(folder.Combine("keyconfig.ini"));

            Assert.Equal("default", configs.Single().Name);
        }

        [Fact]
        public void SaveThenLoad_KeepsAllConfigs()
        {
            using var folder = new TempFolder();
            var first = new KeyConfig();
            first.SetKeyMap(ButtonType.Circle, 45);
            first.SetButtonMap(ButtonType.Circle, 3);
            var second = new KeyConfig { Name = "pad" };
            second.SetButtonMap(ButtonType.Start, 9);

            KeyConfigStore.Save(folder.Combine("keyconfig.ini"), [first, second]);
            var loaded = KeyConfigStore.Load(folder.Combine("keyconfig.ini"));

            Assert.Equal(["default", "pad"], loaded.Select(c => c.Name));
            Assert.Equal(45, loaded[0].GetKeyMap(ButtonType.Circle));
            Assert.Equal(3, loaded[0].GetButtonMap(ButtonType.Circle));
            Assert.Equal(9, loaded[1].GetButtonMap(ButtonType.Start));
            Assert.Equal(0, loaded[1].GetKeyMap(ButtonType.Start));
        }

        [Fact]
        public void Load_OldFormat_ReadsKeyAndButtonPerLine()
        {
            using var folder = new TempFolder();
            // 以前の形式は、ボタンの順に 1 行に「キー:ボタン」
            var path = folder.Write("keyconfig.ini", "10:1\r\n20:2\r\n30:3\r\n");

            var config = KeyConfigStore.Load(path).Single();

            Assert.Equal(10, config.GetKeyMap(ButtonUtility.Array[0]));
            Assert.Equal(2, config.GetButtonMap(ButtonUtility.Array[1]));
            Assert.Equal(30, config.GetKeyMap(ButtonUtility.Array[2]));
            Assert.Equal(0, config.GetKeyMap(ButtonUtility.Array[3]));
        }

        [Fact]
        public void Clone_CopiesMapsButNotName()
        {
            var config = new KeyConfig { Name = "mine" };
            config.SetKeyMap(ButtonType.Up, 200);

            var clone = config.Clone();
            clone.SetKeyMap(ButtonType.Up, 1);

            Assert.Equal("default", clone.Name);
            Assert.Equal(200, config.GetKeyMap(ButtonType.Up));
        }
    }

    public sealed class KeyConfigerViewModelTests : IDisposable
    {
        sealed class FakeDialogService : IDialogService
        {
            public string NewConfigName { get; set; }
            public bool ConfirmAnswer { get; set; } = true;
            public List<string> Messages { get; } = [];
            public int ConfirmCount { get; private set; }
            public bool Closed { get; private set; }

            public string AskNewConfigName() => NewConfigName;

            public void ShowMessage(string text) => Messages.Add(text);

            public bool Confirm(string text, string caption)
            {
                ConfirmCount++;
                return ConfirmAnswer;
            }

            public void Close() => Closed = true;
        }

        sealed class FakeInputDevice : IInputDevice
        {
            public string[] JoyStickNames { get; set; } = ["Pad A", "Pad B"];
            public int CurrentJoyStickIndex { get; set; }
            public int LoadCount { get; private set; }
            public int? PressedKey { get; set; }
            public int? PressedButton { get; set; }

            public void Load() => LoadCount++;

            public bool TryGetPressedKey(out int key)
            {
                key = PressedKey ?? 0;
                return PressedKey.HasValue;
            }

            public bool TryGetPressedButton(out int button)
            {
                button = PressedButton ?? 0;
                return PressedButton.HasValue;
            }
        }

        readonly TempFolder folder = new(setCurrentDirectory: true);
        readonly FakeDialogService dialog = new();

        public KeyConfigerViewModelTests()
        {
            AppFiles.CopyTo("KeyConfiger", folder);
        }

        public void Dispose()
        {
            folder.Dispose();
        }

        private MainWindowViewModel Create()
        {
            return new MainWindowViewModel(dialog, "keyconfig.ini", "KeyConfiger.ini");
        }

        [Fact]
        public void Startup_WithoutSettings_ShowsDefaultConfigInEnglish()
        {
            var viewModel = Create();

            Assert.Equal("default", viewModel.CurrentConfig.Name);
            Assert.True(viewModel.CurrentConfig.IsChecked);
            Assert.Equal(12, viewModel.Rows.Count);
            Assert.Equal("Square", viewModel.Rows[0].Header);
            Assert.Equal(6, viewModel.Languages.Count);
            Assert.False(viewModel.IsWaitingInput);
        }

        [Fact]
        public void SelectLanguage_ChangesHeadersAndCheck()
        {
            var viewModel = Create();
            var japanese = viewModel.Languages.Single(l => l.Value == "jp");

            viewModel.SelectLanguageCommand.Execute(japanese);

            Assert.True(japanese.IsChecked);
            Assert.Single(viewModel.Languages, l => l.IsChecked);
            Assert.NotEqual("Square", viewModel.Rows[0].Header);
            Assert.Equal("言語", viewModel.Texts["Language"]);
        }

        [Fact]
        public void Startup_WithSavedLanguage_UsesIt()
        {
            File.WriteAllText("keyconfig.ini", "1:1\r\n");
            File.WriteAllText("KeyConfiger.ini", "[Language]jp\r\n");

            var viewModel = Create();

            Assert.True(viewModel.Languages.Single(l => l.Value == "jp").IsChecked);
            Assert.Equal(1, int.Parse(viewModel.Rows[0].Key));
        }

        [Fact]
        public void CreateConfig_AddsCopyAndSelectsIt()
        {
            var viewModel = Create();
            viewModel.Rows[0].Key = "ignored";
            viewModel.CurrentConfig.Value.SetKeyMap(ButtonUtility.Array[0], 77);
            dialog.NewConfigName = "mine";

            viewModel.CreateConfigCommand.Execute(null);

            Assert.Equal(["default", "mine"], viewModel.Configs.Select(c => c.Name));
            Assert.Equal("mine", viewModel.CurrentConfig.Name);
            Assert.Equal("mine", viewModel.CurrentConfig.Value.Name);
            Assert.Equal("77", viewModel.Rows[0].Key);
            Assert.False(viewModel.Configs[0].IsChecked);
        }

        [Fact]
        public void CreateConfig_Cancelled_DoesNothing()
        {
            var viewModel = Create();

            viewModel.CreateConfigCommand.Execute(null);

            Assert.Single(viewModel.Configs);
        }

        [Fact]
        public void DeleteConfig_Default_ShowsError()
        {
            var viewModel = Create();

            viewModel.DeleteConfigCommand.Execute(null);

            Assert.Single(dialog.Messages);
            Assert.Single(viewModel.Configs);
            Assert.Equal(0, dialog.ConfirmCount);
        }

        [Fact]
        public void DeleteConfig_Confirmed_RemovesAndSelectsNext()
        {
            var viewModel = Create();
            dialog.NewConfigName = "a";
            viewModel.CreateConfigCommand.Execute(null);
            dialog.NewConfigName = "b";
            viewModel.CreateConfigCommand.Execute(null);
            viewModel.SelectConfigCommand.Execute(viewModel.Configs[1]);

            viewModel.DeleteConfigCommand.Execute(null);

            Assert.Equal(["default", "b"], viewModel.Configs.Select(c => c.Name));
            Assert.Equal("b", viewModel.CurrentConfig.Name);
        }

        [Fact]
        public void DeleteConfig_LastOne_SelectsPrevious()
        {
            var viewModel = Create();
            dialog.NewConfigName = "a";
            viewModel.CreateConfigCommand.Execute(null);

            viewModel.DeleteConfigCommand.Execute(null);

            Assert.Equal("default", viewModel.CurrentConfig.Name);
        }

        [Fact]
        public void DeleteConfig_Declined_Keeps()
        {
            var viewModel = Create();
            dialog.NewConfigName = "a";
            viewModel.CreateConfigCommand.Execute(null);
            dialog.ConfirmAnswer = false;

            viewModel.DeleteConfigCommand.Execute(null);

            Assert.Equal(2, viewModel.Configs.Count);
        }

        [Fact]
        public void AttachInput_ListsControllersAndChecksFirst()
        {
            var viewModel = Create();
            var input = new FakeInputDevice();

            viewModel.AttachInput(input);

            Assert.Equal(["Pad A", "Pad B"], viewModel.Controllers.Select(c => c.Name));
            Assert.True(viewModel.Controllers[0].IsChecked);
            Assert.Equal(1, input.LoadCount);
        }

        [Fact]
        public void SelectController_ChangesDevice()
        {
            var viewModel = Create();
            var input = new FakeInputDevice();
            viewModel.AttachInput(input);

            viewModel.SelectControllerCommand.Execute(viewModel.Controllers[1]);

            Assert.Equal(1, input.CurrentJoyStickIndex);
            Assert.False(viewModel.Controllers[0].IsChecked);
            Assert.True(viewModel.Controllers[1].IsChecked);
        }

        [Fact]
        public void ReloadControllers_LoadsAgain()
        {
            var viewModel = Create();
            var input = new FakeInputDevice();
            viewModel.AttachInput(input);
            input.JoyStickNames = [];

            viewModel.ReloadControllersCommand.Execute(null);

            Assert.Equal(2, input.LoadCount);
            Assert.Empty(viewModel.Controllers);
        }

        [Fact]
        public void PollInput_WhileWaiting_AssignsPressedKeyToSelectedRow()
        {
            var viewModel = Create();
            var input = new FakeInputDevice();
            viewModel.AttachInput(input);
            viewModel.SelectedIndex = 2;
            viewModel.WaitForInputCommand.Execute(null);
            input.PressedKey = 30;

            viewModel.PollInput();

            Assert.Equal("30", viewModel.Rows[2].Key);
            Assert.Equal(30, viewModel.CurrentConfig.Value.GetKeyMap(ButtonUtility.Array[2]));
            Assert.False(viewModel.IsWaitingInput);
        }

        [Fact]
        public void PollInput_WhileWaiting_AssignsPressedButton()
        {
            var viewModel = Create();
            var input = new FakeInputDevice();
            viewModel.AttachInput(input);
            viewModel.SelectedIndex = 5;
            viewModel.WaitForInputCommand.Execute(null);
            input.PressedButton = 7;

            viewModel.PollInput();

            Assert.Equal("7", viewModel.Rows[5].Button);
            Assert.Equal(7, viewModel.CurrentConfig.Value.GetButtonMap(ButtonUtility.Array[5]));
        }

        [Fact]
        public void PollInput_NotWaitingOrNoRow_IgnoresInput()
        {
            var viewModel = Create();
            var input = new FakeInputDevice { PressedKey = 30 };
            viewModel.AttachInput(input);
            viewModel.SelectedIndex = 2;

            viewModel.PollInput();
            Assert.Equal("0", viewModel.Rows[2].Key);

            viewModel.SelectedIndex = -1;
            viewModel.WaitForInputCommand.Execute(null);
            viewModel.PollInput();
            Assert.True(viewModel.IsWaitingInput);
        }

        [Fact]
        public void PollInput_BeforeAttach_DoesNothing()
        {
            var viewModel = Create();

            viewModel.PollInput();
            viewModel.ReloadControllersCommand.Execute(null);

            Assert.Empty(viewModel.Controllers);
        }

        [Fact]
        public void Ok_SavesConfigsAndLanguageThenCloses()
        {
            var viewModel = Create();
            dialog.NewConfigName = "mine";
            viewModel.CreateConfigCommand.Execute(null);
            viewModel.SelectLanguageCommand.Execute(viewModel.Languages.Single(l => l.Value == "jp"));

            viewModel.OkCommand.Execute(null);

            Assert.True(dialog.Closed);
            Assert.Equal(["default", "mine"], KeyConfigStore.Load("keyconfig.ini").Select(c => c.Name));
            Assert.Contains("[Language]jp", File.ReadAllText("KeyConfiger.ini"));
        }

        [Fact]
        public void Cancel_ClosesWithoutSaving()
        {
            var viewModel = Create();

            viewModel.CancelCommand.Execute(null);

            Assert.True(dialog.Closed);
            Assert.False(File.Exists("keyconfig.ini"));
        }
    }
}
