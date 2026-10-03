using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace PPDEditor.Forms
{
    public partial class EditParameterWindow : Window
    {
        readonly ObservableCollection<ParameterItem> items = [];
        HashSet<string> existKeys;

        public Dictionary<string, string> Parameters
        {
            get
            {
                var ret = new Dictionary<string, string>();
                foreach (var item in items)
                {
                    ret.Add(item.Key ?? "", item.Value ?? "");
                }
                return ret;
            }
            set
            {
                items.Clear();
                foreach (var pair in value)
                {
                    items.Add(new ParameterItem { Key = pair.Key, Value = pair.Value });
                }
            }
        }

        public string[] ExistKeys
        {
            set
            {
                existKeys = [.. value];
            }
        }

        public EditParameterWindow()
        {
            InitializeComponent();
            existKeys = [];
            dataGridView1.ItemsSource = items;
        }

        public void SetLang()
        {
            Title = Utility.Language["EditParameter"];
            KeyColumn.Header = Utility.Language["Key"];
            ValueColumn.Header = Utility.Language["Value"];
            addButton.Content = Utility.Language["Add"];
            removeButton.Content = Utility.Language["Remove"];
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // 編集中のセルの値を確定してから閉じる
            dataGridView1.CommitEdit();
            dataGridView1.CommitEdit();
            DialogResult = true;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var keys = new HashSet<string>(items.Select(i => i.Key));
            int index = 1;
            string name;
            while (true)
            {
                name = String.Format("Key{0}", index);
                if (!keys.Contains(name) && !existKeys.Contains(name))
                {
                    break;
                }
                index++;
            }
            items.Add(new ParameterItem { Key = name, Value = "" });
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in dataGridView1.SelectedItems.OfType<ParameterItem>().ToArray())
            {
                items.Remove(item);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (DialogResult == true)
            {
                var keys = new HashSet<string>();
                foreach (var item in items)
                {
                    if (!keys.Add(item.Key ?? ""))
                    {
                        MessageBox.Show(this, Utility.Language["DuplicateKeyError"]);
                        e.Cancel = true;
                        return;
                    }
                }
            }
        }

        class ParameterItem
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }
    }
}
