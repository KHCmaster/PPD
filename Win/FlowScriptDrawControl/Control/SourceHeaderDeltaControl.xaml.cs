using FlowScriptDrawControl.Model;
using System.Windows.Controls;
using System.Windows.Input;

namespace FlowScriptDrawControl.Control
{
    /// <summary>
    /// SourceHeaderDelta.xaml の相互作用ロジック
    /// </summary>
    public partial class SourceHeaderDeltaControl : UserControl
    {
        public SourceHeaderDeltaControl()
        {
            InitializeComponent();
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var source = (Source)DataContext;
            source.IsCollapsed = !source.IsCollapsed;
            e.Handled = true;
        }
    }
}
