using CommunityToolkit.Mvvm.Messaging;
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

            WeakReferenceMessenger.Default.Register<ShowMessageBoxMessage>(this, (r, m) => ShowMessageBoxMessage_Received(m));
            WeakReferenceMessenger.Default.Register<CloseWindowMessage>(this, (r, m) => CloseWindowMessage_Received(m));
            WeakReferenceMessenger.Default.Register<OpenFileDialogMessage>(this, (r, m) => OpenFileDialogMessage_Received(m));
            WeakReferenceMessenger.Default.Register<FolderBrowserDialogMessage>(this, (r, m) => FolderBrowserDialogMessage_Received(m));
            WeakReferenceMessenger.Default.Register<SelectMessage>(this, (r, m) => SelectMessage_Received(m));

            Unloaded += NewProjectWindow_Unloaded;
        }

        void NewProjectWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            WeakReferenceMessenger.Default.Unregister<ShowMessageBoxMessage>(this);
            WeakReferenceMessenger.Default.Unregister<CloseWindowMessage>(this);
            WeakReferenceMessenger.Default.Unregister<OpenFileDialogMessage>(this);
            WeakReferenceMessenger.Default.Unregister<FolderBrowserDialogMessage>(this);
            WeakReferenceMessenger.Default.Unregister<SelectMessage>(this);
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

            if (elem is FrameworkElement element)
            {
                element.Focus();
                if (elem is TextBox box)
                {
                    box.SelectAll();
                }
            }
        }
    }
}
