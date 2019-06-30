using System;
using System.Collections.Generic;

namespace PPDSingle
{
    public abstract class TreeViewItem
    {
        class TreeViewItemComparer : IComparer<TreeViewItem>
        {
            static TreeViewItemComparer comparer = new TreeViewItemComparer();

            public static TreeViewItemComparer Comparer
            {
                get
                {
                    return comparer;
                }
            }

            #region IComparer<TreeViewItem> メンバ

            public int Compare(TreeViewItem x, TreeViewItem y)
            {
                if (x.IsFolder)
                {
                    if (y.IsFolder)
                    {
                        return StringComparer.InvariantCulture.Compare(x.Text, y.Text);
                    }
                    else
                    {
                        return -1;
                    }
                }
                else
                {
                    if (y.IsFolder)
                    {
                        return 1;
                    }
                    else
                    {
                        return StringComparer.InvariantCulture.Compare(x.Text, y.Text);
                    }
                }
            }

            #endregion
        }

        List<TreeViewItem> items;
        protected TreeViewItem()
        {
            items = new List<TreeViewItem>();
        }

        public TreeViewItem Parent
        {
            get;
            private set;
        }

        public TreeView TreeView
        {
            get;
            set;
        }

        public bool IsExpanded
        {
            get;
            private set;
        }

        public void ExpandOrShrink()
        {
            if (IsExpanded)
            {
                Shrink();
            }
            else
            {
                Expand();
            }
        }

        public void Expand()
        {
            IsExpanded |= ItemCount > 0;
        }

        public void Shrink()
        {
            IsExpanded = false;
        }

        public List<TreeViewItem> Items
        {
            get
            {
                return items;
            }
        }

        public int ItemCount
        {
            get
            {
                return items.Count;
            }
        }

        public void Add(TreeViewItem item)
        {
            SetInfo(item);
            items.Add(item);
            if (item.TreeView != null)
            {
                item.TreeView.Select(item);
            }
        }

        public void AddRange(TreeViewItem[] item)
        {
            foreach (TreeViewItem tvi in item)
            {
                SetInfo(tvi);
            }
            items.AddRange(item);
            if (item[item.Length - 1].TreeView != null)
            {
                item[item.Length - 1].TreeView.Select(item[item.Length - 1]);
            }
        }

        public void Remove()
        {
            if (Parent != null)
            {
                if (TreeView != null && TreeView.SelectedItem == this)
                {
                    int index = Parent.items.IndexOf(this) + 1;
                    if (index >= Parent.ItemCount) index -= 2;
                    if (index < 0)
                    {
                        TreeView.Select(Parent);
                    }
                    else
                    {
                        TreeView.Select(Parent.items[index]);
                    }
                }
                Parent.items.Remove(this);
                if (Parent.ItemCount == 0)
                {
                    Parent.IsExpanded = false;
                }
            }
        }

        public virtual void Sort()
        {
            items.Sort(Comparer);
        }

        private void SetInfo(TreeViewItem item)
        {
            item.Parent = this;
            item.TreeView = this.TreeView;
        }

        public bool Contain(TreeViewItem item)
        {
            if (items.Contains(item))
            {
                return true;
            }
            else
            {
                foreach (TreeViewItem tvi in items)
                {
                    if (tvi.Contain(item))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public TreeViewItem GetNext()
        {
            return GetNext(null);
        }

        private TreeViewItem GetNext(TreeViewItem from)
        {
            if (Parent == null) return null;
            if (IsExpanded && Items[ItemCount - 1] != from)
            {
                return Items[0];
            }
            else
            {
                var index = Parent.items.IndexOf(this);
                if (index < 0) return null;
                if (index + 1 < Parent.ItemCount) return Parent.Items[index + 1];
                return Parent.GetNext(this);
            }
        }

        public TreeViewItem GetPrevious()
        {
            return GetPrevious(null);
        }

        private TreeViewItem GetPrevious(TreeViewItem from)
        {
            if (Parent == null) return null;
            var index = Parent.items.IndexOf(this);
            if (index < 0) return null;
            if (index - 1 >= 0)
            {
                return Parent.Items[index - 1].GetLast();
            }
            return Parent;
        }
        public TreeViewItem GetLast()
        {
            if (IsExpanded)
            {
                return Items[ItemCount - 1].GetLast();
            }
            else
            {
                return this;
            }
        }

        protected abstract string Text { get; }
        protected abstract bool IsFolder { get; }
        protected virtual IComparer<TreeViewItem> Comparer
        {
            get
            {
                return TreeViewItemComparer.Comparer;
            }
        }
    }
}
