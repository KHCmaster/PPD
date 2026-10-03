using FlowScriptControl.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PPDEditor.DockForm.Script
{
    /// <summary>
    /// コマンド、関数、譜面のスクリプトを木で出します。
    /// </summary>
    public partial class ScriptListPanel : UserControl
    {
        [Flags]
        enum Filter
        {
            None = 0,
            Enabled = 1,
            Disabled = 2
        }

        /// <summary>
        /// 木の 1 項目の情報です。<see cref="TreeViewItem.Tag"/> に入れる。
        /// </summary>
        sealed class ScriptListNode
        {
            public bool IsFolder
            {
                get;
                init;
            }

            public string Name
            {
                get;
                set;
            }

            public bool Disabled
            {
                get;
                set;
            }

            public TextBlock NameText
            {
                get;
                init;
            }

            public TextBox NameEditor
            {
                get;
                init;
            }

            public UIElement DisabledMark
            {
                get;
                init;
            }
        }

        const string DragDataFormat = "PPDEditor.ScriptListNode";

        static readonly ImageSource folderIcon = LoadIcon("folder");
        static readonly ImageSource scriptIcon = LoadIcon("document");
        static readonly ImageSource disabledIcon = LoadIcon("disabled");

        public event Action Reloaded;
        public event Action<string, string> Renamed;
        public event Action<string> Deleted;
        public event Action<string> ScriptSelected;
        public event EventHandler Modified;

        // スクリプトの項目のメニュー
        readonly ContextMenu contextMenuStrip1;
        readonly MenuItem 無効化MenuItem;
        readonly MenuItem 新規フォルダ作成MenuItem1;
        readonly MenuItem 削除MenuItem;
        readonly MenuItem 名前の変更MenuItem;
        // フォルダの項目のメニュー
        readonly ContextMenu contextMenuStrip2;
        readonly MenuItem 新規スクリプトの作成MenuItem;
        readonly MenuItem 新規フォルダ作成MenuItem;
        readonly MenuItem 削除MenuItem1;
        readonly MenuItem 名前の変更MenuItem1;

        Point? dragStartPoint;
        TreeViewItem dragStartItem;

        private TreeViewItem CommandNode
        {
            get
            {
                return (TreeViewItem)treeView1.Items[0];
            }
        }

        private TreeViewItem FunctionNode
        {
            get
            {
                return (TreeViewItem)treeView1.Items[1];
            }
        }

        private TreeViewItem ScriptNode
        {
            get
            {
                return (TreeViewItem)treeView1.Items[2];
            }
        }

        private TreeViewItem SelectedNode
        {
            get
            {
                return treeView1.SelectedItem as TreeViewItem;
            }
        }

        public string CurrentScriptPath
        {
            get
            {
                if (SelectedNode == null || IsFolder(SelectedNode))
                {
                    return null;
                }

                return GetFullPath(SelectedNode) + ".fsml";
            }
        }

        public bool ChildNodeSelected
        {
            get
            {
                return SelectedNode != CommandNode && SelectedNode != FunctionNode && SelectedNode != ScriptNode;
            }
        }

        public ScriptListPanel()
        {
            InitializeComponent();

            contextMenuStrip1 = new ContextMenu();
            無効化MenuItem = AddMenuItem(contextMenuStrip1, "無効化", (sender, e) => ToggleDisabled());
            新規フォルダ作成MenuItem1 = AddMenuItem(contextMenuStrip1, "新規フォルダ作成", (sender, e) => CreateNewFolder());
            削除MenuItem = AddMenuItem(contextMenuStrip1, "削除", (sender, e) => DeleteNode(SelectedNode));
            名前の変更MenuItem = AddMenuItem(contextMenuStrip1, "名前の変更", (sender, e) => BeginEdit());
            contextMenuStrip1.Opened += ContextMenuStrip1_Opened;

            contextMenuStrip2 = new ContextMenu();
            新規スクリプトの作成MenuItem = AddMenuItem(contextMenuStrip2, "新規スクリプトの作成", (sender, e) => CreateNewScript());
            新規フォルダ作成MenuItem = AddMenuItem(contextMenuStrip2, "新規フォルダ作成", (sender, e) => CreateNewFolder());
            削除MenuItem1 = AddMenuItem(contextMenuStrip2, "削除", (sender, e) => DeleteNode(SelectedNode));
            名前の変更MenuItem1 = AddMenuItem(contextMenuStrip2, "名前の変更", (sender, e) => BeginEdit());
            contextMenuStrip2.Opened += ContextMenuStrip2_Opened;

            CreateFolderNode(treeView1.Items, "Commands");
            CreateFolderNode(treeView1.Items, "Functions");
            CreateFolderNode(treeView1.Items, "Scripts");

            ReadCommands();
            ReadFunctions();
        }

        private static ImageSource LoadIcon(string name)
        {
            var image = new BitmapImage(new Uri(String.Format("pack://application:,,,/PPDEditor;component/img/{0}.png", name)));
            image.Freeze();
            return image;
        }

        private static MenuItem AddMenuItem(ContextMenu menu, string header, RoutedEventHandler click)
        {
            var item = new MenuItem { Header = header };
            item.Click += click;
            menu.Items.Add(item);
            return item;
        }

        public void SetLang()
        {
            toolStripButton1.ToolTip = Utility.Language["Update"];
            toolStripButton2.ToolTip = Utility.Language["CreateNewFolder"];
            this.削除MenuItem1.Header = this.削除MenuItem.Header = Utility.Language["Delete"];
            this.新規スクリプトの作成MenuItem.Header = Utility.Language["CreateNewScript"];
            this.新規フォルダ作成MenuItem1.Header = this.新規フォルダ作成MenuItem.Header = Utility.Language["CreateNewFolder"];
            this.名前の変更MenuItem.Header = this.名前の変更MenuItem1.Header = Utility.Language["Rename"];
        }

        private static ScriptListNode GetNode(TreeViewItem item)
        {
            return (ScriptListNode)item.Tag;
        }

        private static bool IsFolder(TreeViewItem item)
        {
            return GetNode(item).IsFolder;
        }

        private static ItemsControl GetParent(TreeViewItem item)
        {
            return ItemsControl.ItemsControlFromItemContainer(item);
        }

        /// <summary>
        /// 一番上からの名前を「\」でつないだもの（以前の TreeNode.FullPath と同じ）を返す。
        /// </summary>
        private static string GetTreePath(TreeViewItem item)
        {
            var names = new List<string>();
            for (ItemsControl current = item; current is TreeViewItem treeViewItem; current = GetParent(treeViewItem))
            {
                names.Insert(0, GetNode(treeViewItem).Name);
            }
            return String.Join("\\", names);
        }

        private TreeViewItem FindNodeByPath(string path)
        {
            ItemsControl current = treeView1;
            foreach (var name in path.Split('\\'))
            {
                var found = FindNodeByText(current.Items, name);
                if (found == null)
                {
                    return null;
                }
                current = found;
            }
            return current as TreeViewItem;
        }

        /// <summary>
        /// 名前を変える。Enter で決め、Esc でやめる。
        /// </summary>
        private void BeginEdit()
        {
            var item = SelectedNode;
            if (item == null || !ChildNodeSelected)
            {
                return;
            }
            var node = GetNode(item);
            node.NameEditor.Text = node.Name;
            node.NameText.Visibility = Visibility.Collapsed;
            node.NameEditor.Visibility = Visibility.Visible;
            node.NameEditor.Focus();
            node.NameEditor.SelectAll();
        }

        private void EndEdit(TreeViewItem item, bool commit)
        {
            var node = GetNode(item);
            if (node.NameEditor.Visibility != Visibility.Visible)
            {
                return;
            }
            node.NameEditor.Visibility = Visibility.Collapsed;
            node.NameText.Visibility = Visibility.Visible;
            if (commit)
            {
                Rename(item, node.NameEditor.Text);
            }
            item.Focus();
        }

        private void Rename(TreeViewItem item, string label)
        {
            if (String.IsNullOrEmpty(label) || label == GetNode(item).Name)
            {
                return;
            }

            if (label.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || label.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                return;
            }

            var beforePath = GetFullPath(item);
            var path = Path.Combine(GetFullPath((TreeViewItem)GetParent(item)), label);

            if (IsFolder(item))
            {
                if (Directory.Exists(path))
                {
                    MessageBox.Show(Window.GetWindow(this), Utility.Language["AlreadyExistName"]);
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
                    MessageBox.Show(Window.GetWindow(this), Utility.Language["AlreadyExistName"]);
                    return;
                }
                File.Move(beforePath, path);
            }
            var node = GetNode(item);
            node.Name = label;
            node.NameText.Text = label;
            Renamed?.Invoke(beforePath, path);
        }

        public void Clear()
        {
            ScriptNode.Items.Clear();
        }

        public string[] GetEnabledScriptList()
        {
            return GetList(Filter.Enabled);
        }

        public string GetDisabledScriptList()
        {
            var list = GetList(Filter.Disabled);
            return String.Join("|", list);
        }

        private string[] GetList(Filter filter)
        {
            var ret = new List<string>();
            var nodes = new Queue<TreeViewItem>();
            nodes.Enqueue(ScriptNode);
            while (nodes.Count > 0)
            {
                var node = nodes.Dequeue();
                foreach (TreeViewItem childNode in node.Items)
                {
                    if (IsFolder(childNode))
                    {
                        nodes.Enqueue(childNode);
                    }
                    else
                    {
                        var enabled = !GetNode(childNode).Disabled;
                        if (enabled)
                        {
                            if ((filter & Filter.Enabled) == Filter.Enabled)
                            {
                                ret.Add(GetTreePath(childNode));
                            }
                        }
                        else
                        {
                            if ((filter & Filter.Disabled) == Filter.Disabled)
                            {
                                ret.Add(GetTreePath(childNode));
                            }
                        }
                    }
                }
            }

            return [.. ret];
        }

        public void WriteScript(Stream stream, string scriptName)
        {
            var split = scriptName.Split('\\');
            TreeViewItem node = ScriptNode;
            for (int i = 1; i < split.Length; i++)
            {
                node = FindNodeByText(node.Items, split[i]);
                if (node == null)
                {
                    return;
                }
            }

            string path = GetFullPath(node) + ".fsml";
            using FileStream fs = File.Open(path, FileMode.Open);
            byte[] data = new byte[fs.Length];
            fs.ReadAtLeast(data, data.Length, throwOnEndOfStream: false);
            stream.Write(data, 0, data.Length);
        }

        public void ReadScript(string projectDirPath, AvailableDifficulty difficulty, string disableList)
        {
            var queue = new Queue<TreeViewItem>();
            queue.Enqueue(ScriptNode);
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                var dir = Path.Combine(projectDirPath, String.Format("{0}_{1}", difficulty, GetTreePath(node)));
                Utility.SafeCreateDirectory(dir);
                foreach (string childDir in Directory.GetDirectories(dir))
                {
                    queue.Enqueue(CreateFolderNode(node.Items, Path.GetFileName(childDir)));
                }

                foreach (string childFile in Directory.GetFiles(dir))
                {
                    if (Path.GetExtension(childFile).Equals(".fsml", StringComparison.CurrentCultureIgnoreCase))
                    {
                        CreateScriptNode(node.Items, Path.GetFileNameWithoutExtension(childFile));
                    }
                }
            }

            var disables = disableList.Split('|');
            foreach (string disable in disables)
            {
                var node = FindNodeByPath(disable);
                if (node != null && !IsFolder(node))
                {
                    SetDisabled(node, true);
                }
            }
        }

        private void ReadFunctions()
        {
            var dir = Path.Combine(Utility.AppDir, "Functions");
            if (!Directory.Exists(dir))
            {
                return;
            }
            else
            {
                Directory.CreateDirectory("Functions");
            }

            ReadNodes(FunctionNode.Items, dir);
        }

        private void ReadCommands()
        {
            var dir = Path.Combine(Utility.AppDir, "Commands");
            if (!Directory.Exists(dir))
            {
                return;
            }
            else
            {
                Directory.CreateDirectory("Commands");
            }

            ReadNodes(CommandNode.Items, dir);
        }

        private void ReadNodes(ItemCollection nodes, string dirPath)
        {
            foreach (string childDirPath in Directory.GetDirectories(dirPath))
            {
                var name = Path.GetFileNameWithoutExtension(childDirPath);
                var node = FindNodeByText(nodes, name);
                node ??= CreateFolderNode(nodes, name);
                ReadNodes(node.Items, childDirPath);
            }

            foreach (string childFilePath in Directory.GetFiles(dirPath))
            {
                if (Path.GetExtension(childFilePath).Equals(".fsml", StringComparison.CurrentCultureIgnoreCase))
                {
                    var name = Path.GetFileNameWithoutExtension(childFilePath);
                    var node = FindNodeByText(nodes, name);
                    if (node == null)
                    {
                        CreateScriptNode(nodes, name);
                    }
                }
            }

            // remove exist
            var removeNode = new List<TreeViewItem>();
            foreach (TreeViewItem node in nodes)
            {
                var path = Path.Combine(Utility.AppDir, GetTreePath(node));
                if (IsFolder(node))
                {
                    if (!Directory.Exists(path))
                    {
                        removeNode.Add(node);
                    }
                }
                else
                {
                    path = Path.ChangeExtension(path, ".fsml");
                    if (!File.Exists(path))
                    {
                        removeNode.Add(node);
                    }
                }
            }
            foreach (TreeViewItem node in removeNode)
            {
                nodes.Remove(node);
            }
        }

        private static TreeViewItem FindNodeByText(ItemCollection nodes, string text)
        {
            return nodes.OfType<TreeViewItem>().FirstOrDefault(node => GetNode(node).Name == text);
        }

        private void Reload()
        {
            ReadCommands();
            ReadFunctions();
            if (!WindowUtility.MainForm.IsProjectLoaded)
            {
                return;
            }

            var disableList = GetDisabledScriptList();
            ScriptNode.Items.Clear();
            ReadScript(WindowUtility.MainForm.CurrentProjectDir, WindowUtility.MainForm.CurrentDifficulty, disableList);

            Reloaded?.Invoke();
        }

        private void ToolStripButton1_Click(object sender, RoutedEventArgs e)
        {
            Reload();
        }

        private void ToolStripButton2_Click(object sender, RoutedEventArgs e)
        {
            CreateNewFolder();
        }

        private static TreeViewItem GetRootNode(TreeViewItem node)
        {
            while (GetParent(node) is TreeViewItem parent)
            {
                node = parent;
            }
            return node;
        }

        private void CreateNewFolder()
        {
            var selectedNode = SelectedNode;
            if (selectedNode == null)
            {
                return;
            }
            if (ContainAsChild(ScriptNode, selectedNode) && !WindowUtility.MainForm.IsProjectLoaded)
            {
                return;
            }

            var folderNode = IsFolder(selectedNode) ? selectedNode : (TreeViewItem)GetParent(selectedNode);
            var newFolder = GetSafeFolderName(GetFullPath(folderNode));

            Directory.CreateDirectory(newFolder);
            CreateFolderNode(folderNode.Items, Path.GetFileNameWithoutExtension(newFolder));
            selectedNode.IsExpanded = true;
            Modified?.Invoke(this, EventArgs.Empty);
        }

        private void CreateNewScript()
        {
            var selectedNode = SelectedNode;
            if (selectedNode == null || (ContainAsChild(ScriptNode, selectedNode) && !WindowUtility.MainForm.IsProjectLoaded))
            {
                return;
            }
            var newFile = GetSafeScriptName(GetFullPath(selectedNode));

            Utility.RecursiveCreateFolder(newFile);
            File.WriteAllText(newFile, FlowDrawPanel.EmptyFileString, Encoding.UTF8);
            CreateScriptNode(selectedNode.Items, Path.GetFileNameWithoutExtension(newFile));
            selectedNode.IsExpanded = true;
        }

        private static bool ContainAsChildOrSelf(TreeViewItem parent, TreeViewItem child)
        {
            return parent == child || ContainAsChild(parent, child);
        }

        private static bool ContainAsChild(TreeViewItem parent, TreeViewItem child)
        {
            for (var current = child; current != null; current = GetParent(current) as TreeViewItem)
            {
                if (GetParent(current) == parent)
                {
                    return true;
                }
            }
            return false;
        }

        private string GetFullPath(TreeViewItem node)
        {
            var rootNode = GetRootNode(node);
            if (rootNode == FunctionNode || rootNode == CommandNode)
            {
                return Path.Combine(Utility.AppDir, GetTreePath(node));
            }
            else
            {
                return Path.Combine(WindowUtility.MainForm.CurrentProjectDir, String.Format("{0}_{1}", WindowUtility.MainForm.CurrentDifficulty, GetTreePath(node)));
            }
        }

        private static string GetSafeScriptName(string dirPath)
        {
            var newPath = Path.Combine(dirPath, String.Format("New Script.fsml"));
            int iter = 1;
            while (File.Exists(newPath))
            {
                newPath = Path.Combine(dirPath, String.Format("New Script({0}).fsml", iter));
                iter++;
            }
            return newPath;
        }

        private static string GetSafeFolderName(string dirPath)
        {
            var newPath = Path.Combine(dirPath, String.Format("New Folder"));
            int iter = 1;
            while (Directory.Exists(newPath))
            {
                newPath = Path.Combine(dirPath, String.Format("New Folder({0})", iter));
                iter++;
            }
            return newPath;
        }

        private TreeViewItem CreateFolderNode(ItemCollection nodes, string name)
        {
            return AddNode(nodes, name, true, contextMenuStrip2);
        }

        private TreeViewItem CreateScriptNode(ItemCollection nodes, string name)
        {
            return AddNode(nodes, name, false, contextMenuStrip1);
        }

        /// <summary>
        /// アイコンと名前（名前を変えるときの入力欄も）を出す項目を加える。
        /// </summary>
        private TreeViewItem AddNode(ItemCollection nodes, string name, bool isFolder, ContextMenu contextMenu)
        {
            var icon = new Grid
            {
                Width = 16,
                Height = 16,
                Margin = new Thickness(0, 0, 3, 0)
            };
            icon.Children.Add(new Image { Source = isFolder ? folderIcon : scriptIcon });
            var disabledMark = new Image { Source = disabledIcon, Visibility = Visibility.Collapsed };
            icon.Children.Add(disabledMark);
            var nameText = new TextBlock { Text = name, VerticalAlignment = VerticalAlignment.Center };
            var nameEditor = new TextBox { MinWidth = 100, Visibility = Visibility.Collapsed };
            var header = new StackPanel { Orientation = Orientation.Horizontal };
            header.Children.Add(icon);
            header.Children.Add(nameText);
            header.Children.Add(nameEditor);
            var item = new TreeViewItem
            {
                Header = header,
                ContextMenu = contextMenu,
                Tag = new ScriptListNode
                {
                    IsFolder = isFolder,
                    Name = name,
                    NameText = nameText,
                    NameEditor = nameEditor,
                    DisabledMark = disabledMark
                }
            };
            nameEditor.KeyDown += (sender, e) =>
            {
                if (e.Key == Key.Return || e.Key == Key.Escape)
                {
                    e.Handled = true;
                    EndEdit(item, e.Key == Key.Return);
                }
            };
            nameEditor.LostKeyboardFocus += (sender, e) => EndEdit(item, true);
            nodes.Add(item);
            return item;
        }

        private static void SetDisabled(TreeViewItem item, bool disabled)
        {
            var node = GetNode(item);
            node.Disabled = disabled;
            node.DisabledMark.Visibility = disabled ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ContextMenuStrip1_Opened(object sender, RoutedEventArgs e)
        {
            無効化MenuItem.Header = GetNode(SelectedNode).Disabled ? Utility.Language["MakeValid"] : Utility.Language["MakeInvalid"];
            無効化MenuItem.IsEnabled = GetRootNode(SelectedNode) == ScriptNode;
            名前の変更MenuItem.IsEnabled = ChildNodeSelected;
        }

        private void ContextMenuStrip2_Opened(object sender, RoutedEventArgs e)
        {
            削除MenuItem1.IsEnabled = 名前の変更MenuItem1.IsEnabled = ChildNodeSelected;
        }

        private void ToggleDisabled()
        {
            if (SelectedNode != null)
            {
                SetDisabled(SelectedNode, !GetNode(SelectedNode).Disabled);
            }
        }

        private void TreeView1_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 右クリックした項目を選ぶ
            var item = GetItemAt(e.OriginalSource as DependencyObject);
            if (item != null)
            {
                item.IsSelected = true;
                item.Focus();
            }
        }

        private TreeViewItem GetItemAt(DependencyObject element)
        {
            for (; element != null && element != treeView1; element = VisualTreeHelper.GetParent(element))
            {
                if (element is TreeViewItem item)
                {
                    return item;
                }
            }
            return null;
        }

        private void DeleteNode(TreeViewItem node)
        {
            if (node == null)
            {
                return;
            }
            if (MessageBox.Show(Window.GetWindow(this), String.Format(Utility.Language["DeleteConfirmText"], GetTreePath(node)), Utility.Language["DeleteConfirm"], MessageBoxButton.OKCancel) != MessageBoxResult.OK)
            {
                return;
            }

            var path = GetFullPath(node);
            if (IsFolder(node))
            {
                Directory.Delete(path, true);
                Deleted?.Invoke(path);
            }
            else
            {
                path += ".fsml";
                File.Delete(path);
                Deleted?.Invoke(path);
            }
            GetParent(node).Items.Remove(node);
        }

        private void TreeView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.OriginalSource is TextBox || SelectedNode == null)
            {
                return;
            }
            if (e.Key == Key.F2)
            {
                BeginEdit();
                e.Handled = true;
            }
            else if (e.Key == Key.Return)
            {
                if (IsFolder(SelectedNode))
                {
                    return;
                }
                ScriptSelected?.Invoke(Path.ChangeExtension(GetFullPath(SelectedNode), ".fsml"));
                e.Handled = true;
            }
        }

        private void TreeView1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = GetItemAt(e.OriginalSource as DependencyObject);
            if (item == null || item != SelectedNode || IsFolder(item))
            {
                return;
            }
            ScriptSelected?.Invoke(Path.ChangeExtension(GetFullPath(item), ".fsml"));
        }

        private void TreeView1_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            dragStartPoint = e.GetPosition(treeView1);
            dragStartItem = GetItemAt(e.OriginalSource as DependencyObject);
        }

        private void TreeView1_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            // 関数を、譜面のスクリプトのフォルダへドラッグしてコピーできる
            if (e.LeftButton != MouseButtonState.Pressed || dragStartPoint == null || dragStartItem == null)
            {
                return;
            }
            var diff = e.GetPosition(treeView1) - dragStartPoint.Value;
            if (Math.Abs(diff.X) < SystemParameters.MinimumHorizontalDragDistance && Math.Abs(diff.Y) < SystemParameters.MinimumVerticalDragDistance)
            {
                return;
            }
            var item = dragStartItem;
            dragStartPoint = null;
            dragStartItem = null;
            if (!WindowUtility.MainForm.IsProjectLoaded || !ContainAsChild(FunctionNode, item))
            {
                return;
            }
            item.IsSelected = true;
            treeView1.Focus();
            DragDrop.DoDragDrop(treeView1, new DataObject(DragDataFormat, item), DragDropEffects.Copy);
        }

        private TreeViewItem GetDropTarget(DragEventArgs e, out TreeViewItem source)
        {
            source = e.Data.GetDataPresent(DragDataFormat) ? e.Data.GetData(DragDataFormat) as TreeViewItem : null;
            if (source == null || GetRootNode(source) != FunctionNode)
            {
                return null;
            }
            var target = GetItemAt(e.OriginalSource as DependencyObject);
            if (target == null || target == source || !ContainAsChildOrSelf(ScriptNode, target))
            {
                return null;
            }
            return target;
        }

        private void TreeView1_DragOver(object sender, DragEventArgs e)
        {
            var target = GetDropTarget(e, out _);
            if (target == null)
            {
                e.Effects = DragDropEffects.None;
            }
            else
            {
                e.Effects = DragDropEffects.Copy;
                target.IsSelected = true;
            }
            e.Handled = true;
        }

        private void TreeView1_Drop(object sender, DragEventArgs e)
        {
            var target = GetDropTarget(e, out var source);
            e.Handled = true;
            if (target == null)
            {
                e.Effects = DragDropEffects.None;
                return;
            }
            if (!IsFolder(target))
            {
                target = (TreeViewItem)GetParent(target);
            }

            // copy files
            var sourcePath = Path.Combine(Utility.AppDir, GetTreePath(source));
            var destPath = Path.Combine(WindowUtility.MainForm.CurrentProjectDir, String.Format("{0}_{1}",
                WindowUtility.MainForm.CurrentDifficulty, GetTreePath(target)), GetNode(source).Name);
            if (Directory.Exists(sourcePath))
            {
                Utility.CopyDirectory(sourcePath, destPath, null,
                    fileName => MessageBox.Show(Window.GetWindow(this), String.Format(Utility.Language["OverwriteConfirm"] + "\n{0}", fileName),
                        Utility.Language["Confirm"], MessageBoxButton.OKCancel) == MessageBoxResult.OK);
                Reload();
            }
            else
            {
                sourcePath += ".fsml";
                destPath += ".fsml";
                bool copy = true;
                if (File.Exists(destPath))
                {
                    copy &= MessageBox.Show(Window.GetWindow(this), String.Format(Utility.Language["OverwriteConfirm"] + "\n{0}", destPath),
                        Utility.Language["Confirm"], MessageBoxButton.OKCancel) == MessageBoxResult.OK;
                }
                if (copy)
                {
                    File.Copy(sourcePath, destPath, true);
                    Reload();
                }
            }
        }
    }
}
