using System;
using System.Collections.Generic;
using System.IO;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Keys = System.Windows.Forms.Keys;

namespace PPDEditor.Forms
{
    public partial class SettingWindow : Window
    {
        readonly ObservableCollection<ShortcutRow> rows = [];
        bool ignore;
        ShortcutManager shortcutManager;
        int regularShortcutCount;
        public SettingWindow()
        {
            ignore = true;
            InitializeComponent();
            ignore = false;
            dataGridView1.ItemsSource = rows;
            listBox1.SelectedIndex = 0;
        }

        public void SetLangAngShortcut(ShortcutManager shortcutManager)
        {
            Title = Utility.Language["Setting"];
            label1.Text = Utility.Language["SFAutherName"];
            label2.Text = Utility.Language["SFMovieLatency"];
            groupBox1.Header = Utility.Language["SFGroupBox1"];
            groupBox2.Header = Utility.Language["SFGroupBox2"];
            checkBox1.Content = Utility.Language["SFCheckBox1"];
            checkBox2.Content = Utility.Language["SFCheckBox2"];
            ((ListBoxItem)listBox1.Items[0]).Content = Utility.Language["General"];
            ((ListBoxItem)listBox1.Items[1]).Content = Utility.Language["ShortcutKey"];
            ((ListBoxItem)listBox1.Items[2]).Content = Utility.Language["EnvironmentVariable"];
            Column1.Header = Utility.Language["Command"];
            Column2.Header = Utility.Language["Key"];
            groupBox3.Header = Utility.Language["ToggleOption"];
            moveLabel1.Text = Utility.Language["Move1"];
            moveLabel2.Text = Utility.Language["Move2"];
            moveLabel3.Text = Utility.Language["Move3"];
            moveLabel4.Text = Utility.Language["Move4"];
            moveLabel5.Text = Utility.Language["Move5"];
            moveLabel6.Text = Utility.Language["Move6"];
            moveLabel7.Text = Utility.Language["Move7"];
            moveLabel8.Text = Utility.Language["Move8"];
            label13.Text = Utility.Language["None"];
            button3.Content = Utility.Language["DeleteShortcut"];
            label6.Text = Utility.Language["DragAngleRestriction"];
            checkBox3.Content = new TextBlock { Text = Utility.Language["SFCheckBox3"], TextWrapping = TextWrapping.Wrap };
            button4.Content = Utility.Language["AddCommand"];
            button5.Content = Utility.Language["RemoveCommand"];


            this.shortcutManager = shortcutManager;
            ShortcutInfo[] shortcuts = shortcutManager.Shortcuts;
            rows.Clear();

            regularShortcutCount = 0;
            foreach (ShortcutType type in Enum.GetValues<ShortcutType>())
            {
                if (type == ShortcutType.None)
                {
                    continue;
                }
                else if (type == ShortcutType.Custom)
                {
                    foreach (var shortcut in shortcuts.Where(s => s.ShortcutType == ShortcutType.Custom))
                    {
                        rows.Add(new ShortcutRow(GetCustomShortcutText(shortcut), GetReadableShortcut(shortcut)));
                    }
                }
                else
                {
                    var info = shortcuts.FirstOrDefault(i => i.ShortcutType == type);
                    rows.Add(new ShortcutRow(Utility.Language[type.ToString()], info == null ? "" : GetReadableShortcut(info)));
                    regularShortcutCount++;
                }
            }

        }

        private static string GetCustomShortcutText(ShortcutInfo shortcut)
        {
            return GetCustomShortcutText(shortcut.ScriptPath);
        }

        private static string GetCustomShortcutText(string scriptPath)
        {
            return String.Join(":", scriptPath.Split(Path.DirectorySeparatorChar).Skip(1).ToArray());
        }

        private static string GetCustomShortcutScriptPath(string str)
        {
            return Path.Combine("Commands", String.Join(Path.DirectorySeparatorChar.ToString(), str.Split(':')));
        }

        public string AuthorName
        {
            get
            {
                return textBox1.Text;
            }
            set
            {
                ignore = true;
                textBox1.Text = value;
                ignore = false;
            }
        }
        public float MovieLatency
        {
            get
            {
                _ = float.TryParse(this.textBox2.Text, out float ret);
                return ret;
            }
            set
            {
                ignore = true;
                this.textBox2.Text = value.ToString();
                ignore = false;
            }
        }

