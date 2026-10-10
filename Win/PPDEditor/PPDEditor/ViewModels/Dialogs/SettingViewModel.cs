using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Keys = System.Windows.Forms.Keys;

namespace PPDEditor.ViewModels.Dialogs
{
    /// <summary>
    /// エディターの設定（一般、ショートカットキー、環境変数）のダイアログです。
    /// </summary>
    public partial class SettingViewModel : DialogViewModel
    {
        readonly ShortcutManager shortcutManager;

        /// <summary>
        /// 表示しているページです（0: 一般、1: ショートカットキー、2: 環境変数）。
        /// </summary>
        [ObservableProperty]
        private int pageIndex;

        [ObservableProperty]
        private string authorName;

        [ObservableProperty]
        private string movieLatency;

        [ObservableProperty]
        private bool showToggleArrow;

        [ObservableProperty]
        private bool showToggleRectangle;

        [ObservableProperty]
        private bool enableToChangeMarkTypeAndTime;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RemoveCommandCommand), nameof(DeleteShortcutCommand))]
        private ShortcutRow selectedRow;

        /// <summary>
        /// 入力中のショートカットキーの表示です。
        /// </summary>
        [ObservableProperty]
        private string editingKey = "";

        public SettingViewModel(ShortcutManager shortcutManager)
        {
            this.shortcutManager = shortcutManager;
            authorName = PPDStaticSetting.AuthorName;
            movieLatency = PPDStaticSetting.MovieLatency.ToString();
            showToggleArrow = !PPDStaticSetting.HideToggleArrow;
            showToggleRectangle = !PPDStaticSetting.HideToggleRectangle;
            enableToChangeMarkTypeAndTime = PPDStaticSetting.EnableToChangeMarkTypeAndTime;
            Moves = [.. PPDStaticSetting.Moves.Select(v => (decimal)v)];
            Angles = [.. PPDStaticSetting.Angles.Select(v => (decimal)v)];

            // 決まったコマンドを種類の順に並べ、カスタムのコマンドはその後ろに並べる
            var shortcuts = shortcutManager.Shortcuts;
            foreach (var type in Enum.GetValues<ShortcutType>())
            {
                if (type == ShortcutType.None || type == ShortcutType.Custom)
                {
                    continue;
                }
                var info = shortcuts.FirstOrDefault(i => i.ShortcutType == type);
                Rows.Add(new ShortcutRow(Texts[type.ToString()], info == null ? "" : GetReadableShortcut(info), type));
            }
            foreach (var shortcut in shortcuts.Where(s => s.ShortcutType == ShortcutType.Custom))
            {
                Rows.Add(new ShortcutRow(GetCustomShortcutText(shortcut.ScriptPath), GetReadableShortcut(shortcut), ShortcutType.Custom));
            }
        }

        /// <summary>
        /// ショートカットキーを入力してほしいときに起こります（コマンドを足したときや選んだとき）。
        /// </summary>
        public event EventHandler KeyInputRequested;

        public ObservableCollection<ShortcutRow> Rows { get; } = [];

        /// <summary>
        /// マークの移動量（1～8）です。
        /// </summary>
        public decimal[] Moves { get; }

        /// <summary>
        /// ドラッグの角度制限（修飾キーの組み合わせごと）です。
        /// </summary>
        public decimal[] Angles { get; }

        /// <summary>
        /// 作者名か動画の遅延を変えたかです（再起動が要る）。
        /// </summary>
        public bool ShouldRestart
        {
            get;
            private set;
        }

        partial void OnAuthorNameChanged(string value)
        {
            ShouldRestart = true;
        }

        partial void OnMovieLatencyChanged(string value)
        {
            ShouldRestart = true;
        }

        partial void OnSelectedRowChanged(ShortcutRow value)
        {
            if (value != null)
            {
                EditingKey = value.Key;
                KeyInputRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 設定を保存します（ショートカットも登録し直します）。
        /// </summary>
        public void Apply()
        {
            PPDStaticSetting.AuthorName = AuthorName;
            _ = float.TryParse(MovieLatency, out float latency);
            PPDStaticSetting.MovieLatency = latency;
            PPDStaticSetting.HideToggleArrow = !ShowToggleArrow;
            PPDStaticSetting.HideToggleRectangle = !ShowToggleRectangle;
            PPDStaticSetting.EnableToChangeMarkTypeAndTime = EnableToChangeMarkTypeAndTime;
            PPDStaticSetting.Moves = [.. Moves.Select(v => (int)v)];
            PPDStaticSetting.Angles = [.. Angles.Select(v => (int)v)];

            shortcutManager.ClearShortcut();
            foreach (var row in Rows)
            {
                GetModifiers(row.Key, out Keys key, out bool ctrl, out bool shift, out bool alt);
                if (row.ShortcutType == ShortcutType.Custom)
                {
                    // キーがなくてもコマンドとして残す（以前と同じ）。スクリプトのパスは表示の文字から作り直す
                    shortcutManager.RegisterShortcut(new ShortcutInfo(key, shift, ctrl, alt, GetCustomShortcutScriptPath(row.Command)));
                }
                else if (key != Keys.None)
                {
                    shortcutManager.RegisterShortcut(new ShortcutInfo(key, shift, ctrl, alt, row.ShortcutType));
                }
            }
        }

        // 同じキーが 2 つのコマンドに割り当てられていないかを確かめる
        protected override bool Validate()
        {
            for (int i = 0; i < Rows.Count; i++)
            {
                for (int j = i + 1; j < Rows.Count; j++)
                {
                    string key1 = Rows[i].Key, key2 = Rows[j].Key;
                    if (!String.IsNullOrEmpty(key1) && key1 == key2)
                    {
                        ShowMessage(String.Format("{0}\n{1}\n{2}", Texts["SameShortcutKeyAssigned"], GetRowContent(Rows[i]), GetRowContent(Rows[j])));
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 押したキーを、選んでいるコマンドのショートカットにします。
        /// </summary>
        public void AssignKey(Keys key, bool shift, bool control, bool alt)
        {
            EditingKey = GetReadableShortcut(new ShortcutInfo(key, shift, control, alt, ShortcutType.None));
            if (SelectedRow != null)
            {
                SelectedRow.Key = EditingKey;
            }
        }

        /// <summary>
        /// キーの入力をやめます（Enter や Space を押したとき）。
        /// </summary>
        public void CancelKeyInput()
        {
            EditingKey = "";
        }

        private bool HasSelectedRow()
        {
            return SelectedRow != null;
        }

        [RelayCommand(CanExecute = nameof(HasSelectedRow))]
        private void DeleteShortcut()
        {
            SelectedRow.Key = "";
            EditingKey = "";
        }

        /// <summary>
        /// コマンドを選ばせます（選んだスクリプトのパスを返し、キャンセルしたときは null）。
        /// </summary>
        public Func<string> SelectCommand
        {
            get;
            set;
        }

        [RelayCommand]
        private void AddCommand()
        {
            var scriptPath = SelectCommand?.Invoke();
            if (scriptPath == null)
            {
                return;
            }
            var row = new ShortcutRow(GetCustomShortcutText(scriptPath), "", ShortcutType.Custom);
            Rows.Add(row);
            SelectedRow = row;
        }

        private bool CanRemoveCommand()
        {
            return SelectedRow != null && SelectedRow.ShortcutType == ShortcutType.Custom;
        }

        [RelayCommand(CanExecute = nameof(CanRemoveCommand))]
        private void RemoveCommand()
        {
            Rows.Remove(SelectedRow);
        }

        private static string GetRowContent(ShortcutRow row)
        {
            return String.Format("{0} {1}", row.Command, row.Key);
        }

        // Commands\a\b → a:b
        private static string GetCustomShortcutText(string scriptPath)
        {
            return String.Join(":", scriptPath.Split(Path.DirectorySeparatorChar).Skip(1).ToArray());
        }

        // a:b → Commands\a\b
        private static string GetCustomShortcutScriptPath(string text)
        {
            return Path.Combine("Commands", String.Join(Path.DirectorySeparatorChar.ToString(), text.Split(':')));
        }

        private static void GetModifiers(string text, out Keys key, out bool ctrl, out bool shift, out bool alt)
        {
            key = Keys.None;
            ctrl = false;
            shift = false;
            alt = false;
            foreach (string part in text.Split('+'))
            {
                switch (part)
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
                        if (!Enum.TryParse(part, out key))
                        {
                            key = Keys.None;
                        }
                        break;
                }
            }
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
            return String.Join("+", list);
        }
    }

    /// <summary>
    /// ショートカットの一覧の 1 行（コマンドとキー）です。
    /// </summary>
    public partial class ShortcutRow(string command, string key, ShortcutType shortcutType) : ObservableObject
    {
        public string Command { get; } = command;

        [ObservableProperty]
        private string key = key;

        public ShortcutType ShortcutType { get; } = shortcutType;
    }
}
