using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PPDEditor.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace PPDEditor.ViewModels.Docks
{
    /// <summary>
    /// ViewModel から使う、エディターと画面の機能です。
    /// </summary>
    public interface IResourceHost
    {
        bool IsProjectLoaded { get; }

        string ProjectDir { get; }

        /// <summary>
        /// 追加するファイルを選ばせます。
        /// </summary>
        /// <returns>キャンセルしたときは空。</returns>
        string[] AskFiles(string filter);

        void ShowMessage(string text);

        bool Confirm(string text, string caption);

        void SetClipboardText(string text);

        /// <summary>
        /// 音の情報（長さ、チャンネルの数、種類）です。
        /// </summary>
        List<KeyValuePair<object, object>> GetSoundInfo(string path);
    }

    /// <summary>
    /// 選んだリソースの中身の表示です（画像とエフェクトだけ）。
    /// </summary>
    public record ResourcePreview(ResourceKind Kind, string Path);

    /// <summary>
    /// プロジェクトのリソース（画像、エフェクト、音、その他）の一覧です。追加、削除、無効化と、選んだリソースの情報を扱います。
    /// </summary>
    public partial class ResourceManagerViewModel : DockViewModel
    {
        readonly IResourceHost host;

        [ObservableProperty]
        private ResourceNode selectedNode;

        [ObservableProperty]
        private ResourcePreview preview;

        /// <summary>
        /// リソースを追加できるか（プロジェクトを開いているか）です。
        /// </summary>
        [ObservableProperty]
        private bool canAdd;

        public ResourceManagerViewModel(IResourceHost host)
        {
            this.host = host;
            Roots = [.. new[] { ResourceKind.Image, ResourceKind.Effect, ResourceKind.Sound, ResourceKind.Others }
                .Select(kind => new ResourceNode(ResourceKind.Folder, ResourceStore.GetFolderName(kind), null) { ContentKind = kind })];
        }

        /// <summary>
        /// 種類ごとのフォルダー（Image、Effect、Sound、Others の順）です。
        /// </summary>
        public ResourceNode[] Roots { get; }

        public ObservableCollection<KeyValuePair<object, object>> InfoRows { get; } = [];

        private ResourceNode GetRoot(ResourceKind kind)
        {
            return Roots.First(r => r.ContentKind == kind);
        }

        public void CreateResourceFolders()
        {
            ResourceStore.CreateFolders(host.ProjectDir);
        }

        public void Clear()
        {
            foreach (var root in Roots)
            {
                root.Children.Clear();
            }
            InfoRows.Clear();
        }

        /// <summary>
        /// プロジェクトのリソースを読み込み、無効にしたもの（「|」区切りのパス）に印を付けます。
        /// </summary>
        public void ReadResource(string projectDirPath, string disableList)
        {
            foreach (var root in Roots)
            {
                foreach (var name in ResourceStore.GetFiles(projectDirPath, root.ContentKind))
                {
                    root.Children.Add(new ResourceNode(root.ContentKind, name, root));
                }
            }
            foreach (string disable in disableList.Split('|'))
            {
                var node = FindNode(disable);
                if (node != null && node.Kind != ResourceKind.Folder)
                {
                    node.IsDisabled = true;
                }
            }
        }

        private ResourceNode FindNode(string path)
        {
            IEnumerable<ResourceNode> current = Roots;
            ResourceNode found = null;
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

        public string GetDisableList()
        {
            return String.Join("|", GetList(enabled: false));
        }

        public string[] GetEnabledList()
        {
            return GetList(enabled: true);
        }

        public string[] GetImageList()
        {
            return [.. GetRoot(ResourceKind.Image).Children.Select(n => n.Name)];
        }

        public string[] GetEffectList()
        {
            return [.. GetRoot(ResourceKind.Effect).Children.Select(n => n.Name)];
        }

        public string[] GetSoundList()
        {
            return [.. GetRoot(ResourceKind.Sound).Children.Select(n => n.Name)];
        }

        public string[] GetOthersList()
        {
            return [.. GetRoot(ResourceKind.Others).Children.Select(n => n.Name)];
        }

        // 有効（または無効）なファイルのパス。エフェクトは使う画像も含める
        private string[] GetList(bool enabled)
        {
            var ret = new List<string>();
            var nodes = new Queue<ResourceNode>(Roots);
            while (nodes.Count > 0)
            {
                foreach (var child in nodes.Dequeue().Children)
                {
                    if (child.Kind == ResourceKind.Folder)
                    {
                        nodes.Enqueue(child);
                    }
                    else if (child.IsDisabled != enabled)
                    {
                        ret.Add(child.FullPath);
                        if (child.Parent == GetRoot(ResourceKind.Effect))
                        {
                            ret.AddRange(ResourceStore.GetEffectImages(host.ProjectDir, child.Name));
                        }
                    }
                }
            }
            return [.. ret];
        }

        public void WriteResource(Stream stream, string fileName)
        {
            ResourceStore.Write(host.ProjectDir, stream, fileName);
        }

        /// <summary>
        /// リソースを追加するメニューを開いたときに、追加できるかを確かめます。
        /// </summary>
        public void RefreshCanAdd()
        {
            CanAdd = host.IsProjectLoaded;
        }

        private void AddFiles(ResourceKind kind, string filter)
        {
            var files = host.AskFiles(filter);
            if (files.Length == 0)
            {
                return;
            }
            var root = GetRoot(kind);
            foreach (var file in files)
            {
                ResourceStore.Add(host.ProjectDir, kind, file,
                    name => root.Children.Add(new ResourceNode(kind, name, root)),
                    path => host.ShowMessage(String.Format("{0}\n{1}", Texts["AlreadyExistResource"], path)));
            }
            OnChanged();
        }

        [RelayCommand]
        private void AddImage()
        {
            AddFiles(ResourceKind.Image, "Image(*.jpg;*.jpeg;*.gif;*.png)|*.jpg;*.jpeg;*.gif;*.png");
        }

        [RelayCommand]
        private void AddEffect()
        {
            AddFiles(ResourceKind.Effect, "Effect(*.etd)|*.etd");
        }

        [RelayCommand]
        private void AddSound()
        {
            AddFiles(ResourceKind.Sound, Texts["SMWaveFilter"]);
        }

        [RelayCommand]
        private void AddOthers()
        {
            AddFiles(ResourceKind.Others, Texts["MovieFilter"]);
        }

        /// <summary>
        /// 選んでいるファイルを消します（確かめてから）。
        /// </summary>
        [RelayCommand]
        private void Delete()
        {
            var node = SelectedNode;
            if (node == null || !host.Confirm(String.Format(Texts["DeleteConfirmText"], node.FullPath), Texts["DeleteConfirm"]))
            {
                return;
            }
            ResourceStore.Delete(host.ProjectDir, node.FullPath);
            node.Parent?.Children.Remove(node);
            OnChanged();
        }

        [RelayCommand]
        private void ToggleDisabled()
        {
            if (SelectedNode != null)
            {
                SelectedNode.IsDisabled = !SelectedNode.IsDisabled;
                OnChanged();
            }
        }

        [RelayCommand]
        private void CopyName()
        {
            if (SelectedNode != null)
            {
                host.SetClipboardText(SelectedNode.Name);
            }
        }

        // 選んだリソースの情報と中身を出す
        partial void OnSelectedNodeChanged(ResourceNode value)
        {
            InfoRows.Clear();
            if (value == null || !host.IsProjectLoaded || value.Kind == ResourceKind.Folder)
            {
                return;
            }
            var path = Path.Combine(host.ProjectDir, ResourceStore.ResourcePath, value.FullPath);
            if (!File.Exists(path))
            {
                return;
            }
            try
            {
                var info = value.Kind switch
                {
                    ResourceKind.Image => ResourceStore.GetImageInfo(path),
                    ResourceKind.Effect => ResourceStore.GetEffectInfo(path),
                    ResourceKind.Sound => host.GetSoundInfo(path),
                    _ => ResourceStore.GetOthersInfo(path),
                };
                foreach (var row in info)
                {
                    InfoRows.Add(row);
                }
                if (value.Kind == ResourceKind.Image || value.Kind == ResourceKind.Effect)
                {
                    Preview = new ResourcePreview(value.Kind, path);
                }
            }
            catch (Exception e)
            {
                host.ShowMessage(e.Message);
            }
        }
    }

    /// <summary>
    /// リソースの一覧の 1 項目（フォルダーかファイル）です。
    /// </summary>
    public partial class ResourceNode(ResourceKind kind, string name, ResourceNode parent) : ObservableObject
    {
        public ResourceKind Kind { get; } = kind;

        /// <summary>
        /// 中に入れるファイルの種類です（種類ごとのフォルダーのとき）。
        /// </summary>
        public ResourceKind ContentKind { get; init; } = kind;

        public string Name { get; } = name;

        public ResourceNode Parent { get; } = parent;

        public ObservableCollection<ResourceNode> Children { get; } = [];

        [ObservableProperty]
        private bool isDisabled;

        public bool IsFolder
        {
            get { return Kind == ResourceKind.Folder; }
        }

        /// <summary>
        /// 一番上からの名前を「\」でつないだもの（以前の TreeNode.FullPath と同じ）です。
        /// </summary>
        public string FullPath
        {
            get { return Parent == null ? Name : Parent.FullPath + "\\" + Name; }
        }
    }
}
