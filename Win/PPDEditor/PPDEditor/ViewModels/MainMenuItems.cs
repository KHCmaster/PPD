using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace PPDEditor.ViewModels
{
    /// <summary>
    /// メインのウィンドウのドックのウィンドウの種類です。
    /// </summary>
    public enum DockKind
    {
        GameWindow,
        TimeLine,
        Property,
        PosAndAngle,
        Kasi,
        SoundManager,
        IniFileWriter,
        BPMMeasure,
        LayerManager,
        EventManager,
        GeometryCreator,
        Memo,
        ResourceManager,
        ScriptManager,
        StatsManager,
        Help,
    }

    /// <summary>
    /// グリッドの大きさの選び方です。
    /// </summary>
    public enum GridSizeChoice
    {
        None,
        Px10,
        Px20,
        Px50,
        Custom,
    }

    /// <summary>
    /// 言語ファイルのキーで表示する文字です（コンボボックスの項目など）。言語を変えたら <see cref="Refresh"/> で表示し直します。
    /// </summary>
    public class LocalizedText(string key) : ObservableObject
    {
        public string Key { get; } = key;

        public string Text
        {
            get { return Utility.Language[Key]; }
        }

        public void Refresh()
        {
            OnPropertyChanged(nameof(Text));
        }

        public override string ToString()
        {
            return Text;
        }
    }

    /// <summary>
    /// 表示のメニューの、ドックのウィンドウを出したり隠したりする項目です。
    /// </summary>
    public partial class DockMenuItem(DockKind kind, string key) : LocalizedText(key)
    {
        public DockKind Kind { get; } = kind;

        /// <summary>
        /// ウィンドウが出ているかどうか（メニューを開いたときに調べる）。
        /// </summary>
        [ObservableProperty]
        private bool isShown;
    }

    /// <summary>
    /// メニューの、ファイルを指す項目です（最近使ったプロジェクト、言語、ビューの配置）。
    /// </summary>
    public partial class FileMenuItem(string header, string path) : ObservableObject
    {
        /// <summary>
        /// 表示する文字（「_」はアクセスキーにならないようにしてある）。
        /// </summary>
        public string Header { get; } = MenuText.Escape(header);

        public string Path { get; } = path;

        [ObservableProperty]
        private bool isChecked;
    }

    /// <summary>
    /// コマンド（Commands のフォルダの FlowScript）のメニューの項目です。フォルダのときは子を持つ。
    /// </summary>
    public class CommandMenuItem(string name, string path, string gesture)
    {
        public string Header { get; } = MenuText.Escape(name);

        /// <summary>
        /// スクリプトのパス。フォルダのときは null。
        /// </summary>
        public string Path { get; } = path;

        public string InputGesture { get; } = gesture;

        public bool IsFolder
        {
            get { return Path == null; }
        }

        public ObservableCollection<CommandMenuItem> Children { get; } = [];
    }

    /// <summary>
    /// MOD（mods のフォルダ）のメニューの項目です。フォルダのときは子を持ち、MOD のファイルのときはチェックできる。
    /// </summary>
    public partial class ModMenuItem(string name, bool isMod, ModMenuItem parent) : ObservableObject
    {
        public string Name { get; } = name;

        public string Header { get; } = MenuText.Escape(name);

        public bool IsMod { get; } = isMod;

        public ModMenuItem Parent { get; } = parent;

        public ObservableCollection<ModMenuItem> Children { get; } = [];

        [ObservableProperty]
        private bool isChecked;

        /// <summary>
        /// mods のフォルダからの名前（「フォルダ\名前」）です。
        /// </summary>
        public string FullName
        {
            get { return Parent == null ? Name : Parent.FullName + System.IO.Path.DirectorySeparatorChar + Name; }
        }
    }

    static class MenuText
    {
        /// <summary>
        /// メニューでは「_」がアクセスキーの印になるので、そのまま出るようにする。
        /// </summary>
        public static string Escape(string text)
        {
            return text.Replace("_", "__");
        }
    }
}
