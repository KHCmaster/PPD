using FlowScriptControl.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace PPDEditor.DockForm.Script
{
    public partial class ScriptListDockForm : DockContent
    {
        [Flags]
        enum Filter
        {
            None = 0,
            Enabled = 1,
            Disabled = 2
        }

        public event Action Reloaded;
        public event Action<string, string> Renamed;
        public event Action<string> Deleted;
        public event Action<string> ScriptSelected;
        public event EventHandler Modified;

        public TreeNode CommandNode
        {
            get
            {
                return treeView1.Nodes[0];
            }
        }

        public TreeNode FunctionNode
        {
            get
            {
                return treeView1.Nodes[1];
            }
        }

        public TreeNode ScriptNode
        {
            get
            {
                return treeView1.Nodes[2];
            }
        }

        public string CurrentScriptPath
        {
            get
            {
                if (treeView1.SelectedNode == null || treeView1.SelectedNode.Name == "folder")
                {
                    return null;
                }

                return GetFullPath(treeView1.SelectedNode) + ".fsml";
            }
        }

        public bool ChildNodeSelected
        {
            get
            {
                return treeView1.SelectedNode != CommandNode && treeView1.SelectedNode != FunctionNode && treeView1.SelectedNode != ScriptNode;
            }
        }

        public ScriptListDockForm()
        {
            InitializeComponent();

            treeView1.ImageList = new ImageList
            {
                ColorDepth = ColorDepth.Depth32Bit
            };

            treeView1.ImageList.Images.Add("folder", PPDEditor.Properties.Resources.folder);
            treeView1.ImageList.Images.Add("script", PPDEditor.Properties.Resources.document);
            treeView1.ImageList.Images.Add("disabled", PPDEditor.Properties.Resources.disabled);

            Win32.ImageList_SetOverlayImage(treeView1.ImageList.Handle, 2, 1);

            treeView1.Nodes.Add("folder", "Commands").ContextMenuStrip = contextMenuStrip2;
            treeView1.Nodes.Add("folder", "Functions").ContextMenuStrip = contextMenuStrip2;
            treeView1.Nodes.Add("folder", "Scripts").ContextMenuStrip = contextMenuStrip2;

            treeView1.BeforeLabelEdit += treeView1_BeforeLabelEdit;
            treeView1.AfterLabelEdit += treeView1_AfterLabelEdit;

            ReadCommands();
            ReadFunctions();
        }

        public void SetLang()
        {
            toolStripButton1.ToolTipText = Utility.Language["Update"];
            toolStripButton2.ToolTipText = Utility.Language["CreateNewFolder"];
            this.削除ToolStripMenuItem1.Text = this.削除ToolStripMenuItem.Text = Utility.Language["Delete"];
            this.新規スクリプトの作成ToolStripMenuItem.Text = Utility.Language["CreateNewScript"];
            this.新規フォルダ作成ToolStripMenuItem1.Text = this.新規フォルダ作成ToolStripMenuItem.Text = Utility.Language["CreateNewFolder"];
            this.名前の変更ToolStripMenuItem.Text = this.名前の変更ToolStripMenuItem1.Text = Utility.Language["Rename"];
        }

        void treeView1_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            treeView1.LabelEdit = false;
            if (String.IsNullOrEmpty(e.Label))
            {
                e.CancelEdit = true;
                return;
            }

            foreach (char c in Path.GetInvalidFileNameChars())
            {
                if (e.Label.IndexOf(c) >= 0)
                {
                    e.CancelEdit = true;
                    return;
                }
            }

            foreach (char c in Path.GetInvalidPathChars())
            {
                if (e.Label.IndexOf(c) >= 0)
                {
                    e.CancelEdit = true;
                    return;
                }
            }

            var beforePath = GetFullPath(e.Node);
            var path = Path.Combine(GetFullPath(e.Node.Parent), e.Label);

            if (e.Node.Name == "folder")
            {
                if (Directory.Exists(path))
                {
                    MessageBox.Show(Utility.Language["AlreadyExistName"]);
                    e.CancelEdit = true;
                }
                else
                {
                    Directory.Move(beforePath, path);
                    Renamed(beforePath, path);
                }
            }
            else
            {
                path += ".fsml";
                beforePath += ".fsml";
                if (File.Exists(path))
                {
                    MessageBox.Show(Utility.Language["AlreadyExistName"]);
                    e.CancelEdit = true;
                }
                else
                {
                    File.Move(beforePath, path);
                    Renamed(beforePath, path);
                }
            }
        }

        void treeView1_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            e.CancelEdit = e.Node == CommandNode || e.Node == FunctionNode || e.Node == ScriptNode;
        }

        public void Clear()
        {
            ScriptNode.Nodes.Clear();
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
            var nodes = new Queue<TreeNode>();
            nodes.Enqueue(ScriptNode);
            while (nodes.Count > 0)
            {
                var node = nodes.Dequeue();
                foreach (TreeNode childNode in node.Nodes)
                {
                    if (childNode.Name == "folder")
                    {
                        nodes.Enqueue(childNode);
                    }
                    else
                    {
                        var enabled = EnabledNode(childNode);
                        if (enabled)
                        {
                            if ((filter & Filter.Enabled) == Filter.Enabled)
                            {
                                ret.Add(childNode.FullPath);
                            }
                        }
                        else
                        {
                            if ((filter & Filter.Disabled) == Filter.Disabled)
                            {
                                ret.Add(childNode.FullPath);
                            }
                        }
                    }
                }
            }

            return ret.ToArray();
        }

        public void WriteScript(Stream stream, string scriptName)
        {
            var split = scriptName.Split('\\');
            TreeNode node = ScriptNode;
            for (int i = 1; i < split.Length; i++)
            {
                bool found = false;
                foreach (TreeNode childNode in node.Nodes)
                {
                    if (childNode.Text == split[i])
                    {
                        node = childNode;
                        found = true;
                        break;
                    }
                }

                if (found)
                {
                    continue;
                }
                else
                {
                    return;
                }
            }

            string path = GetFullPath(node) + ".fsml";
            using (FileStream fs = File.Open(path, FileMode.Open))
            {
                byte[] data = new byte[fs.Length];
                fs.Read(data, 0, data.Length);
                stream.Write(data, 0, data.Length);
            }
        }

        public void ReadScript(string projectDirPath, AvailableDifficulty difficulty, string disableList)
        {
            var queue = new Queue<TreeNode>();
            queue.Enqueue(ScriptNode);
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                var dir = Path.Combine(projectDirPath, String.Format("{0}_{1}", difficulty, node.FullPath));
                Utility.SafeCreateDirectory(dir);
                foreach (string childDir in Directory.GetDirectories(dir))
                {
                    queue.Enqueue(CreateFolderNode(node.Nodes, Path.GetFileName(childDir)));
                }

                foreach (string childFile in Directory.GetFiles(dir))
                {
                    if (Path.GetExtension(childFile).ToLower() == ".fsml")
                    {
                        CreateScriptNode(node.Nodes, Path.GetFileNameWithoutExtension(childFile));
                    }
                }
            }

            var disables = disableList.Split('|');
            foreach (string disable in disables)
            {
                var node = Utility.GetTreeNodeFromPath(treeView1.Nodes, disable);
                if (node != null)
                {
                    Win32.SetTreeViewOverlay(node, 1);
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

            ReadNodes(FunctionNode.Nodes, dir);
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

            ReadNodes(CommandNode.Nodes, dir);
        }

        private void ReadNodes(TreeNodeCollection nodes, string dirPath)
        {
            foreach (string childDirPath in Directory.GetDirectories(dirPath))
            {
                var name = Path.GetFileNameWithoutExtension(childDirPath);
                var node = FindNodeByText(nodes, name);
                if (node == null)
                {
                    node = CreateFolderNode(nodes, name);
                }
                ReadNodes(node.Nodes, childDirPath);
            }

            foreach (string childFilePath in Directory.GetFiles(dirPath))
            {
                if (Path.GetExtension(childFilePath).ToLower() == ".fsml")
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
            var removeNode = new List<TreeNode>();
            foreach (TreeNode node in nodes)
            {
                var path = Path.Combine(Utility.AppDir, node.FullPath);
                if (node.ImageKey == "folder")
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
            foreach (TreeNode node in removeNode)
            {
                node.Remove();
            }
        }

        private TreeNode FindNodeByText(TreeNodeCollection nodes, string text)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Text == text)
                {
                    return node;
                }
            }

            return null;
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
            ScriptNode.Nodes.Clear();
            ReadScript(WindowUtility.MainForm.CurrentProjectDir, WindowUtility.MainForm.CurrentDifficulty, disableList);

            OnReloaded();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Reload();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            CreateNewFolder();
        }

        private TreeNode GetRootNode(TreeNode node)
        {
            while (node.Parent != null)
            {
                node = node.Parent;
            }
            return node;
        }

        private void CreateNewFolder()
        {
            if (treeView1.SelectedNode == null)
            {
                return;
            }
            if (ContainAsChild(ScriptNode, treeView1.SelectedNode) && !WindowUtility.MainForm.IsProjectLoaded)
            {
                return;
            }

            TreeNodeCollection coll = treeView1.SelectedNode.Name == "folder" ? treeView1.SelectedNode.Nodes : treeView1.SelectedNode.Parent.Nodes;
            var newFolder = GetSafeFolderName(GetFullPath(treeView1.SelectedNode.Name == "folder" ? treeView1.SelectedNode : treeView1.SelectedNode.Parent));

            Directory.CreateDirectory(newFolder);
            CreateFolderNode(coll, Path.GetFileNameWithoutExtension(newFolder));
            if (!treeView1.SelectedNode.IsExpanded)
            {
                treeView1.SelectedNode.Expand();
            }
            OnModified(this, EventArgs.Empty);
        }

        private bool IsFolder(TreeNode node)
        {
            return node.Name == "folder";
        }

        private bool ContainAsChildOrSelf(TreeNode parent, TreeNode child)
        {
            return parent == child || ContainAsChild(parent, child);
        }

        private bool ContainAsChild(TreeNode parent, TreeNode child)
        {
            var queue = new Queue<TreeNode>();
            queue.Enqueue(parent);
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                foreach (TreeNode childNode in node.Nodes)
                {
                    if (childNode == child)
                    {
                        return true;
                    }
                    queue.Enqueue(childNode);
                }
            }

            return false;
        }

        private void 新規スクリプトの作成ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ContainAsChild(ScriptNode, treeView1.SelectedNode) && !WindowUtility.MainForm.IsProjectLoaded)
            {
                return;
            }
            var newFile = GetSafeScriptName(GetFullPath(treeView1.SelectedNode));

            Utility.RecursiveCreateFolder(newFile);
            File.WriteAllText(newFile, FlowDrawPanel.EmptyFileString, Encoding.UTF8);
            CreateScriptNode(treeView1.SelectedNode.Nodes, Path.GetFileNameWithoutExtension(newFile));
            if (!treeView1.SelectedNode.IsExpanded)
            {
                treeView1.SelectedNode.Expand();
            }
        }

        private string GetFullPath(TreeNode node)
        {
            var rootNode = GetRootNode(node);
            if (rootNode == FunctionNode || rootNode == CommandNode)
            {
                return Path.Combine(Utility.AppDir, node.FullPath);
            }
            else
            {
                return Path.Combine(WindowUtility.MainForm.CurrentProjectDir, String.Format("{0}_{1}", WindowUtility.MainForm.CurrentDifficulty, node.FullPath));
            }
        }

        private string GetSafeScriptName(string dirPath)
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

        private string GetSafeFolderName(string dirPath)
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

        private TreeNode CreateFolderNode(TreeNodeCollection nodes, string name)
        {
            var node = nodes.Add("folder", name);
            node.ImageKey = "folder";
            node.SelectedImageKey = "folder";
            node.ContextMenuStrip = contextMenuStrip2;
            return node;
        }

        private TreeNode CreateScriptNode(TreeNodeCollection nodes, string name)
        {
            var node = nodes.Add("script", name);
            node.ImageKey = "script";
            node.SelectedImageKey = "script";
            node.ContextMenuStrip = contextMenuStrip1;
            return node;
        }

        private void 新規フォルダ作成ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateNewFolder();
        }

        private void 新規フォルダ作成ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            CreateNewFolder();
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            無効化ToolStripMenuItem.Text = Win32.GetTreeViewOverlay(treeView1.SelectedNode) == 0 ? Utility.Language["MakeInvalid"] : Utility.Language["MakeValid"];
            無効化ToolStripMenuItem.Enabled = GetRootNode(treeView1.SelectedNode) == ScriptNode;
            名前の変更ToolStripMenuItem.Enabled = ChildNodeSelected;
        }

        private void contextMenuStrip2_Opening(object sender, CancelEventArgs e)
        {
            削除ToolStripMenuItem1.Enabled = 名前の変更ToolStripMenuItem1.Enabled = ChildNodeSelected;
        }

        private void 無効化ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Win32.SetTreeViewOverlay(treeView1.SelectedNode, EnabledNode(treeView1.SelectedNode) ? (uint)1 : 0);
        }

        private bool EnabledNode(TreeNode node)
        {
            return Win32.GetTreeViewOverlay(node) == 0;
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            treeView1.SelectedNode = e.Node;
        }

        private void DeleteNode(TreeNode node)
        {
            if (MessageBox.Show(String.Format(Utility.Language["DeleteConfirmText"], node.FullPath), Utility.Language["DeleteConfirm"], MessageBoxButtons.OKCancel) != DialogResult.OK)
            {
                return;
            }

            var path = GetFullPath(node);
            if (node.Name == "folder")
            {
                Directory.Delete(path, true);
                Deleted(path);
            }
            else
            {
                path += ".fsml";
                File.Delete(path);
                Deleted(path);
            }
            node.Remove();
        }

        private void 削除ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DeleteNode(treeView1.SelectedNode);
        }

        private void 削除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteNode(treeView1.SelectedNode);
        }

        private void OnModified(object sender, EventArgs e)
        {
            Modified?.Invoke(sender, e);
        }

        private void OnDeleted(string deletedName)
        {
            Deleted?.Invoke(deletedName);
        }

        private void OnRenamed(string beforeName, string newName)
        {
            Renamed?.Invoke(beforeName, newName);
            OnModified(this, EventArgs.Empty);
        }

        private void OnReloaded()
        {
            Reloaded?.Invoke();
        }

        private void 名前の変更ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BeginEdit();
        }

        private void 名前の変更ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            BeginEdit();
        }

        private void BeginEdit()
        {
            if (treeView1.SelectedNode != null)
            {
                treeView1.LabelEdit = true;
                treeView1.SelectedNode.BeginEdit();
            }
        }

        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2)
            {
                BeginEdit();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                if (treeView1.SelectedNode.Name == "folder")
                {
                    return;
                }
                ScriptSelected?.Invoke(Path.ChangeExtension(GetFullPath(treeView1.SelectedNode), ".fsml"));
            }
        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (treeView1.SelectedNode.Name == "folder")
            {
                return;
            }
            ScriptSelected?.Invoke(Path.ChangeExtension(GetFullPath(treeView1.SelectedNode), ".fsml"));
        }

        private void treeView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            var treeView = (TreeView)sender;
            var treeNode = (TreeNode)e.Item;
            if (!WindowUtility.MainForm.IsProjectLoaded || !ContainAsChild(FunctionNode, treeNode))
            {
                return;
            }
            treeView.SelectedNode = treeNode;
            treeView.Focus();
            var effect = treeView.DoDragDrop(e.Item, DragDropEffects.Copy);
        }

        private void treeView1_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(TreeNode)))
            {
                var treeNode = (TreeNode)e.Data.GetData(typeof(TreeNode));
                if (treeNode.TreeView != treeView1)
                {
                    e.Effect = DragDropEffects.None;
                }
                else
                {
                    e.Effect = DragDropEffects.Copy;
                }
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }

            if (e.Effect != DragDropEffects.None)
            {
                var treeView = (TreeView)sender;
                var target = treeView.GetNodeAt(treeView.PointToClient(new Point(e.X, e.Y)));
                var source = (TreeNode)e.Data.GetData(typeof(TreeNode));
                if (target != null && target != source && ContainAsChildOrSelf(ScriptNode, target))
                {
                    if (target.IsSelected == false)
                    {
                        treeView.SelectedNode = target;
                    }
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
            }
        }

        private void treeView1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(TreeNode)))
            {
                var treeView = (TreeView)sender;
                var source = (TreeNode)e.Data.GetData(typeof(TreeNode));
                var target = treeView.GetNodeAt(treeView.PointToClient(new Point(e.X, e.Y)));
                if (!IsFolder(target))
                {
                    target = target.Parent;
                }
                if (target != null && target != source && ContainAsChildOrSelf(ScriptNode, target))
                {
                    // copy files
                    var sourcePath = Path.Combine(Utility.AppDir, source.FullPath);
                    var destPath = Path.Combine(WindowUtility.MainForm.CurrentProjectDir, String.Format("{0}_{1}",
                        WindowUtility.MainForm.CurrentDifficulty, target.FullPath), source.Text);
                    if (Directory.Exists(sourcePath))
                    {
                        Utility.CopyDirectory(sourcePath, destPath, null,
                            fileName => MessageBox.Show(String.Format(Utility.Language["OverwriteConfirm"] + "\n{0}", fileName),
                                Utility.Language["Confirm"], MessageBoxButtons.OKCancel) == DialogResult.OK);
                        Reload();
                    }
                    else
                    {
                        sourcePath += ".fsml";
                        destPath += ".fsml";
                        bool copy = true;
                        if (File.Exists(destPath))
                        {
                            copy &= MessageBox.Show(String.Format(Utility.Language["OverwriteConfirm"] + "\n{0}", destPath),
                                Utility.Language["Confirm"], MessageBoxButtons.OKCancel) == DialogResult.OK;
                        }
                        if (copy)
                        {
                            File.Copy(sourcePath, destPath, true);
                            Reload();
                        }
                    }
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }
    }
}
