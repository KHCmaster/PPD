using PPDConfiguration;
using PPDFramework;
using PPDInput;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using DIKey = Vortice.DirectInput.Key;

namespace KeyConfiger
{
    /// <summary>
    /// ボタンに割り当てるキーボードのキーとゲームパッドのボタンを設定するウィンドウです。
    /// </summary>
    /// <remarks>
    /// 入力は DirectInput（PPDInput）でタイマーごとに調べる。行を選んで「選択行を変更する」を押す（または行をクリックする）と入力待ちになり、
    /// 次に押されたキーかボタンをその行に割り当てる。
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "input はウィンドウを閉じたとき（Closed）に破棄する")]
    public partial class MainWindow : Window
    {
        const string iniFileName = "KeyConfiger.ini";
        const string keyConfigFileName = "keyconfig.ini";
        const int ConfigMenuStartIndex = 3;
        const int ControllerMenuStartIndex = 2;

        readonly List<KeyConfig> keyConfigs = [];
        readonly List<KeyRow> rows = [];
        readonly DispatcherTimer inputTimer;
        Input input;
        bool waitinginput;
        int currentConfigIndex;
        string langFileISO = "";

        string deletionError = "default の設定を削除することはできません";
        string confirmDeletion = "現在の設定を削除してもよろしいですか？(元には戻せません)";
        string confirmDeletionCaption = "削除確認";

        public MainWindow()
        {
            InitializeComponent();
            for (int i = 0; i < ButtonUtility.Array.Length; i++)
            {
                rows.Add(new KeyRow());
            }
            keyGrid.ItemsSource = rows;
            ReadKeySetting();
            if (File.Exists(keyConfigFileName))
            {
                CheckSetting();
            }
            CheckLangFiles();
            SetLanguage(langFileISO);
            inputTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(16), DispatcherPriority.Input, InputTimer_Tick, Dispatcher);
            inputTimer.Stop();
        }

        public KeyConfig CurrentConfig
        {
            get
            {
                return keyConfigs[currentConfigIndex];
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            input = new Input(new WindowInteropHelper(this).Handle);
            input.Load();
            ReloadController();
            inputTimer.Start();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            inputTimer.Stop();
            input?.Dispose();
            input = null;
        }

        private void InputTimer_Tick(object sender, EventArgs e)
        {
            if (input == null)
            {
                return;
            }
            if (input.GetPressedKey(out DIKey k))
            {
                SetNumber((int)k, true);
            }
            if (input.GetPressedButton(out int a))
            {
                SetNumber(a, false);
            }
        }

        private void ReadKeySetting()
        {
            if (File.Exists(keyConfigFileName))
            {
                try
                {
                    ReadKeySettingFromXml();
                }
                catch
                {
                    ReadKeySettingFromIni();
                }
            }
            else
            {
                AddKeyConfig(new KeyConfig());
            }
            ApplyConfigToDataGrid(keyConfigs[0]);
            ((MenuItem)settingMenuItem.Items[ConfigMenuStartIndex]).IsChecked = true;
        }

        private void ReadKeySettingFromXml()
        {
            using var reader = XmlReader.Create(keyConfigFileName, new XmlReaderSettings());
            while (reader.Read())
            {
                if (reader.IsStartElement())
                {
                    switch (reader.LocalName)
                    {
                        case "KeyConfigs":
                            ClearKeyConfigs();
                            break;
                        case "KeyConfig":
                            var keyConfig = new KeyConfig
                            {
                                Name = reader.GetAttribute("Name")
                            };
                            AddKeyConfig(keyConfig);
                            ReadKeySetting(reader.ReadSubtree(), keyConfig);
                            break;
                    }
                }
            }

            if (keyConfigs.Count == 0)
            {
                AddKeyConfig(new KeyConfig());
            }
        }

        private void ClearKeyConfigs()
        {
            keyConfigs.Clear();
            while (settingMenuItem.Items.Count > ConfigMenuStartIndex)
            {
                settingMenuItem.Items.RemoveAt(settingMenuItem.Items.Count - 1);
            }
        }

        private void AddKeyConfig(KeyConfig keyConfig)
        {
            keyConfigs.Add(keyConfig);
            var menuItem = new MenuItem
            {
                Header = CreateMenuHeader(keyConfig.Name),
                ToolTip = keyConfig.Name,
                Tag = keyConfig
            };
            menuItem.Click += ConfigMenuItem_Click;
            settingMenuItem.Items.Add(menuItem);
        }

        // 名前の「_」がアクセスキーとして扱われないように、TextBlock で表示する
        private static TextBlock CreateMenuHeader(string text)
        {
            return new TextBlock { Text = text };
        }

        private static void ReadKeySetting(XmlReader reader, KeyConfig keyConfig)
        {
            while (reader.Read())
            {
                if (reader.IsStartElement() && reader.LocalName == "Config")
                {
                    int key = int.Parse(reader.GetAttribute("Key")), button = int.Parse(reader.GetAttribute("Button")),
                        index = FindIndex(reader.GetAttribute("Type"));
                    if (index >= 0)
                    {
                        keyConfig.SetKeyMap(ButtonUtility.Array[index], key);
                        keyConfig.SetButtonMap(ButtonUtility.Array[index], button);
                    }
                }
            }
            reader.Close();
        }

        private static int FindIndex(string value)
        {
            for (int i = 0; i < ButtonUtility.Array.Length; i++)
            {
                if (ButtonUtility.Array[i].ToString() == value)
                {
                    return i;
                }
            }
            return -1;
        }

        private void ReadKeySettingFromIni()
        {
            ClearKeyConfigs();
            var keyConfig = new KeyConfig();
            var s = File.ReadAllText(keyConfigFileName).Replace("\r\n", "\n").Replace("\r", "\n");
            var sp = s.Split('\n');
            for (int i = 0; i < Math.Min(sp.Length, ButtonUtility.Array.Length); i++)
            {
                var secondsp = sp[i].Split(':');
                if (secondsp.Length >= 2)
                {
                    keyConfig.SetKeyMap(ButtonUtility.Array[i], int.Parse(secondsp[0]));
                    keyConfig.SetButtonMap(ButtonUtility.Array[i], int.Parse(secondsp[1]));
                }
            }
            AddKeyConfig(keyConfig);
        }

        private void ApplyConfigToDataGrid(KeyConfig config)
        {
            for (int i = 0; i < ButtonUtility.Array.Length; i++)
            {
                rows[i].Key = config.GetKeyMap(ButtonUtility.Array[i]).ToString();
                rows[i].Button = config.GetButtonMap(ButtonUtility.Array[i]).ToString();
            }
        }

        private void CheckSetting()
        {
            if (File.Exists(iniFileName))
            {
                var setting = new SettingReader(File.ReadAllText(iniFileName));
                langFileISO = setting["Language"];
            }
        }

        private void CheckLangFiles()
        {
            if (!Directory.Exists("Lang"))
            {
                return;
            }
            var componentName = this.GetType().Assembly.GetName().Name;
            foreach (string fileName in Directory.GetFiles("Lang", String.Format("lang_{0}_*.ini", componentName)))
            {
                if (!Regex.IsMatch(Path.GetFileName(fileName), String.Format("lang_{0}_[0-9a-zA-Z]+\\.ini", componentName)))
                {
                    continue;
                }
                var setting = new SettingReader(File.ReadAllText(fileName));
                var menuItem = new MenuItem
                {
                    Header = CreateMenuHeader(setting["DisplayName"]),
                    Tag = fileName,
                    IsChecked = Path.GetFileName(fileName).Equals(String.Format("lang_{0}_{1}.ini", componentName, langFileISO), StringComparison.CurrentCultureIgnoreCase)
                };
                menuItem.Click += LanguageMenuItem_Click;
                languageMenuItem.Items.Add(menuItem);
            }
        }

        void LanguageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var m = LangFileRegex().Match(Path.GetFileName((string)menuItem.Tag));
            if (m.Success)
            {
                langFileISO = m.Groups["ISO"].Value;
            }
            SetLanguage(langFileISO);
            UncheckAllMenuItem(languageMenuItem, 0);
            menuItem.IsChecked = true;
        }

        private void SetLanguage(string langIso)
        {
            Utility.ChangeLanguage(langIso);
            languageMenuItem.Header = Utility.Language["Language"];
            string[] headerKeys = ["Square", "Cross", "Circle", "Triangle", "Left", "Down", "Right", "Up", "R", "L", "Start", "Home"];
            for (int i = 0; i < Math.Min(headerKeys.Length, rows.Count); i++)
            {
                rows[i].Header = Utility.Language[headerKeys[i]];
            }
            keyboardColumn.Header = Utility.Language["Keyboard"];
            gamepadColumn.Header = Utility.Language["Gamepad"];
            changeButton.Content = Utility.Language["Button1"];
            waitingText.Text = Utility.Language["Label1"];
            settingMenuItem.Header = Utility.Language["Setting"];
            createSettingMenuItem.Header = Utility.Language["CreateNewSetting"];
            deleteSettingMenuItem.Header = Utility.Language["DeleteCurrentSetting"];
            controllerMenuItem.Header = Utility.Language["Gamepad"];
            reloadControllerMenuItem.Header = Utility.Language["ResetControllerList"];
            deletionError = Utility.Language["DeletionError"];
            confirmDeletion = Utility.Language["ConfirmDeletion"];
            confirmDeletionCaption = Utility.Language["ConfirmDeletionCaption"];
        }

        private void SetNumber(int num, bool keyboard)
        {
            if (waitinginput && keyGrid.SelectedIndex >= 0)
            {
                int index = keyGrid.SelectedIndex;
                if (keyboard)
                {
                    rows[index].Key = num.ToString();
                    CurrentConfig.SetKeyMap(ButtonUtility.Array[index], num);
                }
                else
                {
                    rows[index].Button = num.ToString();
                    CurrentConfig.SetButtonMap(ButtonUtility.Array[index], num);
                }
                waitinginput = false;
                waitingText.Visibility = Visibility.Hidden;
            }
        }

        private void ChangeButton_Click(object sender, RoutedEventArgs e)
        {
            WaitForInput();
        }

        private void WaitForInput()
        {
            waitinginput = true;
            waitingText.Visibility = Visibility.Visible;
        }

        // 行をクリックしても入力待ちにする
        private void KeyGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            for (var element = e.OriginalSource as DependencyObject; element != null; element = VisualTreeHelper.GetParent(element))
            {
                if (element is DataGridRow)
                {
                    WaitForInput();
                    return;
                }
            }
        }

        // 入力待ちの間は、押したキーで選択行が動かないようにする
        private void KeyGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled |= waitinginput;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            using (var writer = XmlWriter.Create(keyConfigFileName, new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "   ",
                NewLineChars = Environment.NewLine
            }))
            {
                writer.WriteStartElement("root");
                writer.WriteStartElement("KeyConfigs");
                foreach (KeyConfig keyConfig in keyConfigs)
                {
                    writer.WriteStartElement("KeyConfig");
                    writer.WriteAttributeString("Name", keyConfig.Name);
                    WriteSetting(writer, keyConfig);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
            SaveSetting();
            Close();
        }

        private static void WriteSetting(XmlWriter writer, KeyConfig keyConfig)
        {
            foreach (ButtonType buttonType in ButtonUtility.Array)
            {
                writer.WriteStartElement("Config");
                writer.WriteAttributeString("Type", buttonType.ToString());
                writer.WriteAttributeString("Key", keyConfig.GetKeyMap(buttonType).ToString());
                writer.WriteAttributeString("Button", keyConfig.GetButtonMap(buttonType).ToString());
                writer.WriteEndElement();
            }
        }

        private void SaveSetting()
        {
            var setting = new SettingReader(File.Exists(iniFileName) ? File.ReadAllText(iniFileName) : "");
            setting.ReplaceOrAdd("Language", langFileISO);
            using SettingWriter sw = new(iniFileName, false);
            foreach (KeyValuePair<string, string> kvp in setting.Dictionary)
            {
                sw.Write(kvp.Key, kvp.Value);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CreateSettingMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var window = new NewConfigWindow { Owner = this };
            window.SetLang();
            if (window.ShowDialog() == true)
            {
                var keyConfig = CurrentConfig.Clone();
                keyConfig.Name = window.SettingName;
                AddKeyConfig(keyConfig);
                currentConfigIndex = keyConfigs.Count - 1;
                UncheckAllMenuItem(settingMenuItem, ConfigMenuStartIndex);
                ((MenuItem)settingMenuItem.Items[ConfigMenuStartIndex + currentConfigIndex]).IsChecked = true;
                ApplyConfigToDataGrid(CurrentConfig);
            }
        }

        void ConfigMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var index = keyConfigs.IndexOf((KeyConfig)menuItem.Tag);
            if (index >= 0)
            {
                currentConfigIndex = index;
                ApplyConfigToDataGrid(CurrentConfig);
            }
            UncheckAllMenuItem(settingMenuItem, ConfigMenuStartIndex);
            menuItem.IsChecked = true;
        }

        private static void UncheckAllMenuItem(MenuItem parent, int startIndex)
        {
            for (int i = startIndex; i < parent.Items.Count; i++)
            {
                if (parent.Items[i] is MenuItem item)
                {
                    item.IsChecked = false;
                }
            }
        }

        private void DeleteSettingMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (currentConfigIndex == 0)
            {
                MessageBox.Show(this, deletionError);
                return;
            }

            if (MessageBox.Show(this, confirmDeletion, confirmDeletionCaption, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                keyConfigs.RemoveAt(currentConfigIndex);
                settingMenuItem.Items.RemoveAt(ConfigMenuStartIndex + currentConfigIndex);
                if (currentConfigIndex >= keyConfigs.Count)
                {
                    currentConfigIndex--;
                }
                ApplyConfigToDataGrid(CurrentConfig);
                UncheckAllMenuItem(settingMenuItem, ConfigMenuStartIndex);
                ((MenuItem)settingMenuItem.Items[ConfigMenuStartIndex + currentConfigIndex]).IsChecked = true;
            }
        }

        private void ReloadControllerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            input.Load();
            ReloadController();
        }

        private void ReloadController()
        {
            while (controllerMenuItem.Items.Count > ControllerMenuStartIndex)
            {
                controllerMenuItem.Items.RemoveAt(controllerMenuItem.Items.Count - 1);
            }
            int iter = 0;
            foreach (string controllerName in input.JoyStickNames)
            {
                var menuItem = new MenuItem
                {
                    Header = CreateMenuHeader(controllerName),
                    ToolTip = controllerName,
                    Tag = iter
                };
                menuItem.Click += ControllerMenuItem_Click;
                controllerMenuItem.Items.Add(menuItem);
                iter++;
            }
            if (controllerMenuItem.Items.Count > ControllerMenuStartIndex)
            {
                ((MenuItem)controllerMenuItem.Items[ControllerMenuStartIndex]).IsChecked = true;
            }
        }

        void ControllerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            UncheckAllMenuItem(controllerMenuItem, ControllerMenuStartIndex);
            input.CurrentJoyStickIndex = (int)menuItem.Tag;
            menuItem.IsChecked = true;
        }

        [GeneratedRegex("^lang_\\w+_(?<ISO>\\w+).ini$")]
        private static partial Regex LangFileRegex();

        /// <summary>
        /// 一覧の 1 行（ボタン 1 つ分）です。
        /// </summary>
        class KeyRow : INotifyPropertyChanged
        {
            private string header = "";
            private string key = "0";
            private string button = "0";

            public event PropertyChangedEventHandler PropertyChanged;

            public string Header
            {
                get { return header; }
                set { header = value; OnPropertyChanged(); }
            }

            public string Key
            {
                get { return key; }
                set { key = value; OnPropertyChanged(); }
            }

            public string Button
            {
                get { return button; }
                set { button = value; OnPropertyChanged(); }
            }

            private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
