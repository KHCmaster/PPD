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
        }

        public string InputText
        {
            get { return inputTextBox.Text; }
            set { inputTextBox.Text = value; }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
