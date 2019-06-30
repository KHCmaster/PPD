using PPDFramework;
using System;
using System.Collections.Generic;

namespace PPDSingle
{
    class LogicFolderTreeViewItem : TreeViewItem
    {
        class LogicFolderTreeViewItemComparer : IComparer<TreeViewItem>
        {
            static LogicFolderTreeViewItemComparer comparer = new LogicFolderTreeViewItemComparer();

            public static LogicFolderTreeViewItemComparer Comparer
            {
                get
                {
                    return comparer;
                }
            }
            #region IComparer<TreeViewItem> メンバ

            public int Compare(TreeViewItem x, TreeViewItem y)
            {
                var xn = x as LogicFolderTreeViewItem;
                var yn = y as LogicFolderTreeViewItem;

                int xIndex = Array.IndexOf(xn.LogicFolderInfo.Parent.Children, xn.LogicFolderInfo), yIndex = Array.IndexOf(yn.LogicFolderInfo.Parent.Children, yn.LogicFolderInfo);

                if (xIndex < 0 || yIndex < 0)
                {

                }

                return xIndex - yIndex;

                /*if (xn.IsFolder)
                {
                    if (yn.IsFolder)
                    {
                        if (xn.Text == yn.Text)
                        {
                            return xn.LogicFolderInfo.DateTime.CompareTo(yn.LogicFolderInfo.DateTime);
                        }
                        else
                        {
                            return StringComparer.InvariantCulture.Compare(xn.Text, yn.Text);
                        }
                    }
                    else
                    {
                        return -1;
                    }
                }
                else
                {
                    if (yn.IsFolder)
                    {
                        return 1;
                    }
                    else
                    {
                        if (xn.Text == yn.Text)
                        {
                            return xn.LogicFolderInfo.DateTime.CompareTo(yn.LogicFolderInfo.DateTime);
                        }
                        else
                        {
                            return StringComparer.InvariantCulture.Compare(xn.Text, yn.Text);
                        }
                    }
                }*/
            }

            #endregion
        }

        public TextureString TextureString
        {
            get;
            set;
        }

        public LogicFolderInfomation LogicFolderInfo
        {
            get;
            set;
        }

        protected override string Text
        {
            get { return TextureString.Text; }
        }

        protected override bool IsFolder
        {
            get { return LogicFolderInfo.IsFolder; }
        }

        protected override IComparer<TreeViewItem> Comparer
        {
            get
            {
                return LogicFolderTreeViewItemComparer.Comparer;
            }
        }
    }
}
