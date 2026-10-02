using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Xml.Linq;

namespace FlowScriptControl.Controls
{
    /// <summary>
    /// デバッグ実行中のノードとスコープのプロパティの値を表示するパネルです。
    /// </summary>
    /// <remarks>
    /// ノードを切り替えても、ファイルとノードごとにツリーの展開の状態を覚えておき、同じノードに戻ったときに元に戻す。
    /// </remarks>
    public class FlowExecutingPropertyPanel : UserControl
    {
        private readonly TreeView treeView;
        private readonly Dictionary<string, Dictionary<int, ExpandInfo>> expandInfos = [];

        public FlowExecutingPropertyPanel()
        {
            treeView = new TreeView();
            Content = treeView;
        }

        public string FileName
        {
            get;
            private set;
        }

        public int SourceId
        {
            get;
            private set;
        }

        public void ChangeSource(string fileName, string text)
        {
            if (treeView.Items.Count > 0 && !String.IsNullOrEmpty(FileName))
            {
                SaveExpandInfo(new ExpandInfo(treeView.Items, FileName, SourceId));
            }

            treeView.Items.Clear();
            var document = XDocument.Parse(text);
            var sourceElem = document.Root.Element("Source");
            var sourceId = sourceElem.Attribute("ID").Value;
            SourceId = int.Parse(sourceId);
            FileName = fileName;
            var sourceNode = new TreeViewItem { Header = String.Format("Source(ID:{0})", sourceId) };
            treeView.Items.Add(sourceNode);
            foreach (var elm in sourceElem.Elements("Property"))
            {
                ParseProperty(sourceNode, elm);
            }
            sourceNode.IsExpanded = true;

            var scopeElem = document.Root.Element("Scope");
            var scopeNode = new TreeViewItem { Header = String.Format("Scope(ID:{0})", scopeElem.Attribute("ID").Value) };
            treeView.Items.Add(scopeNode);
            foreach (var elm in scopeElem.Elements("Property"))
            {
                ParseProperty(scopeNode, elm);
            }
            scopeNode.IsExpanded = true;

            GetExpandInfo(FileName, SourceId)?.Restore(treeView.Items);
        }

        private static void ParseProperty(TreeViewItem node, XElement element)
        {
            var name = element.Attribute("Name").Value;
            var valueElems = element.Elements("Value");
            if (valueElems.Elements().Any())
            {
                var type = GetTypeAlias(valueElems.First().Attribute("Type").Value);
                var propertyNode = new TreeViewItem { Header = String.Format("{0}{1}", name, String.IsNullOrEmpty(type) ? "" : String.Format("({0})", type)) };
                node.Items.Add(propertyNode);
                foreach (var elm in valueElems.Elements("Property"))
                {
                    ParseProperty(propertyNode, elm);
                }
            }
            else
            {
                string header;
                if (valueElems.Any())
                {
                    var valueElem = valueElems.First();
                    var type = GetTypeAlias(valueElem.Attribute("Type").Value);
                    header = String.Format("{0}{1}: {2}", name, String.IsNullOrEmpty(type) ? "" : String.Format("({0})", type), valueElem.Value);
                }
                else
                {
                    header = name;
                }
                node.Items.Add(new TreeViewItem { Header = header });
            }
        }

        private static string GetTypeAlias(string type)
        {
            return FlowDrawPanel.TypeAliasDictionary.TryGetValue(type, out string value) ? value : "";
        }

        private ExpandInfo GetExpandInfo(string fileName, int sourceId)
        {
            if (expandInfos.TryGetValue(fileName, out Dictionary<int, ExpandInfo> dict) && dict.TryGetValue(sourceId, out ExpandInfo ret))
            {
                return ret;
            }
            return null;
        }

        private void SaveExpandInfo(ExpandInfo expandInfo)
        {
            if (!expandInfos.TryGetValue(expandInfo.FileName, out Dictionary<int, ExpandInfo> dict))
            {
                dict = [];
                expandInfos.Add(expandInfo.FileName, dict);
            }
            dict[expandInfo.SourceId] = expandInfo;
        }

        class ExpandInfo
        {
            private readonly ExpandInfoChild root = new();

            public string FileName { get; }

            public int SourceId { get; }

            public ExpandInfo(ItemCollection items, string fileName, int sourceId)
            {
                FileName = fileName;
                SourceId = sourceId;
                Check(items, root);
            }

            public void Restore(ItemCollection items)
            {
                Restore(items, root);
            }

            private static void Restore(ItemCollection items, ExpandInfoChild info)
            {
                for (int i = 0; i < Math.Min(items.Count, info.Children.Count); i++)
                {
                    if (items[i] is TreeViewItem item)
                    {
                        item.IsExpanded = info.Children[i].IsExpanded;
                        Restore(item.Items, info.Children[i]);
                    }
                }
            }

            private static void Check(ItemCollection items, ExpandInfoChild info)
            {
                foreach (var child in items.OfType<TreeViewItem>())
                {
                    var childInfo = new ExpandInfoChild { IsExpanded = child.IsExpanded };
                    info.Children.Add(childInfo);
                    Check(child.Items, childInfo);
                }
            }

            class ExpandInfoChild
            {
                public bool IsExpanded { get; set; }

                public List<ExpandInfoChild> Children { get; } = [];
            }
        }
    }
}
