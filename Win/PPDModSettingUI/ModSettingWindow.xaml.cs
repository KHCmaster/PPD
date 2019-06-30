using System.Windows;

namespace PPDModSettingUI
{
    /// <summary>
    /// ModSettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ModSettingWindow : Window
    {
        public ModSettingWindow()
        {
            InitializeComponent();
            Loaded += ModSettingWindow_Loaded;
        }

        void ModSettingWindow_Loaded(object sender, RoutedEventArgs e)
        {
            NewValueTextBox.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}
