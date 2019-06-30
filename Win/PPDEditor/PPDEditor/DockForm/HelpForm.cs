using System;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace PPDEditor
{
    public partial class HelpForm : DockContent
    {
        public HelpForm()
        {
            InitializeComponent();
            webBrowser1.Navigated += webBrowser1_Navigated;
            webBrowser1.ProgressChanged += webBrowser1_ProgressChanged;
        }

        void webBrowser1_ProgressChanged(object sender, WebBrowserProgressChangedEventArgs e)
        {
            toolStripButton3.Visible &= e.CurrentProgress != e.MaximumProgress;
        }

        void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            toolStripTextBox1.Text = webBrowser1.Url.ToString();
            toolStripButton1.Enabled = webBrowser1.CanGoBack;
            toolStripButton2.Enabled = webBrowser1.CanGoForward;
            toolStripButton3.Visible = true;
        }

        public void ChangeUrl(string url)
        {
            webBrowser1.Navigate(url);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (webBrowser1.CanGoBack)
            {
                webBrowser1.GoBack();
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (webBrowser1.CanGoForward)
            {
                webBrowser1.GoForward();
            }
        }

        private void toolStripTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                webBrowser1.Navigate(toolStripTextBox1.Text);
            }
        }

        private void toolStripTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r' || e.KeyChar == '\n')
            {
                e.Handled = true;
            }
        }
    }
}
