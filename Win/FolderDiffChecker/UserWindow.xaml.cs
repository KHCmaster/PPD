using System.IO;
using System.Windows;

namespace FolderDiffChecker
{
    /// <summary>
    /// アップロードに使うユーザー名を入力するウィンドウです（前回の名前を lastUser.txt に覚えておきます）。
    /// </summary>
    public partial class UserWindow : Window
    {
        private const string lastUserNameFilePath = "lastUser.txt";

        public UserWindow()
        {
            InitializeComponent();
        }

        public string UserName
        {
            get
            {
                return userNameTextBox.Text;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (File.Exists(lastUserNameFilePath))
            {
                userNameTextBox.Text = File.ReadAllText(lastUserNameFilePath);
            }
            userNameTextBox.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            File.WriteAllText(lastUserNameFilePath, UserName);
            DialogResult = true;
        }
    }
}
