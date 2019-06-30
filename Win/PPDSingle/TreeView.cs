using System.Collections.Generic;

namespace PPDSingle
{
    public delegate void SelectionEventHandler(object sender, SelectionEventArgs args);

    public class SelectionEventArgs
    {
        public SelectionEventArgs(TreeViewItem prevItem, TreeViewItem newItem)
        {
            Prev = prevItem;
            New = newItem;
        }

        public TreeViewItem Prev
        {
            get;
            private set;
        }

        public TreeViewItem New
        {
            get;
            private set;
        }
    }

    public class TreeView
    {
        public event SelectionEventHandler SelectionChanged;
        TreeViewItem root;
        public TreeView(TreeViewItem root)
        {
            this.root = root;
            root.TreeView = this;
        }

        public List<TreeViewItem> Items
        {
            get
            {
                return root.Items;
            }
        }

        public int ItemCount
        {
            get
            {
                return root.ItemCount;
            }
        }

        public TreeViewItem Root
        {
            get
            {
                return root;
            }
        }

        public TreeViewItem SelectedItem
        {
            get;
            private set;
        }

        public void Select(TreeViewItem item)
        {
            if (item == root || item == null)
            {
                InnerSelect(null);
                return;
            }
            if (Contain(item))
            {
                InnerSelect(item);
            }
        }

        private void InnerSelect(TreeViewItem item)
        {
            TreeViewItem prev = SelectedItem;
            SelectedItem = item;
            OnSelectionChanged(prev, item);
        }

        public bool Contain(TreeViewItem item)
        {
            return root.Contain(item);
        }

        public bool MoveToNext()
        {
            if (SelectedItem == null)
            {
                if (root.ItemCount > 0)
                {
                    InnerSelect(root.Items[0]);
                    return true;
                }
                return false;
            }
            var tvi = SelectedItem.GetNext();
            if (tvi != null)
            {
                InnerSelect(tvi);
                return true;
            }
            return false;
        }

        public bool MoveToPrevious()
        {
            if (SelectedItem == null)
            {
                if (root.ItemCount > 0)
                {
                    InnerSelect(root.Items[root.ItemCount - 1].GetLast());
                    return true;
                }
                return false;
            }
            var tvi = SelectedItem.GetPrevious();
            if (tvi != null && tvi != root)
            {
                InnerSelect(tvi);
                return true;
            }
            return false;
        }

        protected void OnSelectionChanged(TreeViewItem prevItem, TreeViewItem newItem)
        {
            if (SelectionChanged != null)
            {
                SelectionChanged.Invoke(this, new SelectionEventArgs(prevItem, newItem));
            }
        }
    }
}
