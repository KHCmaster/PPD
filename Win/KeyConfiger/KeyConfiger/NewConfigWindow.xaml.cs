using KeyConfiger.ViewModels;
using System.Windows;

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
            DataContext = this;
        }

        public LanguageTexts Texts { get; } = new();

        public string SettingName
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
