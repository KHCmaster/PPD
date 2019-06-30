using FlowScriptControl.Classes;
using FlowScriptDrawControl.Model;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace FlowScriptControl.Controls
{
    public partial class FlowSearchPanel : UserControl
    {
        public event Func<string, SearchResult> Searched;
        public event Action<SearchResultItem> SelectionChanged;

        public FlowSearchPanel()
        {
            InitializeComponent();
        }

        public string SearchQuery
        {
            get { return toolStripTextBox1.Text; }
            set
            {
                toolStripTextBox1.Text = value;
                Search();
            }
        }

        private void ProcessSearchResult(SearchResult searchResult)
        {
            listBox1.Items.Clear();
            if (searchResult == null)
            {
                return;
            }

            listBox1.Items.AddRange(searchResult.Sources.Select(s => new SearchResultItem(searchResult, s)).ToArray());
            listBox1.Items.AddRange(searchResult.Comments.Select(c => new SearchResultItem(searchResult, c)).ToArray());


            using (Graphics g = listBox1.CreateGraphics())
            {
                float maxWidth = 0;
                foreach (SearchResultItem item in listBox1.Items)
                {
                    maxWidth = Math.Max(maxWidth,
                        g.MeasureString(listBox1.Items[listBox1.Items.Count - 1].ToString(), listBox1.Font).Width);
                }
                listBox1.HorizontalExtent = (int)maxWidth + 50;
            }
        }

        private void Search()
        {
            var searchResult = OnSearched(toolStripTextBox1.Text);
            ProcessSearchResult(searchResult);
        }

        private void toolStripTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Search();
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

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0 && listBox1.SelectedIndex < listBox1.Items.Count)
            {
                OnSelectionChanged(listBox1.Items[listBox1.SelectedIndex] as SearchResultItem);
            }
        }

        private SearchResult OnSearched(string query)
        {
            if (Searched != null)
            {
                return Searched(query);
            }
            return null;
        }

        private void OnSelectionChanged(SearchResultItem searchResultItem)
        {
            SelectionChanged?.Invoke(searchResultItem);
        }
    }
}
