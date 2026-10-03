using System.ComponentModel;
using System.Windows;

namespace BMSTOPPD
{
    /// <summary>
    /// ボタンごとのノーツの位置と角度を設定するウィンドウです。値は表に直接書き、アプリの終了時に保存します。
    /// </summary>
    public partial class SettingWindow : Window
    {
        public SettingWindow()
        {
            InitializeComponent();
        }

        // 閉じても設定を保持するため、隠すだけにする
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            dataGrid.CommitEdit();
            e.Cancel = true;
            Hide();
        }
    }
}
