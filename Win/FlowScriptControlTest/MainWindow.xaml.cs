using FlowScriptControl.Controls;
using FlowScriptControlTest.Models;
using FlowScriptControlTest.ViewModels;
using Microsoft.Win32;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace FlowScriptControlTest
{
    /// <summary>
    /// フロースクリプトを編集して実行・デバッグするウィンドウです。処理は <see cref="MainWindowViewModel"/> にあります。
    /// </summary>
    /// <remarks>
    /// 右側のノードの一覧・プロパティ・検索・デバッグのパネルは、すべてのタブの編集の領域で共有するので、ウィンドウが持つ。
    /// </remarks>
    public partial class MainWindow : Window, IWindowService
    {
        const string FileFilter = "FSML|*.fsml|All|*.*";

        readonly FlowScriptLanguageProvider languageProvider = FlowScriptLanguageProvider.Load();

        public MainWindow()
        {
            InitializeComponent();
            var viewModel = new MainWindowViewModel(this);
            DataContext = viewModel;
            flowSearchPanel.Searched += viewModel.Search;
            flowSearchPanel.SelectionChanged += viewModel.ShowSearchResult;
        }

        private MainWindowViewModel ViewModel
        {
            get { return (MainWindowViewModel)DataContext; }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = !ViewModel.TryShutdown();
        }

        private void ConsoleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((TextBox)sender).ScrollToEnd();
        }

        public FlowDrawPanel CreateEditor()
        {
            var editor = new FlowDrawPanel
            {
                FlowTreeView = flowTreeView,
                FlowPropertyPanel = flowPropertyPanel
            };
            if (languageProvider != null)
            {
                editor.LanguageProvider = languageProvider;
            }
            return editor;
        }

        public void ShowSearch(string query)
        {
            flowSearchPanel.SearchQuery = query;
            toolTabControl.SelectedItem = searchTabItem;
        }

        public void ClearProperty()
        {
            flowPropertyPanel.MakeEmpty();
        }

        public int? ShowExecutingSource(string filePath, string sourceText)
        {
            var lastSourceId = flowExecutingPropertyPanel.SourceId;
            flowExecutingPropertyPanel.ChangeSource(filePath, sourceText);
            return lastSourceId != flowExecutingPropertyPanel.SourceId ? flowExecutingPropertyPanel.SourceId : null;
        }

        public string AskOpenFile()
        {
            var dialog = new OpenFileDialog { Filter = FileFilter };
            return dialog.ShowDialog(this) == true ? dialog.FileName : null;
        }

        public string AskSaveFile()
        {
            var dialog = new SaveFileDialog { Filter = FileFilter };
            return dialog.ShowDialog(this) == true ? dialog.FileName : null;
        }

        public string AskStdin(string text)
        {
            var window = new StdinWindow { Owner = this, InputText = text };
            return window.ShowDialog() == true ? window.InputText : null;
        }

        public void ShowMessage(string text)
        {
            MessageBox.Show(this, text);
        }

        public bool Confirm(string text, string caption)
        {
            return MessageBox.Show(this, text, caption, MessageBoxButton.OKCancel) != MessageBoxResult.Cancel;
        }
    }
}
