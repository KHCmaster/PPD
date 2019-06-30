using GalaSoft.MvvmLight.Messaging;
using PPDEditorCommon.Dialog.Message;
using System;
using System.Windows;
using System.Windows.Controls;

namespace PPDEditorCommon.Dialog
{
    /// <summary>
    /// NewProjectWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class NewProjectWindow : Window
    {
        public NewProjectWindow()
        {
            InitializeComponent();

            Messenger.Default.Register<ShowMessageBoxMessage>(this, ShowMessageBoxMessage_Received);
            Messenger.Default.Register<CloseWindowMessage>(this, CloseWindowMessage_Received);
            Messenger.Default.Register<OpenFileDialogMessage>(this, OpenFileDialogMessage_Received);
            Messenger.Default.Register<FolderBrowserDialogMessage>(this, FolderBrowserDialogMessage_Received);
            Messenger.Default.Register<SelectMessage>(this, SelectMessage_Received);

            Unloaded += NewProjectWindow_Unloaded;
        }

        void NewProjectWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            Messenger.Default.Unregister<ShowMessageBoxMessage>(this, ShowMessageBoxMessage_Received);
            Messenger.Default.Unregister<CloseWindowMessage>(this, CloseWindowMessage_Received);
            Messenger.Default.Unregister<OpenFileDialogMessage>(this, OpenFileDialogMessage_Received);
            Messenger.Default.Unregister<FolderBrowserDialogMessage>(this, FolderBrowserDialogMessage_Received);
            Messenger.Default.Unregister<SelectMessage>(this, SelectMessage_Received);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            IconHelper.RemoveIcon(this);
            Utility.HideMinimizeAndMaximizeButtons(this);
        }

        private void ShowMessageBoxMessage_Received(ShowMessageBoxMessage message)
        {
            if (message.Sender != DataContext)
            {
                return;
            }

            MessageBox.Show(message.Text);
        }

        private void CloseWindowMessage_Received(CloseWindowMessage message)
        {
            if (message.Sender != DataContext)
            {
                return;
            }

            DialogResult = message.Result;
            this.Close();
        }

        private void OpenFileDialogMessage_Received(OpenFileDialogMessage message)
        {
            if (message.Sender != DataContext)
            {
                return;
            }

            var dialog = new System.Windows.Forms.OpenFileDialog
            {
                Filter = message.Filter,
                FileName = message.FileName,
                RestoreDirectory = true
            };
            message.Result = dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK;
            if (message.Result)
            {
                message.FileName = dialog.FileName;
            }
            dialog.Dispose();
        }

        private void FolderBrowserDialogMessage_Received(FolderBrowserDialogMessage message)
        {
            if (message.Sender != DataContext)
            {
                return;
            }

            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                SelectedPath = message.SelectedPath
            };
            message.Result = dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK;
            if (message.Result)
            {
                message.SelectedPath = dialog.SelectedPath;
            }
            dialog.Dispose();
        }

        private void SelectMessage_Received(SelectMessage message)
        {
            if (message.Sender != DataContext)
            {
                return;
            }

            var elem = FindName(message.ElementName);
            if (elem == null)
            {
                return;
            }

            if (elem is FrameworkElement)
            {
                ((FrameworkElement)elem).Focus();
                if (elem is TextBox)
                {
                    ((TextBox)elem).SelectAll();
                }
            }
        }
    }
}
