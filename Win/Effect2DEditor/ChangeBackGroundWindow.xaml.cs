using Effect2DEditor.ViewModels.Dialogs;
using Microsoft.Win32;
using System.Windows;
using Color = System.Drawing.Color;

namespace Effect2DEditor
{
    /// <summary>
    /// カンバスの背景を変えるウィンドウです。処理は <see cref="ChangeBackGroundViewModel"/> にあります。
    /// </summary>
    public sealed partial class ChangeBackGroundWindow : Window, IPickerView
    {
        public ChangeBackGroundWindow()
        {
            InitializeComponent();
            DataContextChanged += (sender, e) =>
            {
                if (e.NewValue is ChangeBackGroundViewModel viewModel)
                {
                    viewModel.View = this;
                }
            };
        }

        Color? IPickerView.PickColor(Color current)
        {
            var dialog = new ColorPickWindow
            {
                Owner = this,
                SelectedColor = System.Windows.Media.Color.FromRgb(current.R, current.G, current.B)
            };
            if (dialog.ShowDialog() != true)
            {
                return null;
            }
            return Color.FromArgb(dialog.SelectedColor.R, dialog.SelectedColor.G, dialog.SelectedColor.B);
        }

        string IPickerView.OpenFile()
        {
            var dialog = new OpenFileDialog();
            return dialog.ShowDialog(this) == true ? dialog.FileName : null;
        }
    }
}
