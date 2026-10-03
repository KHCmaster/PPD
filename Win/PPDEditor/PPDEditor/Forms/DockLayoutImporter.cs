using AvalonDock;
using AvalonDock.Layout;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;

namespace PPDEditor.Forms
{
    /// <summary>
    /// 以前の WinForms 版（DockPanel Suite）で保存したドックの配置を、AvalonDock の配置にします。
    /// </summary>
    /// <remarks>
    /// 以前の配置のファイル（dockforminfo.xml）と、同梱の view フォルダーの配置を読むために使う。
    /// 端（左右上下）とドキュメントの領域の分け方、タブの並び、浮かせたウィンドウの位置、隠したウィンドウを移す。
    /// </remarks>
    static class DockLayoutImporter
    {
        /// <summary>
        /// DockPanel Suite の形式のファイルかどうかを返す。
        /// </summary>
        public static bool IsDockPanelSuiteLayout(string path)
        {
            try
            {
                using var reader = XmlReader.Create(path);
                reader.MoveToContent();
                return reader.LocalName == "DockPanel";
            }
            catch (XmlException)
            {
                return false;
            }
        }

        sealed class ContentInfo
        {
            public LayoutAnchorable Anchorable { get; init; }
            public bool IsHidden { get; init; }
            public bool IsFloat { get; init; }
        }

        sealed class PaneInfo
        {
            public string DockState { get; init; }
            public List<ContentInfo> Contents { get; } = [];
            public ContentInfo ActiveContent { get; set; }

            /// <summary>
            /// このペインに今ある（浮かせているかどうかがペインと合う）ウィンドウです。
            /// </summary>
            public IEnumerable<ContentInfo> CurrentContents
            {
                get { return Contents.Where(c => c.Anchorable != null && c.IsFloat == (DockState == "Float")); }
            }
        }

        /// <summary>
        /// 分け方の木です。葉はペイン、それ以外は 2 つに分けたもの。
        /// </summary>
        sealed class Node
        {
            public PaneInfo Pane { get; set; }
            public Node First { get; set; }
            public Node Second { get; set; }
            public Orientation Orientation { get; set; }
            public double FirstProportion { get; set; }
        }

        /// <summary>
        /// 読み込みます。
        /// </summary>
        /// <param name="getAnchorable">保存した名前（以前のクラスの名前）からウィンドウを返す。</param>
        /// <param name="createDefaultLayout">読み込めなかったときに既定の配置にする。</param>
        /// <returns>読み込めたかどうか。</returns>
        public static bool Import(string path, DockingManager manager, Func<string, LayoutAnchorable> getAnchorable, Action createDefaultLayout)
        {
            try
            {
                Build(XDocument.Load(path).Root, manager, getAnchorable);
                return true;
            }
            catch (Exception e) when (e is XmlException or FormatException or InvalidOperationException or NullReferenceException or ArgumentException)
            {
                createDefaultLayout();
                return false;
            }
        }

