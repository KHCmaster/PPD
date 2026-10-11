using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KeyConfiger.Models;
using PPDConfiguration;
using PPDFramework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace KeyConfiger.ViewModels
{
    /// <summary>
    /// 画面に頼む操作です。ウィンドウが実装します。
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// 新しい設定の名前を入力させます。やめたときは null。
        /// </summary>
        string AskNewConfigName();

        void ShowMessage(string text);

        bool Confirm(string text, string caption);

        void Close();
    }

    /// <summary>
    /// 一覧の 1 行（ボタン 1 つ分）です。
    /// </summary>
    public partial class KeyRowViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial string Header { get; set; } = "";

        [ObservableProperty]
        public partial string Key { get; set; } = "0";

        [ObservableProperty]
        public partial string Button { get; set; } = "0";
    }

    /// <summary>
    /// ボタンに割り当てるキーボードのキーとゲームパッドのボタンを設定します。
    /// </summary>
    /// <remarks>
    /// 行を選んで入力待ちにすると、次に押されたキーかボタンをその行に割り当てる（入力はウィンドウが定期的に <see cref="PollInput"/> で調べさせる）。
    /// </remarks>
    public partial class MainWindowViewModel : ObservableObject
    {
        static readonly string[] headerKeys = ["Square", "Cross", "Circle", "Triangle", "Left", "Down", "Right", "Up", "R", "L", "Start", "Home"];

        readonly IDialogService dialogService;
        readonly string keyConfigPath;
        readonly string settingPath;
        IInputDevice input;
        string langFileISO = "";

        public LanguageTexts Texts { get; } = new();

        public ObservableCollection<MenuOption<string>> Languages { get; } = [];

        public ObservableCollection<MenuOption<KeyConfig>> Configs { get; } = [];

        public ObservableCollection<MenuOption<int>> Controllers { get; } = [];

        public ObservableCollection<KeyRowViewModel> Rows { get; } = [];

        [ObservableProperty]
        public partial int SelectedIndex { get; set; } = -1;

        [ObservableProperty]
        public partial bool IsWaitingInput { get; set; }

        [ObservableProperty]
        public partial MenuOption<KeyConfig> CurrentConfig { get; set; }

        /// <param name="keyConfigPath">キーの設定のファイル（keyconfig.ini）。</param>
        /// <param name="settingPath">このアプリの設定のファイル（言語を保存する）。</param>
        public MainWindowViewModel(IDialogService dialogService, string keyConfigPath, string settingPath)
        {
            this.dialogService = dialogService;
            this.keyConfigPath = keyConfigPath;
            this.settingPath = settingPath;
            for (int i = 0; i < ButtonUtility.Array.Length; i++)
            {
                Rows.Add(new KeyRowViewModel());
            }
            foreach (var config in KeyConfigStore.Load(keyConfigPath))
            {
                Configs.Add(new MenuOption<KeyConfig>(config.Name, config));
            }
            SelectConfig(Configs[0]);
            // 以前と同じく、キーの設定のファイルがあるときだけ、保存した言語を使う
            if (File.Exists(keyConfigPath) && File.Exists(settingPath))
            {
                langFileISO = new SettingReader(File.ReadAllText(settingPath))["Language"];
            }
            LoadLanguages();
            ChangeLanguage(langFileISO);
        }

        private void LoadLanguages()
        {
            const string componentName = "KeyConfiger";
            if (!Directory.Exists("Lang"))
            {
                return;
            }
            foreach (string fileName in Directory.GetFiles("Lang", String.Format("lang_{0}_*.ini", componentName)))
            {
                var m = LangFileRegex().Match(Path.GetFileName(fileName));
                if (!m.Success || !Regex.IsMatch(Path.GetFileName(fileName), String.Format("lang_{0}_[0-9a-zA-Z]+\\.ini", componentName)))
                {
                    continue;
                }
                var displayName = new SettingReader(File.ReadAllText(fileName))["DisplayName"];
                Languages.Add(new MenuOption<string>(displayName, m.Groups["ISO"].Value));
            }
        }

        [RelayCommand]
        private void SelectLanguage(MenuOption<string> option)
        {
            ChangeLanguage(option.Value);
        }

        private void ChangeLanguage(string iso)
        {
            langFileISO = iso;
            Utility.ChangeLanguage(iso);
            foreach (var language in Languages)
            {
                language.IsChecked = language.Value.Equals(iso, StringComparison.CurrentCultureIgnoreCase);
            }
            for (int i = 0; i < Math.Min(headerKeys.Length, Rows.Count); i++)
            {
                Rows[i].Header = Utility.Language[headerKeys[i]];
            }
            Texts.Refresh();
        }

        [RelayCommand]
        private void SelectConfig(MenuOption<KeyConfig> option)
        {
            CurrentConfig = option;
            foreach (var config in Configs)
            {
                config.IsChecked = config == option;
            }
            for (int i = 0; i < ButtonUtility.Array.Length; i++)
            {
                Rows[i].Key = option.Value.GetKeyMap(ButtonUtility.Array[i]).ToString();
                Rows[i].Button = option.Value.GetButtonMap(ButtonUtility.Array[i]).ToString();
            }
        }

        [RelayCommand]
        private void CreateConfig()
        {
            var name = dialogService.AskNewConfigName();
            if (name == null)
            {
                return;
            }
            var keyConfig = CurrentConfig.Value.Clone();
            keyConfig.Name = name;
            var option = new MenuOption<KeyConfig>(name, keyConfig);
            Configs.Add(option);
            SelectConfig(option);
        }

        [RelayCommand]
        private void DeleteConfig()
        {
            var index = Configs.IndexOf(CurrentConfig);
            if (index == 0)
            {
                dialogService.ShowMessage(Utility.Language["DeletionError"]);
                return;
            }
            if (dialogService.Confirm(Utility.Language["ConfirmDeletion"], Utility.Language["ConfirmDeletionCaption"]))
            {
                Configs.RemoveAt(index);
                SelectConfig(Configs[Math.Min(index, Configs.Count - 1)]);
            }
        }

        /// <summary>
        /// 入力を使えるようにします（ウィンドウができてから呼ぶ）。
        /// </summary>
        public void AttachInput(IInputDevice inputDevice)
        {
            input = inputDevice;
            input.Load();
            UpdateControllers();
        }

        [RelayCommand]
        private void ReloadControllers()
        {
            input?.Load();
            UpdateControllers();
        }

        private void UpdateControllers()
        {
            Controllers.Clear();
            if (input == null)
            {
                return;
            }
            var names = input.JoyStickNames;
            for (int i = 0; i < names.Length; i++)
            {
                Controllers.Add(new MenuOption<int>(names[i], i));
            }
            if (Controllers.Count > 0)
            {
                Controllers[0].IsChecked = true;
            }
        }

        [RelayCommand]
        private void SelectController(MenuOption<int> option)
        {
            input?.CurrentJoyStickIndex = option.Value;
            foreach (var controller in Controllers)
            {
                controller.IsChecked = controller == option;
            }
        }

        [RelayCommand]
        private void WaitForInput()
        {
            IsWaitingInput = true;
        }

        /// <summary>
        /// 押されたキーやボタンを調べて、入力待ちなら選んでいる行に割り当てます。
        /// </summary>
        public void PollInput()
        {
            if (input == null)
            {
                return;
            }
            if (input.TryGetPressedKey(out int key))
            {
                Assign(key, true);
            }
            if (input.TryGetPressedButton(out int button))
            {
                Assign(button, false);
            }
        }

        private void Assign(int value, bool keyboard)
        {
            if (!IsWaitingInput || SelectedIndex < 0)
            {
                return;
            }
            var buttonType = ButtonUtility.Array[SelectedIndex];
            if (keyboard)
            {
                Rows[SelectedIndex].Key = value.ToString();
                CurrentConfig.Value.SetKeyMap(buttonType, value);
            }
            else
            {
                Rows[SelectedIndex].Button = value.ToString();
                CurrentConfig.Value.SetButtonMap(buttonType, value);
            }
            IsWaitingInput = false;
        }

        [RelayCommand]
        private void Ok()
        {
            KeyConfigStore.Save(keyConfigPath, Configs.Select(c => c.Value));
            var setting = new SettingReader(File.Exists(settingPath) ? File.ReadAllText(settingPath) : "");
            setting.ReplaceOrAdd("Language", langFileISO);
            using (SettingWriter sw = new(settingPath, false))
            {
                foreach (KeyValuePair<string, string> kvp in setting.Dictionary)
                {
                    sw.Write(kvp.Key, kvp.Value);
                }
            }
            dialogService.Close();
        }

        [RelayCommand]
        private void Cancel()
        {
            dialogService.Close();
        }

        [GeneratedRegex("^lang_\\w+_(?<ISO>\\w+).ini$")]
        private static partial Regex LangFileRegex();
    }
}
