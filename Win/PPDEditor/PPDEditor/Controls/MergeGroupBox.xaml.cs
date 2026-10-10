using System.Windows.Controls;

namespace PPDEditor.Controls
{
    /// <summary>
    /// マージするプロジェクトと難易度を選ぶ部分です。DataContext は <see cref="ViewModels.Dialogs.MergeSlot"/> です。
    /// </summary>
    public partial class MergeGroupBox : UserControl
    {
        public MergeGroupBox()
        {
            InitializeComponent();
        }
    }
}
