using FlowScriptControl.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FlowScriptControl.Controls
{
    /// <summary>
    /// ノードの一覧のツリーです。ノードをドラッグして FlowDrawPanel に追加できます。
    /// </summary>
    public partial class FlowTreeView : UserControl
    {
        /// <summary>
        /// ドラッグのデータの形式です。データはノードの型の完全な名前（文字列）です。
        /// </summary>
        public const string DragDataFormat = "FlowScriptControl.FlowNodeTypeName";

        private readonly List<FlowNodeEntry> entries = [];
        private Point dragStartPosition;
        private FlowNodeEntry dragStartEntry;

        public event EventHandler SelectedFilterChanged;

        public FlowTreeView()
        {
            InitializeComponent();
        }

        public NodeFilterInfoBase SelectedFilter
        {
            get { return filterComboBox.SelectedItem as NodeFilterInfoBase; }
        }

        internal void SetEntries(IEnumerable<FlowNodeEntry> newEntries)
        {
            entries.Clear();
            entries.AddRange(newEntries);
            treeView.ItemsSource = entries;
            Filter();
        }

        internal void SetFilters(NodeFilterInfoBase[] filters)
        {
            filterComboBox.ItemsSource = filters;
            if (filters.Length > 0)
            {
                filterComboBox.SelectedIndex = 0;
            }
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Filter();
            SelectedFilterChanged?.Invoke(this, EventArgs.Empty);
        }

        private void FilterTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Filter();
                e.Handled = true;
            }
        }

        private void TreeView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
            {
                filterTextBox.Focus();
                e.Handled = true;
            }
        }

        private void Filter()
        {
            var filterText = filterTextBox.Text ?? "";
            var textFilters = filterText.ToLower().Split('.');
            var expandOrCollapse = !String.IsNullOrEmpty(filterText);
            foreach (var entry in entries)
            {
                ResetVisible(entry);
            }
            ApplyFilterInfo(entries, SelectedFilter);
            ApplyTextFilter(entries, textFilters, 0, expandOrCollapse);
        }

        private static void ResetVisible(FlowNodeEntry entry)
        {
            entry.IsVisible = true;
            foreach (var child in entry.Children)
            {
                ResetVisible(child);
            }
        }

        // 選んだフィルター（どのアプリで使えるノードか）に合わないノードを隠す。子が全て隠れたフォルダーも隠す
        private static bool ApplyFilterInfo(IEnumerable<FlowNodeEntry> nodes, NodeFilterInfoBase filterInfo)
        {
            var hasVisibleNodes = false;
            foreach (var node in nodes.Where(n => n.IsVisible))
            {
                bool visible;
                if (node.Children.Count > 0)
                {
                    visible = ApplyFilterInfo(node.Children, filterInfo);
                }
                else
                {
                    visible = node.Dumper == null || filterInfo == null || !filterInfo.IsHide(node.Dumper);
                }
                node.IsVisible = visible;
                hasVisibleNodes |= visible;
            }
            return hasVisibleNodes;
        }

        // 入力した文字列（ドットで区切ると、フォルダーの階層ごとに絞り込む）に合わないノードを隠す
        private static bool ApplyTextFilter(IEnumerable<FlowNodeEntry> nodes, string[] filters, int filterIndex, bool expandOrCollapse)
        {
            var hasVisibleNodes = false;
            foreach (var node in nodes.Where(n => n.IsVisible))
            {
                void applyChildren(int nextIndex)
                {
                    var childrenIsVisible = ApplyTextFilter(node.Children, filters, nextIndex, expandOrCollapse);
                    if (expandOrCollapse)
                    {
                        node.IsExpanded = childrenIsVisible;
                    }
                    if (!childrenIsVisible)
                    {
                        node.IsVisible = false;
                    }
                    hasVisibleNodes |= childrenIsVisible;
                }
                if (IsMatched(node, filters[filterIndex]))
                {
                    if (filterIndex == filters.Length - 1)
                    {
                        if (expandOrCollapse)
                        {
                            node.IsExpanded = true;
                        }
                        hasVisibleNodes = true;
                    }
                    else
                    {
                        applyChildren(filterIndex + 1);
                    }
                }
                else
                {
                    applyChildren(0);
                }
            }
            return hasVisibleNodes;
        }

        private static bool IsMatched(FlowNodeEntry node, string filter)
        {
            if (node.Text.Contains(filter, StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }
            if (node.Dumper == null)
            {
                return false;
            }
            return node.Dumper.InEvents.Concat(node.Dumper.OutEvents).Concat(node.Dumper.InProperties).Concat(node.Dumper.OutProperties)
                .Any(item => item.Name.Contains(filter, StringComparison.CurrentCultureIgnoreCase));
        }

        private void TreeView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            dragStartPosition = e.GetPosition(treeView);
            dragStartEntry = FindEntry(e.OriginalSource as DependencyObject);
        }

        private void TreeView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || dragStartEntry == null || dragStartEntry.Dumper == null || dragStartEntry.Children.Count > 0)
            {
                return;
            }
            var diff = e.GetPosition(treeView) - dragStartPosition;
            if (Math.Abs(diff.X) < SystemParameters.MinimumHorizontalDragDistance && Math.Abs(diff.Y) < SystemParameters.MinimumVerticalDragDistance)
            {
                return;
            }
            var entry = dragStartEntry;
            dragStartEntry = null;
            var data = new DataObject(DragDataFormat, FlowDrawPanel.GetTypeName(entry.Dumper));
            DragDrop.DoDragDrop(treeView, data, DragDropEffects.Copy);
        }

        private static FlowNodeEntry FindEntry(DependencyObject element)
        {
            while (element != null && element is not TreeViewItem)
            {
                element = element is Visual || element is System.Windows.Media.Media3D.Visual3D
                    ? VisualTreeHelper.GetParent(element)
                    : LogicalTreeHelper.GetParent(element);
            }
            return (element as TreeViewItem)?.DataContext as FlowNodeEntry;
        }
    }
}
