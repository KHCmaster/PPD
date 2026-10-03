using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PPDEditor.Forms
{
    public partial class SuggestWindow : Window
    {
        readonly ObservableCollection<KeyValuePair<string, string>> suggests = [];

        public string SelectedName
        {
            get;
            private set;
        }

        public string SelectedValue
        {
            get;
            private set;
        }

        public SuggestWindow()
        {
            InitializeComponent();
            dataGridView1.ItemsSource = suggests;
        }

        public void SetLang()
        {
            this.Title = Utility.Language["Suggestion"];
            this.Column1.Header = Utility.Language["Name"];
            this.Column2.Header = Utility.Language["Value"];
        }

        public void AddSuggest(string name, string value)
        {
            suggests.Add(new KeyValuePair<string, string>(name, value));
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Accept();
        }

        private void Accept()
        {
            if (dataGridView1.SelectedItem is not KeyValuePair<string, string> selected)
            {
                return;
            }
            SelectedName = selected.Key;
            SelectedValue = selected.Value;
            DialogResult = true;
        }

        private void DataGridView1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            okButton.IsEnabled = dataGridView1.SelectedItems.Count == 1;
        }

        private void DataGridView1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Accept();
        }
    }
}
