using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PPDFramework;
using System.Collections.ObjectModel;
using System.Linq;

namespace PPDEditor.ViewModels.Dialogs
{
    /// <summary>
    /// タイムラインの行の順番と表示を編集するダイアログです。
    /// </summary>
    public partial class EditTimeLineRowViewModel(TimeLineRowManager rowManager) : DialogViewModel
    {
        public ObservableCollection<TimeLineRowItem> Rows { get; } = [.. rowManager.RowOrders.Select(row => new TimeLineRowItem(row, rowManager.RowVisibilities[row]))];

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(MoveUpCommand), nameof(MoveDownCommand))]
        public partial TimeLineRowItem SelectedRow { get; set; }

        /// <summary>
        /// 編集した順番と表示を反映します。
        /// </summary>
        public void Apply(TimeLineRowManager manager)
        {
            manager.RowOrders = [.. Rows.Select(r => r.RowIndex)];
            manager.RowVisibilities = [.. Rows.OrderBy(r => r.RowIndex).Select(r => r.IsVisible)];
        }

        private bool CanMoveUp()
        {
            return SelectedRow != null && Rows.IndexOf(SelectedRow) > 0;
        }

        [RelayCommand(CanExecute = nameof(CanMoveUp))]
        private void MoveUp()
        {
            Move(-1);
        }

        private bool CanMoveDown()
        {
            return SelectedRow != null && Rows.IndexOf(SelectedRow) < Rows.Count - 1;
        }

        [RelayCommand(CanExecute = nameof(CanMoveDown))]
        private void MoveDown()
        {
            Move(1);
        }

        private void Move(int direction)
        {
            var index = Rows.IndexOf(SelectedRow);
            Rows.Move(index, index + direction);
            MoveUpCommand.NotifyCanExecuteChanged();
            MoveDownCommand.NotifyCanExecuteChanged();
        }
    }

    /// <summary>
    /// タイムラインの 1 行（マークの種類）です。
    /// </summary>
    public partial class TimeLineRowItem(int rowIndex, bool isVisible) : ObservableObject
    {
        public int RowIndex { get; } = rowIndex;

        [ObservableProperty]
        public partial bool IsVisible { get; set; } = isVisible;

        public string Name
        {
            get { return LanguageTexts.Instance[((ButtonType)RowIndex).ToString()]; }
        }
    }
}
