using PPDEditorCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PPDEditor.Forms
{
    public partial class ImportEventWindow : Window
    {
        SortedList<float, EventData> currentEvent;
        SortedList<float, EventData> readOnlyEvent;

        public SortedList<float, EventData> ChangeData
        {
            get
            {
                return currentEvent;
            }
        }

        public ImportEventWindow()
        {
            InitializeComponent();
        }

        public void SetLang()
        {
            this.Title = Utility.Language["Import"];
            importButton.Content = String.Format("<={0}", Utility.Language["Import"]);
            deleteButton.Content = Utility.Language["Delete"];
            Column1.Header = dataGridViewTextBoxColumn1.Header = Utility.Language["Time(Sec)"];
            Column2.Header = dataGridViewTextBoxColumn2.Header = Utility.Language["Detail"];
        }

        private static void UpdateDataGrid(DataGrid dataGrid, SortedList<float, EventData> eventData)
        {
            dataGrid.ItemsSource = eventData.ToList();
        }

        public void SetCurrentEvent(SortedList<float, EventData> eventData)
        {
            currentEvent = eventData;
            UpdateDataGrid(currentEventDataGrid, currentEvent);
        }

        public void SetReadOnlyEvent(SortedList<float, EventData> eventData)
        {
            readOnlyEvent = eventData;
            UpdateDataGrid(readOnlyEventDataGrid, readOnlyEvent);
        }

        private void ImportEvent()
        {
            if (readOnlyEventDataGrid.SelectedItem is not KeyValuePair<float, EventData> selected)
            {
                return;
            }

            var time = selected.Key;
            if (currentEvent.ContainsKey(time))
            {
                return;
            }

            currentEvent.Add(time, readOnlyEvent[time]);
            UpdateDataGrid(currentEventDataGrid, currentEvent);
        }

        private void DeleteEvent()
        {
            if (currentEventDataGrid.SelectedItem is not KeyValuePair<float, EventData> selected)
            {
                return;
            }

            currentEvent.Remove(selected.Key);
            UpdateDataGrid(currentEventDataGrid, currentEvent);
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            ImportEvent();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteEvent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ShowEventData(EventData eventData)
        {
            var eef = new EventEditWindow
            {
                Owner = this
            };
            eef.SetLang();
            eef.EventData = eventData;
            eef.IsReadOnly = true;
            eef.ShowDialog();
        }

        private void CurrentEventDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (currentEventDataGrid.SelectedItem is KeyValuePair<float, EventData> selected)
            {
                ShowEventData(selected.Value.Clone());
            }
        }

        private void ReadOnlyEventDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (readOnlyEventDataGrid.SelectedItem is KeyValuePair<float, EventData> selected)
            {
                ShowEventData(selected.Value.Clone());
            }
        }
    }
}
