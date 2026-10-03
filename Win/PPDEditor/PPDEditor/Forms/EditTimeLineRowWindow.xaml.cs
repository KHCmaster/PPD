using PPDFramework;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace PPDEditor.Forms
{
    public partial class EditTimeLineRowWindow : Window
    {
        private ObservableCollection<RowItem> rows;

        public EditTimeLineRowWindow()
        {
            InitializeComponent();
        }

        public void SetRowManager(TimeLineRowManager rowManager)
        {
            rows = [.. rowManager.RowOrders.Select(row => new RowItem(row, rowManager.RowVisibilities[row]))];
            dataGridView1.ItemsSource = rows;
        }

        public void SetLang()
        {
            Column1.Header = Utility.Language["Kind"];
            Column2.Header = Utility.Language["Visible"];
            button3.Content = Utility.Language["MoveUp"];
            button4.Content = Utility.Language["MoveDown"];
        }

        public void UpdateRowManager(TimeLineRowManager rowManager)
        {
            rowManager.RowOrders = [.. rows.Select(r => r.RowIndex)];
            rowManager.RowVisibilities = [.. rows.OrderBy(r => r.RowIndex).Select(r => r.IsVisible)];
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            Move(-1);
        }

        private void Button4_Click(object sender, RoutedEventArgs e)
        {
            Move(1);
        }

        private void Move(int direction)
        {
            if (dataGridView1.SelectedItem is not RowItem item)
            {
                return;
            }

            var rowIndex = rows.IndexOf(item);
            var newIndex = rowIndex + direction;
            if (newIndex < 0 || newIndex >= rows.Count)
            {
                return;
            }
            rows.Move(rowIndex, newIndex);
            dataGridView1.SelectedItem = item;
            dataGridView1.ScrollIntoView(item);
        }

        class RowItem(int rowIndex, bool isVisible)
        {
            public int RowIndex
            {
                get;
                private set;
            } = rowIndex;

            public bool IsVisible
            {
                get;
                set;
            } = isVisible;

            public string Name
            {
                get
                {
                    return Utility.Language[((ButtonType)RowIndex).ToString()];
                }
            }
        }
    }
}
