using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace FlowScriptControl.Controls
{
    public partial class FlowExecutingPropertyPanel : UserControl
    {
        private Dictionary<string, Dictionary<int, ExpandInfo>> expandInfos;

        public FlowExecutingPropertyPanel()
        {
            InitializeComponent();
            expandInfos = new Dictionary<string, Dictionary<int, ExpandInfo>>();
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
            // save expand
            if (treeView1.Nodes.Count > 0 && !String.IsNullOrEmpty(FileName))
            {
                var savedInfo = new ExpandInfo(treeView1.Nodes, FileName, SourceId);
                SaveExpandInfo(savedInfo);
            }

            treeView1.Nodes.Clear();
            var document = XDocument.Parse(text);
            var sourceElem = document.Root.Element("Source");
            var sourceId = sourceElem.Attribute("ID").Value;
            SourceId = int.Parse(sourceId);
            FileName = fileName;
            var sourceNode = new TreeNode(String.Format("Source(ID:{0})", sourceId));
            treeView1.Nodes.Add(sourceNode);
            foreach (var elm in sourceElem.Elements("Property"))
            {
                ParseProperty(sourceNode, elm);
            }
            sourceNode.Expand();

            var scopeElem = document.Root.Element("Scope");
            var scopeNode = new TreeNode(String.Format("Scope(ID:{0})", scopeElem.Attribute("ID").Value));
            treeView1.Nodes.Add(scopeNode);
            foreach (var elm in scopeElem.Elements("Property"))
            {
                ParseProperty(scopeNode, elm);
            }
            scopeNode.Expand();

            var expandInfo = GetExpandInfo(FileName, SourceId);
            if (expandInfo != null)
            {
                expandInfo.Restore(treeView1.Nodes);
            }
        }

        public void ParseProperty(TreeNode node, XElement element)
        {
            var name = element.Attribute("Name").Value;
            var valueElems = element.Elements("Value");
            if (valueElems.Elements().Count() > 0)
            {
                var valueElem = valueElems.First();
                var type = valueElem.Attribute("Type").Value;
                if (FlowDrawPanel.TypeAliasDictionary.ContainsKey(type))
                {
                    type = FlowDrawPanel.TypeAliasDictionary[type];
                }
                else
                {
                    type = "";
                }
                var propertyNode = new TreeNode(String.Format("{0}{1}", name, String.IsNullOrEmpty(type) ? "" : String.Format("({0})", type)));
                node.Nodes.Add(propertyNode);
                foreach (var elm in valueElems.Elements("Property"))
                {
                    ParseProperty(propertyNode, elm);
                }
            }
            else
            {
                TreeNode valueNode = null;
                if (valueElems.Count() > 0)
                {
                    var valueElem = valueElems.First();
                    var type = valueElem.Attribute("Type").Value;
                    if (FlowDrawPanel.TypeAliasDictionary.ContainsKey(type))
                    {
                        type = FlowDrawPanel.TypeAliasDictionary[type];
                    }
                    else
                    {
                        type = "";
                    }
                    valueNode = new TreeNode(String.Format("{0}{1}: {2}", name, String.IsNullOrEmpty(type) ? "" : String.Format("({0})", type), valueElem.Value));
                }
                else
                {
                    valueNode = new TreeNode(String.Format("{0}", name));
                }
                node.Nodes.Add(valueNode);
            }
        }

        private ExpandInfo GetExpandInfo(string fileName, int sourceId)
        {
            if (expandInfos.TryGetValue(fileName, out Dictionary<int, ExpandInfo> dict))
            {
                if (dict.TryGetValue(sourceId, out ExpandInfo ret))
                {
                    return ret;
                }
            }
            return null;
        }

        private void SaveExpandInfo(ExpandInfo expandInfo)
        {
            if (!expandInfos.TryGetValue(expandInfo.FileName, out Dictionary<int, ExpandInfo> dict))
            {
                dict = new Dictionary<int, ExpandInfo>();
                expandInfos.Add(expandInfo.FileName, dict);
            }
            dict[expandInfo.SourceId] = expandInfo;
        }

        private void RestoreExpandInfo(ExpandInfo expandInfo)
        {
            expandInfo.Restore(treeView1.Nodes);
        }

        class ExpandInfo
        {
            private ExpandInfoChild root;

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

            public ExpandInfo(TreeNodeCollection nodes, string fileName, int sourceId)
            {
                FileName = fileName;
                SourceId = sourceId;
                root = new ExpandInfoChild();
                Check(nodes, root);
            }

            public void Restore(TreeNodeCollection nodes)
            {
                Restore(nodes, root);
            }

            private void Restore(TreeNodeCollection nodes, ExpandInfoChild info)
            {
                for (int i = 0; i < Math.Min(nodes.Count, info.ChildrenCount); i++)
                {
                    if (info.Children[i].IsExpanded)
                    {
                        nodes[i].Expand();
                    }
                    else
                    {
                        nodes[i].Collapse();
                    }
                    Restore(nodes[i].Nodes, info.Children[i]);
                }
            }

            private void Check(TreeNodeCollection nodes, ExpandInfoChild info)
            {
                foreach (TreeNode childNode in nodes)
                {
                    var child = new ExpandInfoChild
                    {
                        IsExpanded = childNode.IsExpanded
                    };
                    info.Add(child);
                    Check(childNode.Nodes, child);
                }
            }

            class ExpandInfoChild
            {
                List<ExpandInfoChild> children;

                public bool IsExpanded
                {
                    get;
                    set;
                }

                public int ChildrenCount
                {
                    get
                    {
                        return children.Count;
                    }
                }

                public ExpandInfoChild[] Children
                {
                    get
                    {
                        return children.ToArray();
                    }
                }

                public ExpandInfoChild()
                {
                    children = new List<ExpandInfoChild>();
                }

                public void Add(ExpandInfoChild child)
                {
                    children.Add(child);
                }
            }
        }
    }
}