        private static void Build(XElement root, DockingManager manager, Func<string, LayoutAnchorable> getAnchorable)
        {
            var contents = root.Element("Contents").Elements("Content").ToDictionary(
                e => (int)e.Attribute("ID"),
                e => new ContentInfo
                {
                    Anchorable = getAnchorable((string)e.Attribute("PersistString")),
                    IsHidden = (bool)e.Attribute("IsHidden"),
                    IsFloat = (bool)e.Attribute("IsFloat")
                });
            var panes = new Dictionary<int, PaneInfo>();
            foreach (var paneElement in root.Element("Panes").Elements("Pane"))
            {
                var pane = new PaneInfo { DockState = (string)paneElement.Attribute("DockState") };
                foreach (var contentElement in paneElement.Element("Contents").Elements("Content"))
                {
                    pane.Contents.Add(contents[(int)contentElement.Attribute("RefID")]);
                }
                var active = (int)paneElement.Attribute("ActiveContent");
                pane.ActiveContent = active >= 0 && contents.TryGetValue(active, out var activeContent) ? activeContent : null;
                panes.Add((int)paneElement.Attribute("ID"), pane);
            }

            // 今の配置から外して、新しい配置に入れ直す
            var all = contents.Values.Select(c => c.Anchorable).Where(a => a != null).Distinct().ToList();
            foreach (var anchorable in all)
            {
                anchorable.Parent?.RemoveChild(anchorable);
            }

            var placed = new HashSet<LayoutAnchorable>();
            var dockWindows = root.Element("DockWindows").Elements("DockWindow")
                .ToDictionary(e => (string)e.Attribute("DockState"), e => BuildTree(e.Element("NestedPanes"), panes));

            var windowWidth = manager.ActualWidth > 0 ? manager.ActualWidth : 1000;
            var windowHeight = manager.ActualHeight > 0 ? manager.ActualHeight : 700;
            GridLength Portion(string name, double total)
            {
                var value = double.Parse((string)root.Attribute(name), CultureInfo.InvariantCulture);
                return new GridLength(value <= 1 ? value * total : value);
            }

            var center = new LayoutPanel { Orientation = Orientation.Vertical };
            AddSide(center, dockWindows.GetValueOrDefault("DockTop"), false, Portion("DockTopPortion", windowHeight), placed, isWidth: false);
            var document = ToLayout(dockWindows.GetValueOrDefault("Document"), true, placed);
            center.Children.Add(document ?? new LayoutDocumentPane());
            AddSide(center, dockWindows.GetValueOrDefault("DockBottom"), false, Portion("DockBottomPortion", windowHeight), placed, isWidth: false);

            var rootPanel = new LayoutPanel { Orientation = Orientation.Horizontal };
            AddSide(rootPanel, dockWindows.GetValueOrDefault("DockLeft"), false, Portion("DockLeftPortion", windowWidth), placed, isWidth: true);
            rootPanel.Children.Add(center);
            AddSide(rootPanel, dockWindows.GetValueOrDefault("DockRight"), false, Portion("DockRightPortion", windowWidth), placed, isWidth: true);
            manager.Layout = new LayoutRoot { RootPanel = rootPanel };

            // 浮かせているウィンドウ
            foreach (var floatWindow in root.Element("FloatWindows")?.Elements("FloatWindow") ?? [])
            {
                var tree = BuildTree(floatWindow.Element("NestedPanes"), panes);
                var floating = Leaves(tree).SelectMany(p => p.CurrentContents).Where(c => !c.IsHidden && placed.Add(c.Anchorable)).ToList();
                if (floating.Count == 0)
                {
                    continue;
                }
                var bounds = ((string)floatWindow.Attribute("Bounds")).Split(',').Select(s => double.Parse(s.Trim(), CultureInfo.InvariantCulture)).ToArray();
                var first = floating[0].Anchorable;
                first.AddToLayout(manager, AnchorableShowStrategy.Most);
                first.FloatingLeft = bounds[0];
                first.FloatingTop = bounds[1];
                first.FloatingWidth = bounds[2];
                first.FloatingHeight = bounds[3];
                first.Float();
                if (first.Parent is LayoutAnchorablePane floatingPane)
                {
                    foreach (var other in floating.Skip(1))
                    {
                        floatingPane.Children.Add(other.Anchorable);
                    }
                }
            }

            // 隠していたウィンドウは、置いた場所を覚えさせてから隠す
            foreach (var content in contents.Values.Where(c => c.Anchorable != null && c.IsHidden))
            {
                if (content.Anchorable.Root != null && !content.Anchorable.IsHidden)
                {
                    content.Anchorable.Hide();
                }
            }
        }

        private static Node BuildTree(XElement nestedPanes, Dictionary<int, PaneInfo> panes)
        {
            if (nestedPanes == null)
            {
                return null;
            }
            var nodes = new Dictionary<int, Node>();
            Node root = null;
            foreach (var element in nestedPanes.Elements("Pane"))
            {
                var leaf = new Node { Pane = panes[(int)element.Attribute("RefID")] };
                var id = (int)element.Attribute("ID");
                var prev = (int)element.Attribute("PrevPane");
                if (prev < 0 || !nodes.TryGetValue(prev, out var prevNode))
                {
                    root ??= leaf;
                    nodes[id] = leaf;
                    continue;
                }
                // 前のペインの場所を 2 つに分け、新しいペインを指定の側に、指定の割合で置く
                var alignment = (string)element.Attribute("Alignment");
                var proportion = double.Parse((string)element.Attribute("Proportion"), CultureInfo.InvariantCulture);
                var moved = new Node { Pane = prevNode.Pane, First = prevNode.First, Second = prevNode.Second, Orientation = prevNode.Orientation, FirstProportion = prevNode.FirstProportion };
                var newFirst = alignment is "Left" or "Top";
                prevNode.Pane = null;
                prevNode.Orientation = alignment is "Left" or "Right" ? Orientation.Horizontal : Orientation.Vertical;
                prevNode.First = newFirst ? leaf : moved;
                prevNode.Second = newFirst ? moved : leaf;
                prevNode.FirstProportion = newFirst ? proportion : 1 - proportion;
                // 前のペインの番号は、分けた後の前のペイン側を指す
                foreach (var key in nodes.Keys.ToList())
                {
                    if (nodes[key] == prevNode)
                    {
                        nodes[key] = moved;
                    }
                }
                nodes[id] = leaf;
            }
            return root;
        }