        public int[] Moves
        {
            get
            {
                return [
                    (int)move1.Value,                    (int)move2.Value,                    (int)move3.Value,                    (int)move4.Value,                    (int)move5.Value,                    (int)move6.Value,                    (int)move7.Value,                    (int)move8.Value                ];
            }
            set
            {
                move1.Value = value[0];
                move2.Value = value[1];
                move3.Value = value[2];
                move4.Value = value[3];
                move5.Value = value[4];
                move6.Value = value[5];
                move7.Value = value[6];
                move8.Value = value[7];
            }
        }

        public int[] Angles
        {
            get
            {
                return [
                    (int)angle1.Value,                    (int)angle2.Value,                    (int)angle3.Value,                    (int)angle4.Value,                    (int)angle5.Value,                    (int)angle6.Value,                    (int)angle7.Value,                    (int)angle8.Value                ];
            }
            set
            {
                angle1.Value = value[0];
                angle2.Value = value[1];
                angle3.Value = value[2];
                angle4.Value = value[3];
                angle5.Value = value[4];
                angle6.Value = value[5];
                angle7.Value = value[6];
                angle8.Value = value[7];
            }
        }

        public bool HideToggleArrow
        {
            get
            {
                return checkBox1.IsChecked != true;
            }
            set
            {
                checkBox1.IsChecked = !value;
            }
        }

        public bool HideToggleRectangle
        {
            get
            {
                return checkBox2.IsChecked != true;
            }
            set
            {
                checkBox2.IsChecked = !value;
            }
        }

        public bool EnableToChangeMarkTypeAndTime
        {
            get
            {
                return checkBox3.IsChecked == true;
            }
            set
            {
                checkBox3.IsChecked = value;
            }
        }

        public bool ShouldRestart
        {
            get;
            private set;
        }

        private bool CheckSameShortcut()
        {
            for (int i = 0; i < rows.Count; i++)
            {
                for (int j = i + 1; j < rows.Count; j++)
                {
                    string val1 = rows[i].Key, val2 = rows[j].Key;
                    if (!String.IsNullOrEmpty(val1) && !String.IsNullOrEmpty(val2) && val1 == val2)
                    {
                        MessageBox.Show(this, String.Format("{0}\n{1}\n{2}", Utility.Language["SameShortcutKeyAssigned"],
                            GetRowContent(i), GetRowContent(j)));
                        return true;
                    }
                }
            }

            return false;
        }

        private string GetRowContent(int index)
        {
            return String.Format("{0} {1}", rows[index].Command, rows[index].Key);
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            if (CheckSameShortcut())
            {
                return;
            }

            shortcutManager.ClearShortcut();
            int iter = 0;
            foreach (ShortcutType type in Enum.GetValues<ShortcutType>())
            {
                if (type == ShortcutType.None)
                {
                    continue;
                }
                else if (type == ShortcutType.Custom)
                {
                    for (int i = iter; i < rows.Count; i++)
                    {
                        var shortcut = GetCustomShortcutInfo(rows[iter].Key,
                            GetCustomShortcutScriptPath(rows[iter].Command));
                        if (shortcut != null)
                        {
                            shortcutManager.RegisterShortcut(shortcut);
                        }
                        iter++;
                    }
                }
                else
                {
                    var shortcut = GetShortcutInfo(rows[iter].Key, type);
                    if (shortcut != null)
                    {
                        shortcutManager.RegisterShortcut(shortcut);
                    }
                    iter++;
                }
            }

            this.DialogResult = true;
        }

        private static void GetModifiers(string str, out Keys key, out bool ctrl, out bool shift, out bool alt)
        {
            var split = str.Split('+');
            key = Keys.None;
            ctrl = false;
            shift = false;
            alt = false;
            foreach (string sp in split)
            {
                switch (sp)
                {
                    case "Ctrl":
                        ctrl = true;
                        break;
                    case "Shift":
                        shift = true;
                        break;
                    case "Alt":
                        alt = true;
                        break;
                    default:
                        if (!Enum.TryParse(sp, out key))
                        {
                            key = Keys.None;
                        }

                        break;
                }
            }
        }

        private static ShortcutInfo GetShortcutInfo(string str, ShortcutType shortcutType)
        {
            GetModifiers(str, out Keys key, out bool ctrl, out bool shift, out bool alt);

            if (key != Keys.None)
            {
                return new ShortcutInfo(key, shift, ctrl, alt, shortcutType);
            }

            return null;
        }

