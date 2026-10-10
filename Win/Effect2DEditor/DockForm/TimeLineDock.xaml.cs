using Effect2DEditor.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Effect2DEditor.DockForm
{
    /// <summary>
    /// エフェクトの一覧とステートを並べるタイムラインと、再生のツールバーです。
    /// </summary>
    /// <remarks>
    /// DataContext は <see cref="MainWindowViewModel"/>。タイムライン（描いて表示するもの）には <see cref="Attach"/> でエフェクトを渡す。
    /// </remarks>
    public partial class TimeLineDock : UserControl
    {
        public TimeLineDock()
        {
            InitializeComponent();
        }

        public int CurrentFrame
        {
            get { return timeLine1.CurrentFrame; }
        }

        /// <summary>
        /// エディターのエフェクトを、タイムラインに表示するようにします。
        /// </summary>
        public void Attach(MainWindowViewModel viewModel)
        {
            timeLine1.EffectManager = viewModel.EffectManager;
            timeLine1.Host = viewModel;
            timeLine1.SetUtility(viewModel.SelectedManager);
            timeLineItem1.EffectManager = viewModel.EffectManager;
            timeLineItem1.Host = viewModel;
            timeLineItem1.SetUtility(viewModel.SelectedManager);
            timeLineItem1.TimeLine = timeLine1;
        }

        public void DrawAndRefresh()
        {
            timeLine1.DrawAndRefresh();
            timeLineItem1.DrawAndRefresh();
        }

        public void AdjustHScroll()
        {
            timeLine1.AdjustHScrollBar();
        }

        public void MoveVScroll(int val)
        {
            timeLine1.MoveVScroll(val);
        }

        // ▼ で、再生のしかたのメニューを出す
        private void PlayTypeButton_Click(object sender, RoutedEventArgs e)
        {
            var menu = playTypeButton.ContextMenu;
            menu.PlacementTarget = playTypeButton;
            menu.Placement = PlacementMode.Bottom;
            menu.IsOpen = true;
        }

        private void TimeLineDock_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private void TimeLineDock_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop, false) is string[] fileNames)
            {
                e.Handled = true;
                ((MainWindowViewModel)DataContext).AddDroppedFiles(fileNames);
            }
        }
    }
}
