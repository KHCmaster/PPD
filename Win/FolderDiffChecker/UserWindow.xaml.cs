using System.Windows;

namespace FolderDiffChecker
{
    /// <summary>
    /// アップロードに使うユーザー名を入力するウィンドウです。
    /// </summary>
    public partial class UserWindow : Window
    {
        public UserWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string UserName
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
