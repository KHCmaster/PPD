using FlowScriptControl.Classes;
using FlowScriptControl.Controls;
using FlowScriptDrawControl.Model;
using FlowScriptEngine;
using Microsoft.Win32;
using PPDConfiguration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace FlowScriptControlTest
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "consoleWriter はスクリプトの実行が終わったときと、ウィンドウを閉じたときに破棄する")]
    public partial class MainWindow : Window
    {
        FlowScriptLanguageProvider langProvider;
        Executor currentExecutor;
        ConsoleWriter consoleWriter;
        string currentStdinText = "";

        public MainWindow()
        {
            InitializeComponent();
            LoadLang();
        }

        public FlowDrawTab CurrentFlowDrawTab
        {
            get
            {
                return drawTabControl.SelectedItem as FlowDrawTab;
            }
        }

        public FlowDrawPanel CurrentFlowDrawPanel
        {
            get
            {
                return CurrentFlowDrawTab?.FlowDrawPanel;
            }
        }

        public FlowDrawTab[] Tabs
        {
            get
            {
                return [.. drawTabControl.Items.OfType<FlowDrawTab>()];
            }
        }

        private void LoadLang()
        {
            var filePath = Path.Combine("Lang", "lang_FlowScript_jp.ini");
            if (!File.Exists(filePath))
            {
                return;
            }

            var flowLang = new SettingReader(File.ReadAllText(filePath));
            filePath = Path.Combine("Lang", "lang_PPDEditor_jp.ini");
            if (!File.Exists(filePath))
            {
                return;
            }
            var lang = new SettingReader(File.ReadAllText(filePath));
            langProvider = new FlowScriptLanguageProvider(lang, flowLang);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FlowDrawPanel.EnumerateClasses("dlls", []);
            FlowDrawPanel.AddFilter(new NodeFilterInfo("Console", [
                NamespaceFilter("FlowScriptEngineBasic"),
                NamespaceFilter("FlowScriptEngineBasicExtension"),
                NamespaceFilter("FlowScriptEngineConsole"),
                NamespaceFilter("FlowScriptEngineData")],
                null));
            FlowDrawPanel.AddFilter(new NodeFilterInfo("PPD", [
                NamespaceFilter("FlowScriptEngineBasic"),
                NamespaceFilter("FlowScriptEngineBasicExtension"),
                NamespaceFilter("FlowScriptEngineSlimDX"),
                NamespaceFilter("FlowScriptEnginePPD"),
                NamespaceFilter("FlowScriptEngineData")],
                null));
            FlowDrawPanel.AddFilter(new NodeFilterInfo("PPD(Script)", [
                NamespaceFilter("FlowScriptEngineBasic"),
                NamespaceFilter("FlowScriptEngineBasicExtension"),
                NamespaceFilter("FlowScriptEngineSlimDX"),
                NamespaceFilter("FlowScriptEnginePPD"),
                NamespaceFilter("FlowScriptEngineData")],
                null, null, [PPDModRegex()]));
            FlowDrawPanel.AddFilter(new NodeFilterInfo("PPD(Mod)", [
                NamespaceFilter("FlowScriptEngineBasic"),
                NamespaceFilter("FlowScriptEngineBasicExtension"),
                NamespaceFilter("FlowScriptEngineSlimDX"),
                NamespaceFilter("FlowScriptEnginePPD"),
                NamespaceFilter("FlowScriptEngineData")],
                null));
            FlowDrawPanel.AddFilter(new NodeFilterInfo("PPDEditor", [
                NamespaceFilter("FlowScriptEngineBasic"),
                NamespaceFilter("FlowScriptEngineBasicExtension"),
                NamespaceFilter("FlowScriptEngineSlimDX"),
                NamespaceFilter("FlowScriptEnginePPDEditor"),
                NamespaceFilter("FlowScriptEngineData")],
                null));
            AddEmptyFlowDrawPanel();
            flowSearchPanel.Searched += FlowSearchPanel_Searched;
            flowSearchPanel.SelectionChanged += FlowSearchPanel_SelectionChanged;
        }

        private void DrawTabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // タブの中のコントロールの選択の変更も届くので、タブ自体の切り替えのときだけ処理する
            if (e.OriginalSource == drawTabControl)
            {
                flowPropertyPanel.MakeEmpty();
            }
        }

        private bool IsDrawPanelFocused
        {
            get { return CurrentFlowDrawPanel != null && CurrentFlowDrawPanel.IsControlFocused; }
        }

        // 編集のショートカットは、スクリプトの編集領域にフォーカスがあるときだけ使う（テキストボックスでは、通常のコピーなどになる）
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (!IsDrawPanelFocused)
            {
                return;
            }
            var modifiers = Keyboard.Modifiers;
            var handled = true;
            if (modifiers == ModifierKeys.Control && e.Key == Key.Z)
            {
                CurrentFlowDrawPanel.Undo();
            }
            else if (modifiers == ModifierKeys.Control && e.Key == Key.Y)
            {
                CurrentFlowDrawPanel.Redo();
            }
            else if (modifiers == ModifierKeys.Control && e.Key == Key.C)
            {
                CurrentFlowDrawPanel.Copy();
            }
            else if (modifiers == ModifierKeys.Control && e.Key == Key.V)
            {
                CurrentFlowDrawPanel.PasteAtCursor();
            }
            else if (modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.V)
            {
                CurrentFlowDrawPanel.PasteWithLinksAtCursor();
            }
            else if (modifiers == ModifierKeys.None && e.Key == Key.Delete)
            {
                CurrentFlowDrawPanel.Delete();
            }
            else
            {
                handled = false;
            }
            e.Handled = handled;
        }

        private void NewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AddEmptyFlowDrawPanel();
        }

        private void UndoMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CurrentFlowDrawPanel?.Undo();
        }

        private void RedoMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CurrentFlowDrawPanel?.Redo();
        }

        private void CopyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CurrentFlowDrawPanel?.Copy();
        }

        private void PasteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CurrentFlowDrawPanel?.PasteAtCursor();
        }

        private void PasteWithLinksMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CurrentFlowDrawPanel?.PasteWithLinksAtCursor();
        }

        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CurrentFlowDrawPanel?.Delete();
        }

        private void AdjustPositionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CurrentFlowDrawPanel?.AdjustPosition();
        }

        private void ExpandAllMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CurrentFlowDrawPanel?.ExpandAll();
        }

        private void CollapseAllMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CurrentFlowDrawPanel?.CollapseAll();
        }

        private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentFlowDrawTab == null)
            {
                return;
            }
            var dialog = new OpenFileDialog { Filter = "FSML|*.fsml|All|*.*" };
            if (dialog.ShowDialog(this) == true)
            {
                using (var stream = File.OpenRead(dialog.FileName))
                {
                    CurrentFlowDrawPanel.LoadXML(stream);
                }
                CurrentFlowDrawTab.FilePath = dialog.FileName;
            }
        }

        private void SaveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentFlowDrawTab == null)
            {
                return;
            }
            var dialog = new SaveFileDialog { Filter = "FSML|*.fsml|All|*.*" };
            if (dialog.ShowDialog(this) == true)
            {
                using var stream = File.Create(dialog.FileName);
                CurrentFlowDrawPanel.SaveXML(stream);
            }
        }

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            if (drawTabControl.Items.Count == 0)
            {
                return;
            }

            var executeInfos = new List<ExecuteInfo>();
            foreach (var flowDrawTab in Tabs)
            {
                var stream = new MemoryStream();
                flowDrawTab.FlowDrawPanel.SaveXML(stream);
                var breakPoints = flowDrawTab.FlowDrawPanel.GetBreakPoints();
                stream.Seek(0, SeekOrigin.Begin);
                executeInfos.Add(new ExecuteInfo(stream, breakPoints, flowDrawTab, flowDrawTab.FlowDrawPanel.GetSources()));
            }
            consoleWriter = new ConsoleWriter();
            consoleWriter.Output += ConsoleWriter_Output;
            currentExecutor = new Executor([.. executeInfos], currentStdinText);
            currentExecutor.Finished += Executor_Finished;
            currentExecutor.Aborted += Executor_Aborted;
            executeButton.IsEnabled = false;
            debugToolBar.Visibility = Visibility.Visible;
            continueButton.IsEnabled = stepInButton.IsEnabled = false;
            currentExecutor.Controller.SourceChanged += Controller_SourceChanged;
            currentExecutor.Controller.OperationAccepted += Controller_OperationAccepted;
            currentExecutor.Controller.OperationWaited += Controller_OperationWaited;
            currentExecutor.Controller.ErrorOccurred += Controller_ErrorOccurred;
            currentExecutor.Execute();
        }

        void Controller_ErrorOccurred(FlowSourceManager arg1, FlowExecutionException arg2)
        {
            var executeInfo = currentExecutor.GetExecuteInfo(arg1);
            Dispatcher.Invoke(() =>
            {
                if (executeInfo != null)
                {
                    var errorText = arg2.ToString();
                    if (arg2.SourceObject != null)
                    {
                        executeInfo.FlowDrawTab.FlowDrawPanel.ShowError(arg2.SourceObject.Id, errorText);
                    }
                    else
                    {
                        MessageBox.Show(this, errorText);
                    }
                }
            });
        }

        void ConsoleWriter_Output(string obj)
        {
            Dispatcher.Invoke(() =>
            {
                consoleTextBox.AppendText(obj);
                consoleTextBox.ScrollToEnd();
            });
        }

        void Controller_OperationWaited(FlowSourceManager obj)
        {
            var executeInfo = currentExecutor.GetExecuteInfo(obj);
            executeInfo?.FlowDrawTab.FlowDrawPanel.SelectionChanged += FlowDrawPanel_SelectionChanged;
            Dispatcher.Invoke(() =>
            {
                continueButton.IsEnabled = stepInButton.IsEnabled = true;
            });
        }

        void Controller_OperationAccepted(FlowSourceManager obj)
        {
            var executeInfo = currentExecutor.GetExecuteInfo(obj);
            executeInfo?.FlowDrawTab.FlowDrawPanel.SelectionChanged -= FlowDrawPanel_SelectionChanged;
            Dispatcher.Invoke(() =>
            {
                continueButton.IsEnabled = stepInButton.IsEnabled = false;
            });
        }

        void Controller_SourceChanged(string arg1, FlowSourceManager arg2)
        {
            Dispatcher.Invoke(() =>
            {
                var executeInfo = currentExecutor.GetExecuteInfo(arg2);
                if (executeInfo != null)
                {
                    var tab = Tabs.FirstOrDefault(t => t == executeInfo.FlowDrawTab);
                    drawTabControl.SelectedItem = tab;
                    var lastSourceId = flowExecutingPropertyPanel.SourceId;
                    flowExecutingPropertyPanel.ChangeSource(executeInfo.FlowDrawTab.FilePath, arg1);
                    if (lastSourceId != flowExecutingPropertyPanel.SourceId)
                    {
                        tab.FlowDrawPanel.SelectAndFocus(flowExecutingPropertyPanel.SourceId);
                    }
                }
            });
        }

        void FlowDrawPanel_SelectionChanged(object sender, EventArgs e)
        {
            var flowDrawPanel = (FlowDrawPanel)sender;
            var ids = flowDrawPanel.GetSelectedSourceId();
            if (ids.Length == 1)
            {
                currentExecutor.Controller.ChangeSource(ids[0]);
            }
        }

        void Executor_Finished()
        {
            Dispatcher.Invoke(() =>
            {
                executeButton.IsEnabled = true;
                debugToolBar.Visibility = Visibility.Collapsed;
                consoleWriter?.Close();
                consoleWriter = null;
            });
        }

        void Executor_Aborted()
        {
            Dispatcher.Invoke(() =>
            {
                MessageBox.Show(this, "スクリプトのプロセスが終了しなかったためKillしました。");
                consoleWriter?.Stop();
                consoleWriter = null;
            });
        }

        private void AddEmptyFlowDrawPanel()
        {
            var flowDrawTab = new FlowDrawTab();
            flowDrawTab.FlowDrawPanel.SearchRequired += FlowDrawPanel_SearchRequired;
            flowDrawTab.FlowDrawPanel.FlowTreeView = flowTreeView;
            flowDrawTab.FlowDrawPanel.FlowPropertyPanel = flowPropertyPanel;
            if (langProvider != null)
            {
                flowDrawTab.FlowDrawPanel.LanguageProvider = langProvider;
            }
            flowDrawTab.CloseRequested += FlowDrawTab_CloseRequested;
            drawTabControl.Items.Add(flowDrawTab);
            drawTabControl.SelectedItem = flowDrawTab;
        }

        void FlowDrawTab_CloseRequested(object sender, EventArgs e)
        {
            var tab = (FlowDrawTab)sender;
            drawTabControl.Items.Remove(tab);
            tab.FlowDrawPanel.Dispose();
        }

        void FlowDrawPanel_SearchRequired(string obj)
        {
            flowSearchPanel.SearchQuery = obj;
            toolTabControl.SelectedItem = searchTabItem;
        }

        void FlowSearchPanel_SelectionChanged(SearchResultItem obj)
        {
            var foundTab = Tabs.FirstOrDefault(tab => tab.FlowDrawPanel.Guid == obj.SearchResult.Guid);
            if (foundTab == null)
            {
                return;
            }
            if (CurrentFlowDrawTab != foundTab)
            {
                drawTabControl.SelectedItem = foundTab;
            }

            if (obj.Source == null)
            {
                foundTab.FlowDrawPanel.SelectAndFocus(obj.Comment);
            }
            else
            {
                foundTab.FlowDrawPanel.SelectAndFocus(obj.Source);
            }
        }

        SearchResult FlowSearchPanel_Searched(string arg)
        {
            return CurrentFlowDrawPanel?.Search(arg);
        }

        private void ClearConsoleMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Console.Clear();
            }
            catch (IOException)
            {
                // コンソールがない（リダイレクトされている）ときは消せない
            }
            consoleTextBox.Clear();
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            currentExecutor.Controller.Continue();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            currentExecutor.Abort();
        }

        private void StepInButton_Click(object sender, RoutedEventArgs e)
        {
            currentExecutor.Controller.StepIn();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (currentExecutor != null && currentExecutor.IsExecuting)
            {
                if (MessageBox.Show(this, "実行中のスクリプトがあります。終了しますか？", "確認", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }

                currentExecutor.Finished -= Executor_Finished;
                currentExecutor.Aborted -= Executor_Aborted;
                currentExecutor.Abort();
            }
            foreach (var tab in Tabs)
            {
                tab.FlowDrawPanel.Dispose();
            }
            consoleWriter?.Stop();
            consoleWriter?.Dispose();
            consoleWriter = null;
        }

        private void StdinMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var window = new StdinWindow
            {
                Owner = this,
                InputText = currentStdinText
            };
            if (window.ShowDialog() == true)
            {
                currentStdinText = window.InputText;
            }
        }

        private static Regex NamespaceFilter(string ns)
        {
            return new Regex("^" + Regex.Escape(ns) + "\\.");
        }

        [GeneratedRegex("^PPD\\.Mod")]
        private static partial Regex PPDModRegex();
    }
}
