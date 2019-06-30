using FlowScriptDrawControl.Model;
using System.Windows.Controls;
using System.Windows.Input;

namespace FlowScriptDrawControl.Control
{
    /// <summary>
    /// SourceHeaderControl.xaml の相互作用ロジック
    /// </summary>
    public partial class SourceHeaderControl : UserControl
    {
        public SourceHeaderControl()
        {
            InitializeComponent();
        }

        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var source = (Source)DataContext;
            source.IsBreakPointSet = !source.IsBreakPointSet;
        }
    }
}
