using FlowScriptControl.Classes;
using FlowScriptDrawControl.Model;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FlowScriptControl.Controls
{
    /// <summary>
    /// スクリプトの中のノードとコメントを検索するパネルです。
    /// </summary>
    public class FlowSearchPanel : UserControl
    {
        public event Func<string, SearchResult> Searched;
        public event Action<SearchResultItem> SelectionChanged;

        private readonly TextBox queryTextBox;
        private readonly ListBox resultListBox;

        public FlowSearchPanel()
        {
            queryTextBox = new TextBox { Margin = new Thickness(2), VerticalContentAlignment = VerticalAlignment.Center };
            queryTextBox.KeyDown += QueryTextBox_KeyDown;
            resultListBox = new ListBox();
            ScrollViewer.SetHorizontalScrollBarVisibility(resultListBox, ScrollBarVisibility.Auto);
            resultListBox.SelectionChanged += ResultListBox_SelectionChanged;
            var panel = new DockPanel();
            DockPanel.SetDock(queryTextBox, Dock.Top);
            panel.Children.Add(queryTextBox);
            panel.Children.Add(resultListBox);
            Content = panel;
        }

        public string SearchQuery
        {
            get { return queryTextBox.Text; }
            set
            {
                queryTextBox.Text = value;
                Search();
            }
        }

        private void Search()
        {
            resultListBox.Items.Clear();
            var searchResult = Searched?.Invoke(queryTextBox.Text);
            if (searchResult == null)
            {
                return;
            }
            foreach (var item in searchResult.Sources.Select(s => new SearchResultItem(searchResult, s))
                .Concat(searchResult.Comments.Select(c => new SearchResultItem(searchResult, c))))
            {
                resultListBox.Items.Add(item);
            }
        }

        private void QueryTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search();
                e.Handled = true;
            }
        }

        private void ResultListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (resultListBox.SelectedItem is SearchResultItem item)
            {
                SelectionChanged?.Invoke(item);
            }
        }
    }
}