        private static ShortcutInfo GetCustomShortcutInfo(string str, string scriptPath)
        {
            GetModifiers(str, out Keys key, out bool ctrl, out bool shift, out bool alt);

            if (key != Keys.NoName)
            {
                return new ShortcutInfo(key, shift, ctrl, alt, scriptPath);
            }
            return null;
        }

        private static string GetReadableShortcut(ShortcutInfo info)
        {
            var list = new List<string>();
            if (info.Control)
            {
                list.Add("Ctrl");
            }
            if (info.Shift)
            {
                list.Add("Shift");
            }
            if (info.Alt)
            {
                list.Add("Alt");
            }
            list.Add(info.Key.ToString());
            return String.Join("+", list.ToArray());
        }

        private void ListBox1_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            generalPanel.Visibility = listBox1.SelectedIndex == 0 ? Visibility.Visible : Visibility.Collapsed;
            shortcutPanel.Visibility = listBox1.SelectedIndex == 1 ? Visibility.Visible : Visibility.Collapsed;
            variablePanel.Visibility = listBox1.SelectedIndex == 2 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void TextBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ignore) return;
            ShouldRestart = true;
        }

        private void TextBox2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ignore) return;
            ShouldRestart = true;
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridView1.SelectedItem is not ShortcutRow row)
            {
                return;
            }

            row.Key = "";
            textBox3.Text = "";
        }

        private void DataGridView1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGridView1.SelectedItem == null)
            {
                return;
            }

            button5.IsEnabled = dataGridView1.SelectedIndex >= regularShortcutCount;
            EnableEditMode();
        }

        private void EnableEditMode()
        {
            if (dataGridView1.SelectedItem is not ShortcutRow row)
            {
                return;
            }
            textBox3.Text = row.Key;
            textBox3.IsEnabled = true;
            textBox3.Focus();
            textBox3.SelectAll();
        }

        /// <summary>
        /// 押したキーを、選んでいるコマンドのショートカットにします。Enter と Space は入力をやめます。
        /// </summary>
        private void TextBox3_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;

            // Alt を押しているときは SystemKey に本当のキーが入る
            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            if (key == Key.LeftCtrl || key == Key.RightCtrl || key == Key.LeftShift || key == Key.RightShift || key == Key.LeftAlt || key == Key.RightAlt)
            {
                return;
            }

            if (key == Key.Return || key == Key.Space)
            {
                dataGridView1.Focus();
                textBox3.Text = "";
                return;
            }

            // ショートカットは WinForms のキーの値で保存しているので、仮想キーコードを経由して変換する（値は同じ）
            var modifiers = Keyboard.Modifiers;
            var winFormsKey = (Keys)KeyInterop.VirtualKeyFromKey(key);
            textBox3.Text = GetReadableShortcut(new ShortcutInfo(winFormsKey, modifiers.HasFlag(ModifierKeys.Shift), modifiers.HasFlag(ModifierKeys.Control), modifiers.HasFlag(ModifierKeys.Alt), ShortcutType.None));
            if (dataGridView1.SelectedItem is ShortcutRow row)
            {
                row.Key = textBox3.Text;
            }
        }

        private void TextBox3_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            textBox3.IsEnabled = false;
        }

        private void Button4_Click(object sender, RoutedEventArgs e)
        {
            var form = new CommandSelectWindow
            {
                Owner = this
            };
            form.SetLang();
            if (form.ShowDialog() == true)
            {
                var row = new ShortcutRow(GetCustomShortcutText(form.SelectedScriptPath), "");
                rows.Add(row);
                dataGridView1.SelectedItem = row;
                dataGridView1.ScrollIntoView(row);
                EnableEditMode();
            }
        }

        private void Button5_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridView1.SelectedItem is ShortcutRow row)
            {
                rows.Remove(row);
            }
        }

        /// <summary>
        /// ショートカットの一覧の 1 行です。
        /// </summary>
        class ShortcutRow(string command, string key) : System.ComponentModel.INotifyPropertyChanged
        {
            private string key = key;

            public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

            public string Command { get; } = command;

            public string Key
            {
                get { return key; }
                set
                {
                    key = value;
                    PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(Key)));
                }
            }
        }
    }
}
