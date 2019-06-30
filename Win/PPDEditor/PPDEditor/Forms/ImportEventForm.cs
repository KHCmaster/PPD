using PPDEditorCommon;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PPDEditor.Forms
{
    public partial class ImportEventForm : Form
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

        public ImportEventForm()
        {
            InitializeComponent();

            SizeChanged += ImportEventForm_SizeChanged;
            AdjustPosition();
        }

        public void SetLang()
        {
            this.Text = Utility.Language["Import"];
            importButton.Text = String.Format("<={0}", Utility.Language["Import"]);
            deleteButton.Text = Utility.Language["Delete"];
            Column1.HeaderText = dataGridViewTextBoxColumn1.HeaderText = Utility.Language["Time(Sec)"];
            Column2.HeaderText = dataGridViewTextBoxColumn2.HeaderText = Utility.Language["Detail"];
        }

        private void UpdateDataGrid(DataGridView dataGrid, SortedList<float, EventData> eventData)
        {
            dataGrid.RowCount = 0;
            foreach (KeyValuePair<float, EventData> kvp in eventData)
            {
                dataGrid.Rows.Add(kvp.Key, kvp.Value);
            }
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

        private void AdjustPosition()
        {
            currentEventDataGrid.Width = readOnlyEventDataGrid.Width = (Width - 140) / 2;
            currentEventDataGrid.Location = new Point(0, 0);
            readOnlyEventDataGrid.Location = new Point(Width - readOnlyEventDataGrid.Width, 0);
            importButton.Location = new Point((Width - deleteButton.Width) / 2, 120);
            deleteButton.Location = new Point((Width - deleteButton.Width) / 2, 150);
            okButton.Location = new Point(Width / 2 - 20 - okButton.Width, okButton.Location.Y);
            cancelButton.Location = new Point(Width / 2 + 20, cancelButton.Location.Y);
        }

        private void ImportEvent()
        {
            if (readOnlyEventDataGrid.SelectedRows.Count == 0)
            {
                return;
            }


            var time = (float)readOnlyEventDataGrid[0, readOnlyEventDataGrid.SelectedRows[0].Index].Value;
            if (currentEvent.ContainsKey(time))
            {
                return;
            }

            currentEvent.Add(time, readOnlyEvent[time]);
            UpdateDataGrid(currentEventDataGrid, currentEvent);
        }

        private void DeleteEvent()
        {
            if (currentEventDataGrid.SelectedRows.Count == 0)
            {
                return;
            }

            var time = (float)currentEventDataGrid[0, currentEventDataGrid.SelectedRows[0].Index].Value;
            currentEvent.Remove(time);
            currentEventDataGrid.Rows.RemoveAt(currentEventDataGrid.SelectedRows[0].Index);
        }

        void ImportEventForm_SizeChanged(object sender, EventArgs e)
        {
            AdjustPosition();
        }

        private void importButton_Click(object sender, EventArgs e)
        {
            ImportEvent();
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            DeleteEvent();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void ShowEventData(EventData eventData)
        {
            var eef = new EventEditForm();
            eef.SetLang();
            eef.EventData = eventData;
            eef.IsReadOnly = true;
            eef.ShowDialog();
        }

        private void currentEventDataGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (currentEventDataGrid.CurrentRow == null)
            {
                return;
            }

            var eventData = currentEvent.Values[currentEventDataGrid.CurrentRow.Index].Clone();
            ShowEventData(eventData);
        }

        private void readOnlyEventDataGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (readOnlyEventDataGrid.CurrentRow == null)
            {
                return;
            }

            var eventData = readOnlyEvent.Values[readOnlyEventDataGrid.CurrentRow.Index].Clone();
            ShowEventData(eventData);
        }
    }
}
