using PPDEditor.Forms;
using PPDEditor.ViewModels.Docks;
using System.Windows.Documents;
using System.Windows.Input;

namespace PPDEditor
{
    /// <summary>
    /// ボタンやキーを拍に合わせて押して、BPM を測ります。処理は <see cref="BPMMeasureViewModel"/> にあります。
    /// </summary>
    /// <remarks>
    /// 押した瞬間に数えるため、ボタンはマウスを押したとき、キーは押したとき（押しっぱなしの繰り返しは除く）に数える。
    /// </remarks>
    public partial class BPMMeasure : DockView
    {
        bool down;

        public BPMMeasure()
        {
            InitializeComponent();
            AttachViewModel(new BPMMeasureViewModel(() => NativeMethods.timeGetTime()));
        }

        private BPMMeasureViewModel ViewModel
        {
            get { return (BPMMeasureViewModel)DataContext; }
        }

        public void SetSkin()
        {
            TextElement.SetForeground(contentGrid, WpfSkin.TransparentTextBrush);
        }

        private void TapButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ViewModel.TapCommand.Execute(null);
        }

        private void BPMMeasure_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return && e.Key != Key.Space)
            {
                down = true;
                return;
            }
            // 押しっぱなしの繰り返しでは数えず、ボタンも押されないようにする
            e.Handled = true;
            if (!down)
            {
                down = true;
                ViewModel.TapCommand.Execute(null);
            }
        }

        private void BPMMeasure_KeyUp(object sender, KeyEventArgs e)
        {
            down = false;
        }
    }
}
