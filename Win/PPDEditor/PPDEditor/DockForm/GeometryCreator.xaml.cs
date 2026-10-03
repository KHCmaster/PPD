using PPDEditor.Command.PPDSheet;
using PPDEditor.Controls;
using PPDEditor.Forms;
using PPDEditor.ViewModels.Dialogs;
using PPDEditor.ViewModels.Docks;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace PPDEditor
{
    /// <summary>
    /// 曲線の上にマークを並べます。処理は <see cref="GeometryCreatorViewModel"/> にあります。
    /// </summary>
    public sealed partial class GeometryCreator : DockView, IGeometryHost
    {
        public GeometryCreator()
        {
            InitializeComponent();
            var viewModel = new GeometryCreatorViewModel(this);
            AttachViewModel(viewModel);
            viewModel.Attach(bezierControl1.Controller);
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        public GeometryCreatorViewModel ViewModel
        {
            get { return (GeometryCreatorViewModel)DataContext; }
        }

        public bool Antialias
        {
            get { return ViewModel.Antialias; }
            set { ViewModel.Antialias = value; }
        }

        public bool DrawPos
        {
            get { return ViewModel.DrawPos; }
            set { ViewModel.DrawPos = value; }
        }

        public bool DrawAngle
        {
            get { return ViewModel.DrawAngle; }
            set { ViewModel.DrawAngle = value; }
        }

        public bool ApplyMoved
        {
            get { return ViewModel.ApplyMoved; }
            set { ViewModel.ApplyMoved = value; }
        }

        public int FixedDistance
        {
            get { return ViewModel.FixedDistance; }
            set { ViewModel.FixedDistance = value; }
        }

        public int AngleRestriction
        {
            get { return (int)ViewModel.AngleRestriction; }
            set { ViewModel.AngleRestriction = value; }
        }

        public SquareGrid SquareGrid
        {
            get { return ViewModel.SquareGrid; }
            set { ViewModel.SquareGrid = value; }
        }

        public bool SquareGridEnabled
        {
            get { return ViewModel.SquareGridEnabled; }
            set { ViewModel.SquareGridEnabled = value; }
        }

        public void SetSkin()
        {
            var brush = WpfSkin.TransparentTextBrush;
            TextElement.SetForeground(optionPanel, brush);
            angleGroup.Foreground = brush;
        }

        // アンチエイリアスは描画のコントロールの設定なので、ここで反映する
        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GeometryCreatorViewModel.Antialias))
            {
                bezierControl1.AntialiasEnabled = ViewModel.Antialias;
                bezierControl1.DrawAndRefresh();
            }
        }

        // 曲線の編集のショートカット（文字を入力しているときは、その入力を優先する）
        private void GeometryCreator_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.OriginalSource is TextBox)
            {
                return;
            }
            ICommand command = null;
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                command = e.Key switch
                {
                    Key.Z => ViewModel.UndoCommand,
                    Key.Y => ViewModel.RedoCommand,
                    Key.T => ViewModel.StartTransformCommand,
                    _ => null,
                };
            }
            else if (e.Key == Key.Delete)
            {
                command = ViewModel.DeleteCommand;
            }
            else if (e.Key == Key.Return)
            {
                command = ViewModel.EndTransformCommand;
            }
            if (command != null)
            {
                command.Execute(null);
                e.Handled = true;
            }
        }

        private void BezierControl1_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            hintToolTip.Content = ViewModel.HintText;
        }

        private void PolygonCountLabel_ValueChange(object sender, ValueChangeEventArgs e)
        {
            ViewModel.NudgePolygonCount(e.ChangedValue);
        }

        PPDSheet IGeometryHost.SelectedSheet
        {
            get { return WindowUtility.LayerManager?.SelectedPpdSheet; }
        }

        float IGeometryHost.Bpm
        {
            get { return WindowUtility.MainForm.BPM; }
        }

        (int Width, int Height) IGeometryHost.CanvasSize
        {
            get { return ((int)bezierControl1.ActualWidth, (int)bezierControl1.ActualHeight); }
        }

        void IGeometryHost.ShowMessage(string text)
        {
            MessageBox.Show(Window.GetWindow(this), text);
        }

        SquareGrid IGeometryHost.EditGrid(SquareGrid grid)
        {
            var viewModel = new GridSettingViewModel(grid);
            return WpfDialog.ShowDialog(new GridSettingWindow(viewModel)) ? viewModel.Grid : null;
        }

        void IGeometryHost.Redraw()
        {
            bezierControl1?.DrawAndRefresh();
        }
    }
}
