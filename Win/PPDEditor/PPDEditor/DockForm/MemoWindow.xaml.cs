using PPDEditor.Forms;
using PPDEditor.ViewModels.Docks;
using System.Windows.Input;

namespace PPDEditor
{
    /// <summary>
    /// プロジェクトのメモです。処理は <see cref="MemoViewModel"/> にあります。
    /// </summary>
    public partial class MemoWindow : DockView
    {
        public MemoWindow()
        {
            InitializeComponent();
            AttachViewModel(new MemoViewModel());
        }

        public MemoViewModel ViewModel
        {
            get { return (MemoViewModel)DataContext; }
        }

        public void Clear()
        {
            ViewModel.MemoText = "";
        }

        public string MemoText
        {
            get { return ViewModel.MemoText; }
            set { ViewModel.MemoText = value; }
        }

        /// <summary>
        /// 文字の大きさ（ポイント）です。
        /// </summary>
        public float MemoFontSize
        {
            get { return ViewModel.MemoFontSize; }
            set { ViewModel.SetFontSize(value); }
        }

        private void TextBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                ViewModel.IncreaseFontSizeCommand.Execute(null);
            }
            else if (e.Delta < 0)
            {
                ViewModel.DecreaseFontSizeCommand.Execute(null);
            }
            e.Handled = true;
        }
    }
}
