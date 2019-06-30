using FlowScriptDrawControl.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace FlowScriptDrawControl.Control
{
    /// <summary>
    /// SourceItemArrowControl.xaml の相互作用ロジック
    /// </summary>
    public partial class SourceItemArrowControl : UserControl
    {
        public SourceItemArrowControl()
        {
            InitializeComponent();
            DataContextChanged += SourceItemArrowControl_DataContextChanged;
        }

        private void BeginStarAnimation()
        {
            var storyboard = (Storyboard)Resources["storyboard"];
            storyboard.Begin();
        }

        private void EndStarAnimation()
        {
            var storyboard = (Storyboard)Resources["storyboard"];
            storyboard.Stop();
        }

        void SourceItemArrowControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                ((Item)e.OldValue).PropertyChanged -= newItem_PropertyChanged;
            }
            if (e.NewValue != null)
            {
                ((Item)e.NewValue).PropertyChanged += newItem_PropertyChanged;
            }
        }

        void newItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsConnectable")
            {
                var item = sender as Item;
                if (item.IsConnectable)
                {
                    BeginStarAnimation();
                }
                else
                {
                    EndStarAnimation();
                }
            }
        }
    }
}