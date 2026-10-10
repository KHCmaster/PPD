using PPDEditor.Forms;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace PPDEditor
{
    /// <summary>
    /// ヘルプ（同じフォルダーの HTML）を表示します。
    /// </summary>
    public partial class HelpForm : DockView
    {
        public HelpForm()
        {
            InitializeComponent();
        }

        void WebBrowser1_LoadCompleted(object sender, NavigationEventArgs e)
        {
            toolStripButton3.Visibility = Visibility.Collapsed;
        }

        void WebBrowser1_Navigated(object sender, NavigationEventArgs e)
        {
            toolStripTextBox1.Text = e.Uri?.ToString() ?? "";
            toolStripButton1.IsEnabled = webBrowser1.CanGoBack;
            toolStripButton2.IsEnabled = webBrowser1.CanGoForward;
            toolStripButton3.Visibility = Visibility.Visible;
        }

        public void ChangeUrl(string url)
        {
            Navigate(url);
        }

        private void Navigate(string url)
        {
            try
            {
                webBrowser1.Navigate(url);
            }
            catch (UriFormatException)
            {
            }
        }

        private void ToolStripButton1_Click(object sender, RoutedEventArgs e)
        {
            if (webBrowser1.CanGoBack)
            {
                webBrowser1.GoBack();
            }
        }

        private void ToolStripButton2_Click(object sender, RoutedEventArgs e)
        {
            if (webBrowser1.CanGoForward)
            {
                webBrowser1.GoForward();
            }
        }

        private void ToolStripTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Navigate(toolStripTextBox1.Text);
                e.Handled = true;
            }
        }
    }
}
