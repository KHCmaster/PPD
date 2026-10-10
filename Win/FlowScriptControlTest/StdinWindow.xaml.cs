using System.Windows;

namespace FlowScriptControlTest
{
    /// <summary>
    /// スクリプトの実行時に渡す標準入力の文字列を編集するウィンドウです。
    /// </summary>
    public partial class StdinWindow : Window
    {
        public StdinWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string InputText
        {
            get;
            set;
        } = "";

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
