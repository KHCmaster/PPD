using Effect2DEditor.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Effect2DEditor.DockForm
{
    /// <summary>
    /// 選んでいるステートの値を表示し、編集します。処理は <see cref="PropertyViewModel"/> にあります。
    /// </summary>
    public partial class PropertyDock : UserControl
    {
        public PropertyDock()
        {
            InitializeComponent();
        }

        // 名前を左右にドラッグしたら、値を増やしたり減らしたりする
        private void ValueLabel_ValueChange(object sender, ValueChangeEventArgs e)
        {
            ((StateValueRow)((FrameworkElement)sender).DataContext).AddValue(e.ChangedValue);
        }
    }
}