        private static IEnumerable<PaneInfo> Leaves(Node node)
        {
            if (node == null)
            {
                yield break;
            }
            if (node.Pane != null)
            {
                yield return node.Pane;
                yield break;
            }
            foreach (var pane in Leaves(node.First).Concat(Leaves(node.Second)))
            {
                yield return pane;
            }
        }

        /// <summary>
        /// 端に置くウィンドウを加える。
        /// </summary>
        private static void AddSide(LayoutPanel parent, Node tree, bool isDocument, GridLength size, HashSet<LayoutAnchorable> placed, bool isWidth)
        {
            var element = ToLayout(tree, isDocument, placed);
            if (element == null)
            {
                return;
            }
            SetSize(element, isWidth, size);
            parent.Children.Add(element);
        }

        /// <summary>
        /// 木を AvalonDock の要素にする。ウィンドウがないペインは除く。
        /// </summary>
        private static ILayoutPanelElement ToLayout(Node node, bool isDocument, HashSet<LayoutAnchorable> placed)
        {
            if (node == null)
            {
                return null;
            }
            if (node.Pane != null)
            {
                var contents = node.Pane.CurrentContents.Where(c => placed.Add(c.Anchorable)).ToList();
                if (contents.Count == 0)
                {
                    return null;
                }
                ILayoutPanelElement pane;
                if (isDocument)
                {
                    var documentPane = new LayoutDocumentPane();
                    foreach (var content in contents)
                    {
                        documentPane.Children.Add(content.Anchorable);
                    }
                    pane = documentPane;
                }
                else
                {
                    var anchorablePane = new LayoutAnchorablePane();
                    foreach (var content in contents)
                    {
                        anchorablePane.Children.Add(content.Anchorable);
                    }
                    pane = anchorablePane;
                }
                var active = node.Pane.ActiveContent;
                if (active != null && contents.Contains(active))
                {
                    active.Anchorable.IsSelected = true;
                }
                return pane;
            }

            var first = ToLayout(node.First, isDocument, placed);
            var second = ToLayout(node.Second, isDocument, placed);
            if (first == null || second == null)
            {
                return first ?? second;
            }
            SetStarSize(first, node.Orientation, node.FirstProportion);
            SetStarSize(second, node.Orientation, 1 - node.FirstProportion);
            if (isDocument)
            {
                var group = new LayoutDocumentPaneGroup { Orientation = node.Orientation };
                group.Children.Add((ILayoutDocumentPane)first);
                group.Children.Add((ILayoutDocumentPane)second);
                return group;
            }
            else
            {
                var group = new LayoutAnchorablePaneGroup { Orientation = node.Orientation };
                group.Children.Add((ILayoutAnchorablePane)first);
                group.Children.Add((ILayoutAnchorablePane)second);
                return group;
            }
        }

        private static void SetStarSize(ILayoutPanelElement element, Orientation orientation, double proportion)
        {
            SetSize(element, orientation == Orientation.Horizontal, new GridLength(Math.Max(0.01, proportion), GridUnitType.Star));
        }

        /// <summary>
        /// 幅か高さを設定する（AvalonDock の共通のインターフェースは公開されていないので、型ごとに設定する）。
        /// </summary>
        private static void SetSize(ILayoutPanelElement element, bool isWidth, GridLength size)
        {
            switch (element)
            {
                case LayoutAnchorablePane pane:
                    if (isWidth) pane.DockWidth = size; else pane.DockHeight = size;
                    break;
                case LayoutAnchorablePaneGroup group:
                    if (isWidth) group.DockWidth = size; else group.DockHeight = size;
                    break;
                case LayoutDocumentPane documentPane:
                    if (isWidth) documentPane.DockWidth = size; else documentPane.DockHeight = size;
                    break;
                case LayoutDocumentPaneGroup documentGroup:
                    if (isWidth) documentGroup.DockWidth = size; else documentGroup.DockHeight = size;
                    break;
            }
        }
    }
}
