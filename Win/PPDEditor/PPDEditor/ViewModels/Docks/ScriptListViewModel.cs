using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlowScriptControl.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace PPDEditor.ViewModels.Docks
{
    /// <summary>
    /// ViewModel から使う、エディターと画面の機能です。
    /// </summary>
    public interface IScriptListHost
    {
        bool IsProjectLoaded { get; }

        string ProjectDir { get; }

        AvailableDifficulty CurrentDifficulty { get; }

        void ShowMessage(string text);

        bool Confirm(string text, string caption);
    }

    /// <summary>
    /// コマンド、関数、譜面のスクリプトの一覧です。フォルダーとスクリプトの作成・削除・名前の変更・無効化と、関数を譜面のスクリプトへコピーするのを扱います。
    /// </summary>
    /// <remarks>
    /// コマンドと関数はエディターのフォルダー（Commands、Functions）、譜面のスクリプトはプロジェクトの「難易度_Scripts」フォルダーにある。
    /// </remarks>
    public partial class ScriptListViewModel : ObservableObject
    {
        readonly IScriptListHost host;

        [ObservableProperty]
        private ScriptNode selectedNode;

        public ScriptListViewModel(IScriptListHost host)
        {
            this.host = host;
            CommandRoot = new ScriptNode(this, null, "Commands", true);
            FunctionRoot = new ScriptNode(this, null, "Functions", true);
            ScriptRoot = new ScriptNode(this, null, "Scripts", true);
            Roots = [CommandRoot, FunctionRoot, ScriptRoot];
            ReadEditorScripts(CommandRoot);
            ReadEditorScripts(FunctionRoot);
        }

        public event Action Reloaded;

        /// <summary>
        /// 名前を変えたときに起こります（前のパス、新しいパス）。
        /// </summary>
        public event Action<string, string> Renamed;

        public event Action<string> Deleted;

        /// <summary>
        /// スクリプトを開くときに起こります（ファイルのパス）。
        /// </summary>
        public event Action<string> ScriptSelected;

        public event EventHandler Modified;

        public LanguageTexts Texts
        {
            get { return LanguageTexts.Instance; }
        }

        public ScriptNode CommandRoot { get; }

        public ScriptNode FunctionRoot { get; }

        public ScriptNode ScriptRoot { get; }

        public ScriptNode[] Roots { get; }

        internal IScriptListHost Host
        {
            get { return host; }
        }

        /// <summary>
        /// 選んでいるスクリプトのファイルのパスです（フォルダーのときは null）。
        /// </summary>
        public string CurrentScriptPath
        {
            get { return SelectedNode == null || SelectedNode.IsFolder ? null : SelectedNode.FullPath + ".fsml"; }
        }

        /// <summary>
        /// 一番上のフォルダー以外を選んでいるかです。
        /// </summary>
        public bool ChildNodeSelected
        {
            get { return SelectedNode != null && !SelectedNode.IsRoot; }
        }

        /// <summary>
        /// 項目のファイルかフォルダーのパスです（スクリプトは拡張子なし）。
        /// </summary>
        internal string GetFullPath(ScriptNode node)
        {
            if (node.Root == ScriptRoot)
            {
                return Path.Combine(host.ProjectDir, String.Format("{0}_{1}", host.CurrentDifficulty, node.TreePath));
            }
            return Path.Combine(Utility.AppDir, node.TreePath);
        }

        public void Clear()
        {
            ScriptRoot.Children.Clear();
        }

        public string[] GetEnabledScriptList()
        {
            return [.. ScriptRoot.GetScripts().Where(n => !n.IsDisabled).Select(n => n.TreePath)];
        }

        public string GetDisabledScriptList()
        {
            return String.Join("|", ScriptRoot.GetScripts().Where(n => n.IsDisabled).Select(n => n.TreePath));
        }

        /// <summary>
        /// 譜面のスクリプトの中身をストリームに書きます（Scripts\フォルダー\名前）。
        /// </summary>
        public void WriteScript(Stream stream, string scriptName)
        {
            var node = ScriptRoot;
            foreach (var name in scriptName.Split('\\').Skip(1))
            {
                node = node.FindChild(name);
                if (node == null)
                {
                    return;
                }
            }
            using FileStream fs = File.Open(GetFullPath(node) + ".fsml", FileMode.Open);
            fs.CopyTo(stream);
        }

        /// <summary>
        /// 譜面のスクリプトを読み込み、無効にしたもの（「|」区切りのパス）に印を付けます。
        /// </summary>
        public void ReadScript(string projectDirPath, AvailableDifficulty difficulty, string disableList)
        {
            var queue = new Queue<ScriptNode>();
            queue.Enqueue(ScriptRoot);
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                var dir = Path.Combine(projectDirPath, String.Format("{0}_{1}", difficulty, node.TreePath));
                Utility.SafeCreateDirectory(dir);
                foreach (string childDir in Directory.GetDirectories(dir))
                {
                    queue.Enqueue(node.AddChild(Path.GetFileName(childDir), true));
                }
                foreach (string childFile in Directory.GetFiles(dir))
                {
                    if (Path.GetExtension(childFile).Equals(".fsml", StringComparison.CurrentCultureIgnoreCase))
                    {
                        node.AddChild(Path.GetFileNameWithoutExtension(childFile), false);
                    }
                }
            }

            foreach (string disable in disableList.Split('|'))
            {
                var node = FindByPath(disable);
                if (node != null && !node.IsFolder)
                {
                    node.IsDisabled = true;
                }
            }
        }

        private ScriptNode FindByPath(string path)
        {
            IEnumerable<ScriptNode> current = Roots;
            ScriptNode found = null;
            foreach (var name in path.Split('\\'))
            {
                found = current.FirstOrDefault(n => n.Name == name);
                if (found == null)
                {
                    return null;
                }
                current = found.Children;
            }
            return found;
        }

        // エディターのフォルダーのコマンドや関数を読み込む（なくなったものは消す）
        private static void ReadEditorScripts(ScriptNode root)
        {
            var dir = Path.Combine(Utility.AppDir, root.Name);
            if (Directory.Exists(dir))
            {
                ReadNodes(root, dir);
            }
        }

        private static void ReadNodes(ScriptNode parent, string dirPath)
        {
            foreach (string childDirPath in Directory.GetDirectories(dirPath))
            {
                var name = Path.GetFileNameWithoutExtension(childDirPath);
                ReadNodes(parent.FindChild(name) ?? parent.AddChild(name, true), childDirPath);
            }
            foreach (string childFilePath in Directory.GetFiles(dirPath))
            {
                if (Path.GetExtension(childFilePath).Equals(".fsml", StringComparison.CurrentCultureIgnoreCase))
                {
                    var name = Path.GetFileNameWithoutExtension(childFilePath);
                    if (parent.FindChild(name) == null)
                    {
                        parent.AddChild(name, false);
                    }
                }
            }
            foreach (var node in parent.Children.ToArray())
            {
                var path = Path.Combine(Utility.AppDir, node.TreePath);
                var exists = node.IsFolder ? Directory.Exists(path) : File.Exists(Path.ChangeExtension(path, ".fsml"));
                if (!exists)
                {
                    parent.Children.Remove(node);
                }
            }
        }

        /// <summary>
        /// コマンドと関数、譜面のスクリプトを読み込み直します。
        /// </summary>
        [RelayCommand]
        private void Reload()
        {
            ReadEditorScripts(CommandRoot);
            ReadEditorScripts(FunctionRoot);
            if (!host.IsProjectLoaded)
            {
                return;
            }
            var disableList = GetDisabledScriptList();
            ScriptRoot.Children.Clear();
            ReadScript(host.ProjectDir, host.CurrentDifficulty, disableList);
            Reloaded?.Invoke();
        }

        // 譜面のスクリプトは、プロジェクトを開いているときだけ作れる
        private bool CanCreateIn(ScriptNode node)
        {
            return node != null && !(node.Root == ScriptRoot && node != ScriptRoot && !host.IsProjectLoaded);
        }

        [RelayCommand]
        private void CreateFolder()
        {
            var selected = SelectedNode;
            if (!CanCreateIn(selected))
            {
                return;
            }
            var folder = selected.IsFolder ? selected : selected.Parent;
            var newFolder = GetSafeName(GetFullPath(folder), "New Folder", "", Directory.Exists);
            Directory.CreateDirectory(newFolder);
            folder.AddChild(Path.GetFileNameWithoutExtension(newFolder), true);
            selected.IsExpanded = true;
            Modified?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private void CreateScript()
        {
            var selected = SelectedNode;
            if (!CanCreateIn(selected))
            {
                return;
            }
            var newFile = GetSafeName(GetFullPath(selected), "New Script", ".fsml", File.Exists);
            Utility.RecursiveCreateFolder(newFile);
            File.WriteAllText(newFile, FlowDrawPanel.EmptyFileString, Encoding.UTF8);
            selected.AddChild(Path.GetFileNameWithoutExtension(newFile), false);
            selected.IsExpanded = true;
        }

        private static string GetSafeName(string dirPath, string baseName, string extension, Func<string, bool> exists)
        {
            var newPath = Path.Combine(dirPath, baseName + extension);
            for (int i = 1; exists(newPath); i++)
            {
                newPath = Path.Combine(dirPath, String.Format("{0}({1}){2}", baseName, i, extension));
            }
            return newPath;
        }

        [RelayCommand]
        private void Delete()
        {
            var node = SelectedNode;
            // 一番上のフォルダー（Commands など）は消さない（メニューでも消せないようにしている）
            if (node == null || node.IsRoot || !host.Confirm(String.Format(Texts["DeleteConfirmText"], node.TreePath), Texts["DeleteConfirm"]))
            {
                return;
            }
            var path = GetFullPath(node);
            if (node.IsFolder)
            {
                Directory.Delete(path, true);
            }
            else
            {
                path += ".fsml";
                File.Delete(path);
            }
            Deleted?.Invoke(path);
            node.Parent.Children.Remove(node);
        }

        [RelayCommand]
        private void ToggleDisabled()
        {
            SelectedNode?.IsDisabled = !SelectedNode.IsDisabled;
        }

        [RelayCommand]
        private void BeginRename()
        {
            if (ChildNodeSelected)
            {
                SelectedNode.EditingName = SelectedNode.Name;
                SelectedNode.IsEditing = true;
            }
        }

        /// <summary>
        /// 名前の変更を終えます。使えない名前や同じ名前のときは変えません。
        /// </summary>
        internal void EndRename(ScriptNode node, bool commit)
        {
            if (!node.IsEditing)
            {
                return;
            }
            node.IsEditing = false;
            var label = node.EditingName;
            if (!commit || String.IsNullOrEmpty(label) || label == node.Name
                || label.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || label.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                return;
            }

            var beforePath = GetFullPath(node);
            var path = Path.Combine(GetFullPath(node.Parent), label);
            if (node.IsFolder)
            {
                if (Directory.Exists(path))
                {
                    host.ShowMessage(Texts["AlreadyExistName"]);
                    return;
                }
                Directory.Move(beforePath, path);
            }
            else
            {
                path += ".fsml";
                beforePath += ".fsml";
                if (File.Exists(path))
                {
                    host.ShowMessage(Texts["AlreadyExistName"]);
                    return;
                }
                File.Move(beforePath, path);
            }
            node.Name = label;
            Renamed?.Invoke(beforePath, path);
        }

        /// <summary>
        /// 選んでいるスクリプトを開きます。
        /// </summary>
        [RelayCommand]
        private void OpenSelected()
        {
            if (SelectedNode != null && !SelectedNode.IsFolder)
            {
                ScriptSelected?.Invoke(GetFullPath(SelectedNode) + ".fsml");
            }
        }

        /// <summary>
        /// 関数をドラッグし始められるかです（プロジェクトを開いていて、関数の中の項目のとき）。
        /// </summary>
        public bool CanDragFunction(ScriptNode node)
        {
            return node != null && host.IsProjectLoaded && node.Root == FunctionRoot && node != FunctionRoot;
        }

        /// <summary>
        /// 関数を落とせる場所かです（譜面のスクリプトの中）。
        /// </summary>
        public bool CanDropFunction(ScriptNode source, ScriptNode target)
        {
            return source != null && target != null && target != source && source.Root == FunctionRoot && target.Root == ScriptRoot;
        }

        /// <summary>
        /// 関数（ファイルかフォルダー）を、譜面のスクリプトのフォルダーにコピーします。
        /// </summary>
        public void CopyFunction(ScriptNode source, ScriptNode target)
        {
            if (!target.IsFolder)
            {
                target = target.Parent;
            }
            var sourcePath = Path.Combine(Utility.AppDir, source.TreePath);
            var destPath = Path.Combine(GetFullPath(target), source.Name);
            if (Directory.Exists(sourcePath))
            {
                Utility.CopyDirectory(sourcePath, destPath, null,
                    fileName => host.Confirm(String.Format(Texts["OverwriteConfirm"] + "\n{0}", fileName), Texts["Confirm"]));
                Reload();
                return;
            }
            sourcePath += ".fsml";
            destPath += ".fsml";
            if (File.Exists(destPath) && !host.Confirm(String.Format(Texts["OverwriteConfirm"] + "\n{0}", destPath), Texts["Confirm"]))
            {
                return;
            }
            File.Copy(sourcePath, destPath, true);
            Reload();
        }
    }

    /// <summary>
    /// スクリプトの一覧の 1 項目（フォルダーかスクリプト）です。
    /// </summary>
    public partial class ScriptNode(ScriptListViewModel owner, ScriptNode parent, string name, bool isFolder) : ObservableObject
    {
        [ObservableProperty]
        private string name = name;

        [ObservableProperty]
        private bool isDisabled;

        [ObservableProperty]
        private bool isExpanded;

        [ObservableProperty]
        private bool isSelected;

        /// <summary>
        /// 名前を変えているかです。
        /// </summary>
        [ObservableProperty]
        private bool isEditing;

        [ObservableProperty]
        private string editingName = "";

        public ScriptNode Parent { get; } = parent;

        public bool IsFolder { get; } = isFolder;

        public ObservableCollection<ScriptNode> Children { get; } = [];

        public bool IsRoot
        {
            get { return Parent == null; }
        }

        /// <summary>
        /// 一番上のフォルダー（Commands、Functions、Scripts）です。
        /// </summary>
        public ScriptNode Root
        {
            get { return Parent == null ? this : Parent.Root; }
        }

        /// <summary>
        /// 譜面のスクリプトの中かです（無効にできる）。
        /// </summary>
        public bool IsUnderScripts
        {
            get { return Root == owner.ScriptRoot; }
        }

        /// <summary>
        /// 一番上からの名前を「\」でつないだもの（以前の TreeNode.FullPath と同じ）です。
        /// </summary>
        public string TreePath
        {
            get { return Parent == null ? Name : Parent.TreePath + "\\" + Name; }
        }

        /// <summary>
        /// ファイルかフォルダーのパスです（スクリプトは拡張子なし）。
        /// </summary>
        public string FullPath
        {
            get { return owner.GetFullPath(this); }
        }

        partial void OnIsSelectedChanged(bool value)
        {
            if (value)
            {
                owner.SelectedNode = this;
            }
        }

        public ScriptNode FindChild(string childName)
        {
            return Children.FirstOrDefault(c => c.Name == childName);
        }

        public ScriptNode AddChild(string childName, bool childIsFolder)
        {
            var node = new ScriptNode(owner, this, childName, childIsFolder);
            Children.Add(node);
            return node;
        }

        /// <summary>
        /// この中のスクリプト（フォルダーの中も含む）です。
        /// </summary>
        public IEnumerable<ScriptNode> GetScripts()
        {
            var queue = new Queue<ScriptNode>(Children);
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                if (node.IsFolder)
                {
                    foreach (var child in node.Children)
                    {
                        queue.Enqueue(child);
                    }
                }
                else
                {
                    yield return node;
                }
            }
        }

        [RelayCommand]
        private void EndRename(bool commit)
        {
            owner.EndRename(this, commit);
        }
    }
}
