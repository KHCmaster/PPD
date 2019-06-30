using FlowScriptControl.Classes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace FlowScriptControl.Controls
{
    public partial class FlowTreeView : UserControl
    {
        private const string flowSourceKey = "FLOWSOURCE";
        private const string folderKey = "FOLDER";

        private ToolTip tooltip;
        private Timer timer;
        private CustomTreeNode lastHoverNode;
        private List<RemovedTreeNode> removedNodes = new List<RemovedTreeNode>();

        public NodeFilterInfoBase SelectedFilter
        {
            get { return this.toolStripComboBox1.SelectedItem as NodeFilterInfoBase; }
        }

        public event EventHandler SelectedFilterChanged;

        public FlowTreeView()
        {
            InitializeComponent();

            tooltip = new ToolTip();

            timer = new Timer
            {
                Interval = 1000
            };
            timer.Tick += timer_Tick;

            treeView1.ImageList = new ImageList
            {
                ColorDepth = ColorDepth.Depth32Bit
            };
            treeView1.ImageList.Images.Add(flowSourceKey, FlowScriptControl.Properties.Resources.flowsource);
            treeView1.ImageList.Images.Add(folderKey, FlowScriptControl.Properties.Resources.folder);

            treeView1.ItemDrag += treeView1_ItemDrag;
            treeView1.DragOver += treeView1_DragOver;
            treeView1.MouseMove += treeView1_MouseMove;
            treeView1.MouseLeave += treeView1_MouseLeave;
            treeView1.KeyDown += treeView1_KeyDown;
        }

        internal void ReprecateMenuToTree(CustomToolStripMenuItem menu)
        {
            ReprecateMenuToTree(treeView1.Nodes, menu);
        }

        internal void SetFilters(NodeFilterInfoBase[] filters)
        {
            toolStripComboBox1.ComboBox.Items.AddRange(filters);
            toolStripComboBox1.ComboBox.DisplayMember = "Name";
            if (toolStripComboBox1.Items.Count > 0)
            {
                toolStripComboBox1.SelectedIndex = 0;
            }
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Filter();
            SelectedFilterChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ReprecateMenuToTree(TreeNodeCollection collection, CustomToolStripMenuItem menu)
        {
            if (menu.IsFolder)
            {
                var treeNode = new CustomTreeNode(menu.CustomToolTipText, menu.Text);
                this.AddFolder(collection, treeNode);
                foreach (CustomToolStripMenuItem childMenu in menu.DropDownItems)
                {
                    ReprecateMenuToTree(treeNode.Nodes, childMenu);
                }
            }
            else
            {
                var treeNode = new CustomTreeNode(menu.Dumper, menu.Text);
                this.AddFlow(collection, treeNode);
                if (menu.DropDownItems.Count > 0)
                {
                    foreach (CustomToolStripMenuItem childMenu in menu.DropDownItems)
                    {
                        var childNode = new CustomTreeNode(childMenu.Dumper, childMenu.Text);
                        this.AddFlow(treeNode.Nodes, childNode);
                    }
                }
            }
        }

        private void AddFolder(TreeNodeCollection collection, CustomTreeNode node)
        {
            node.Name = node.Text;
            node.ImageKey = folderKey;
            node.SelectedImageKey = folderKey;
            collection.Add(node);
        }

        private void AddFlow(TreeNodeCollection collection, CustomTreeNode node)
        {
            node.Name = node.Text;
            node.ImageKey = flowSourceKey;
            node.SelectedImageKey = flowSourceKey;
            collection.Add(node);
        }

        private TreeNode FindNode(TreeNodeCollection nodes, string name)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Name == name)
                {
                    return node;
                }
            }
            return null;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (lastHoverNode != null)
            {
                timer.Stop();
                var form = this.FindForm();
                if (form != null)
                {
                    tooltip.Hide(form);
                    tooltip.Show(lastHoverNode.CustomToolTipText.ToolTipText, form, form.PointToClient(new Point(Cursor.Position.X + 15, Cursor.Position.Y + 15)));
                }
            }
        }

        private void HideToolTip()
        {
            var form = this.FindForm();
            if (form != null && tooltip != null)
            {
                tooltip.Hide(form);
            }
        }

        private void Filter()
        {
            Filter(toolStripTextBox1.Text, SelectedFilter);
        }

        private void Filter(string filter, NodeFilterInfoBase filterInfo)
        {
            RestoreNodes();
            var filters = filter.ToLower().Split('.');
            HideNodes(treeView1.Nodes, filterInfo);
            HideNodes(treeView1.Nodes, filters, 0, !String.IsNullOrEmpty(filter));
        }

        private bool HideNodes(TreeNodeCollection nodes, NodeFilterInfoBase filterInfo)
        {
            bool hasVisibleNodes = false;
            var nodesToRemove = new List<TreeNode>();
            foreach (CustomTreeNode node in nodes)
            {
                if (node.Nodes.Count > 0)
                {
                    var childrenIsVisible = HideNodes(node.Nodes, filterInfo);
                    if (!childrenIsVisible)
                    {
                        nodesToRemove.Add(node);
                    }
                    hasVisibleNodes |= childrenIsVisible;
                }
                else
                {
                    if (!IsFilteredNode(node, filterInfo))
                    {
                        nodesToRemove.Add(node);
                    }
                    else
                    {
                        hasVisibleNodes = true;
                    }
                }
            }

            foreach (TreeNode node in nodesToRemove)
            {
                removedNodes.Add(new RemovedTreeNode { RemovedNode = node, ParentNode = node.Parent, RemovedNodeIndex = node.Index });
                node.Remove();
            }
            return hasVisibleNodes;
        }

        private bool HideNodes(TreeNodeCollection nodes, string[] filters, int filterIndex, bool expandOrCollapse)
        {
            bool hasVisibleNodes = false;
            var nodesToRemove = new List<TreeNode>();
            foreach (CustomTreeNode node in nodes)
            {
                Action<int> action = (nextIndex) =>
                {
                    var childrenIsVisible = HideNodes(node.Nodes, filters, nextIndex, expandOrCollapse);
                    if (!childrenIsVisible)
                    {
                        if (expandOrCollapse)
                        {
                            node.Collapse();
                        }
                        nodesToRemove.Add(node);
                    }
                    else
                    {
                        if (expandOrCollapse)
                        {
                            node.Expand();
                        }
                    }
                    hasVisibleNodes |= childrenIsVisible;
                };
                if (IsFilteredNode(node, filters[filterIndex]))
                {
                    if (filterIndex == filters.Length - 1)
                    {
                        if (expandOrCollapse)
                        {
                            node.Expand();
                        }
                        hasVisibleNodes = true;
                    }
                    else
                    {
                        action(filterIndex + 1);
                    }
                }
                else
                {
                    action(0);
                }
            }

            foreach (TreeNode node in nodesToRemove)
            {
                removedNodes.Add(new RemovedTreeNode { RemovedNode = node, ParentNode = node.Parent, RemovedNodeIndex = node.Index });
                node.Remove();
            }
            return hasVisibleNodes;
        }

        private bool IsFilteredNode(CustomTreeNode node, NodeFilterInfoBase filter)
        {
            if (node.Dumper == null)
            {
                return true;
            }

            return !filter.IsHide(node.Dumper);
        }

        private bool IsFilteredNode(CustomTreeNode node, string filter)
        {
            if (node.Text.ToLower().Contains(filter))
            {
                return true;
            }

            var customTreeNode = node as CustomTreeNode;
            if (customTreeNode == null || customTreeNode.Dumper == null)
            {
                return false;
            }

            foreach (var item in customTreeNode.Dumper.InEvents.Concat(customTreeNode.Dumper.OutEvents).Concat(
                customTreeNode.Dumper.InProperties).Concat(customTreeNode.Dumper.OutProperties))
            {
                if (item.Name.ToLower().Contains(filter))
                {
                    return true;
                }
            }
            return false;
        }

        private void RestoreNodes()
        {
            removedNodes.Reverse();
            foreach (RemovedTreeNode removedNode in removedNodes)
            {
                if (removedNode.ParentNode == null)
                {
                    treeView1.Nodes.Insert(removedNode.RemovedNodeIndex, removedNode.RemovedNode);
                }
                else
                {
                    removedNode.ParentNode.Nodes.Insert(removedNode.RemovedNodeIndex, removedNode.RemovedNode);
                }
            }

            removedNodes.Clear();
        }

        private void toolStripTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Filter();
                e.Handled = true;
            }
        }

        private void toolStripTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
            }
        }

        private void treeView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (e.Item is CustomTreeNode treeNode)
                {
                    var dde = treeView1.DoDragDrop(e.Item, DragDropEffects.Copy);
                }
            }
        }

        private void treeView1_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(CustomTreeNode)))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void treeView1_MouseLeave(object sender, EventArgs e)
        {
            timer.Stop();
            HideToolTip();
        }

        private void treeView1_MouseMove(object sender, MouseEventArgs e)
        {
            var node = treeView1.GetNodeAt(new Point(e.X, e.Y)) as CustomTreeNode;
            if (node != lastHoverNode)
            {
                HideToolTip();
                timer.Stop();
                timer.Start();
                lastHoverNode = node;
            }
        }

        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F && e.Control)
            {
                toolStripTextBox1.Focus();
            }
        }

        class RemovedTreeNode
        {
            public TreeNode RemovedNode { get; set; }
            public int RemovedNodeIndex { get; set; }
            public TreeNode ParentNode { get; set; }
        }
    }
}
