using Color = System.Drawing.Color;
using PPDEditor.ViewModels.Dialogs;
using System.Windows;

namespace PPDEditor.Forms
{
    /// <summary>
    /// グリッドを設定するウィンドウです。処理は <see cref="GridSettingViewModel"/> にあります。
    /// </summary>
    public partial class GridSettingWindow : Window
    {
        public GridSettingWindow(GridSettingViewModel viewModel)
        {
            InitializeComponent();
            this.AttachDialog(viewModel);
            viewModel.PickColor = PickColor;
        }

        private Color? PickColor(Color current)
        {
            var dialog = new FlowScriptDrawControl.Dialog.ColorDialog
            {
                Owner = this,
                CurrentColor = System.Windows.Media.Color.FromRgb(current.R, current.G, current.B)
            };
            if (dialog.ShowDialog() != true)
            {
                return null;
            }
            return Color.FromArgb(dialog.CurrentColor.R, dialog.CurrentColor.G, dialog.CurrentColor.B);
        }
    }
}
