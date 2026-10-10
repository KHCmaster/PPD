using System.Windows.Controls;
using System.Windows.Input;
using PPDEditor.ViewModels.Dialogs;
using System.Windows;

namespace PPDEditor.Forms
{
    /// <summary>
    /// 動画を切り取る幅を設定するウィンドウです。処理は <see cref="MovieCutViewModel"/> にあります。
    /// </summary>
    public partial class MovieCutWindow : Window
    {
        public MovieCutWindow(MovieCutViewModel viewModel)
        {
            InitializeComponent();
            this.AttachDialog(viewModel);
        }

        /// <summary>
        /// Enter で次の欄に移ります。最後の欄では OK と同じです（以前と同じ）。
        /// </summary>
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return)
            {
                return;
            }
            e.Handled = true;
            TextBox next = null;
            if (sender == leftBox) next = rightBox;
            else if (sender == rightBox) next = topBox;
            else if (sender == topBox) next = bottomBox;
            if (next != null)
            {
                next.Focus();
                next.SelectAll();
            }
            else
            {
                ((MovieCutViewModel)DataContext).OkCommand.Execute(null);
            }
        }
    }
}
