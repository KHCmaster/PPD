using PPDEditor.ViewModels.Dialogs;
using System.Windows;

namespace PPDEditor.Forms
{
    /// <summary>
    /// イベントを編集するウィンドウです。処理は <see cref="EventEditViewModel"/> にあります。
    /// </summary>
    public partial class EventEditWindow : Window
    {
        public EventEditWindow(EventEditViewModel viewModel)
        {
            InitializeComponent();
            this.AttachDialog(viewModel);
        }
    }
}
