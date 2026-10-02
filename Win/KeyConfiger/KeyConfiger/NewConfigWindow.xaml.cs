using System.Windows;
using System.Windows.Controls;

namespace KeyConfiger
{
    /// <summary>
    /// 現在の設定をコピーして作る、新しい設定の名前を入力するウィンドウです。
    /// </summary>
    public partial class NewConfigWindow : Window
    {
        public NewConfigWindow()
        {
            InitializeComponent();
            Loaded += (sender, e) => nameTextBox.Focus();
        }

        public void SetLang()
        {
            nameLabel.Text = Utility.Language["NCWLabel1"];
            noteLabel.Text = Utility.Language["NCWLabel2"];
            Title = Utility.Language["CreateNewSetting"];
        }

        public string SettingName
        {
            get
            {
                return nameTextBox.Text;
            }
        }

        private void NameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            okButton.IsEnabled = nameTextBox.Text.Length > 0;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
