using PPDEditorCommon;
using PPDEditor.ViewModels.Dialogs;
using System.Windows;

namespace PPDEditor.Forms
{
    /// <summary>
    /// ほかの譜面のイベントをインポートするウィンドウです。処理は <see cref="ImportEventViewModel"/> にあります。
    /// </summary>
    public partial class ImportEventWindow : Window
    {
        public ImportEventWindow(ImportEventViewModel viewModel)
        {
            InitializeComponent();
            this.AttachDialog(viewModel);
            viewModel.ShowEventRequested += ShowEvent;
        }

        // イベントの中身を、編集できない状態で表示する
        private void ShowEvent(EventData eventData)
        {
            new EventEditWindow(new EventEditViewModel(eventData, isReadOnly: true)) { Owner = this }.ShowDialog();
        }
    }
}
