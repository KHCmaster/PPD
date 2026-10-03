using PPDEditor.ViewModels.Docks;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace PPDEditor.DockForm.Script
{
    /// <summary>
    /// スクリプトを実行したときのログを出します。DataContext は <see cref="FlowLogViewModel"/> です。
    /// </summary>
    public partial class FlowLogPanel : UserControl
    {
        public FlowLogPanel()
        {
            InitializeComponent();
            DataContextChanged += FlowLogPanel_DataContextChanged;
        }

        // ログが増えたら、最後のログを見えるようにする
        private void FlowLogPanel_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is FlowLogViewModel oldViewModel)
            {
                oldViewModel.Logs.CollectionChanged -= Logs_CollectionChanged;
            }
            if (e.NewValue is FlowLogViewModel newViewModel)
            {
                newViewModel.Logs.CollectionChanged += Logs_CollectionChanged;
            }
        }

        private void Logs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                logGrid.ScrollIntoView(e.NewItems[0]);
            }
        }
    }
}
