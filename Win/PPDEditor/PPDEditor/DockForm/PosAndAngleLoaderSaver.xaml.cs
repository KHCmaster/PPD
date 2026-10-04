using PPDEditor.Controls;
using PPDEditor.Forms;
using PPDEditor.ViewModels.Dialogs;
using PPDEditor.ViewModels.Docks;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace PPDEditor
{
    /// <summary>
    /// マークの位置と角度を保存して当てはめます。処理は <see cref="PosAndAngleViewModel"/> にあります。
    /// </summary>
    public sealed partial class PosAndAngleLoaderSaver : DockView, IPosAndAngleHost
    {
        public PosAndAngleLoaderSaver()
        {
            InitializeComponent();
            AttachViewModel(new PosAndAngleViewModel(this));
        }

        public PosAndAngleViewModel ViewModel
        {
            get { return (PosAndAngleViewModel)DataContext; }
        }

        public bool DisplayAngle
        {
            get { return ViewModel.DisplayAngle; }
            set { ViewModel.DisplayAngle = value; }
        }

        public bool DrawAngle
        {
            get { return ViewModel.DrawAngle; }
        }

        public void SetSkin()
        {
            var brush = WpfSkin.TransparentTextBrush;
            TextElement.SetForeground(skinPanel, brush);
            applyGroup.Foreground = brush;
        }

        // 座標の名前（Tag）をドラッグした量だけ、その値を変える
        private void CoordinateLabel_ValueChange(object sender, ValueChangeEventArgs e)
        {
            ViewModel.Nudge((string)((FrameworkElement)sender).Tag, e.ChangedValue);
        }

        void IPosAndAngleHost.ShowMessage(string text)
        {
            MessageBox.Show(Window.GetWindow(this), text);
        }

        (string FileName, bool WithAngle)? IPosAndAngleHost.AskSaveName(string caption)
        {
            var viewModel = new SavePosAndAngleViewModel(caption);
            return WpfDialog.ShowDialog(new SavePosAndAngleWindow(viewModel)) ? (viewModel.FileName, viewModel.WithAngle) : null;
        }

        bool IPosAndAngleHost.GetSortedMarks(bool reverse, bool inAreaSelection, out Mark[] marks)
        {
            return WindowUtility.Seekmain.GetSortedData(reverse, inAreaSelection, out marks);
        }

        bool IPosAndAngleHost.ApplyToMarks(Vector2[] positions, float[] angles, bool reverse, int mode, bool withAngle)
        {
            return WindowUtility.Seekmain.SetData(positions, angles, reverse, mode, withAngle);
        }

        void IPosAndAngleHost.ShowPreview(Vector2[] positions, float[] angles)
        {
            WindowUtility.Editor?.ChangeData(positions, angles);
        }
    }
}
