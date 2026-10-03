using PPDEditor.ViewModels.Docks;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PPDEditor.DockForm.Script
{
    /// <summary>
    /// コマンド、関数、譜面のスクリプトを木で出します。処理は <see cref="ScriptListViewModel"/> にあります。
    /// </summary>
    /// <remarks>
    /// マウスやキーの扱い（右クリックで選ぶ、ダブルクリックで開く、関数のドラッグ）と、名前を変える欄のフォーカスはここで行う。
    /// </remarks>
    public sealed partial class ScriptListPanel : UserControl, IScriptListHost
    {
        const string DragDataFormat = "PPDEditor.ScriptNode";

        Point? dragStartPoint;
        ScriptNode dragStartNode;

        public ScriptListPanel()
        {
            InitializeComponent();
            DataContext = new ScriptListViewModel(this);
        }

        public ScriptListViewModel ViewModel
        {
            get { return (ScriptListViewModel)DataContext; }
        }

        public event Action Reloaded
        {
            add { ViewModel.Reloaded += value; }
            remove { ViewModel.Reloaded -= value; }
        }

        public event Action<string, string> Renamed
        {
            add { ViewModel.Renamed += value; }
            remove { ViewModel.Renamed -= value; }
        }

        public event Action<string> Deleted
        {
            add { ViewModel.Deleted += value; }
            remove { ViewModel.Deleted -= value; }
        }

        public event Action<string> ScriptSelected
        {
            add { ViewModel.ScriptSelected += value; }
            remove { ViewModel.ScriptSelected -= value; }
        }

        public string CurrentScriptPath
        {
            get { return ViewModel.CurrentScriptPath; }
        }

        public bool ChildNodeSelected
        {
            get { return ViewModel.ChildNodeSelected; }
        }

        public void Clear()
        {
            ViewModel.Clear();
        }

        public string[] GetEnabledScriptList()
        {
            return ViewModel.GetEnabledScriptList();
        }

        public string GetDisabledScriptList()
        {
            return ViewModel.GetDisabledScriptList();
        }

        public void WriteScript(Stream stream, string scriptName)
        {
            ViewModel.WriteScript(stream, scriptName);
        }

        public void ReadScript(string projectDirPath, AvailableDifficulty difficulty, string disableList)
        {
            ViewModel.ReadScript(projectDirPath, difficulty, disableList);
        }

        private ScriptNode GetNodeAt(DependencyObject element)
        {
            for (; element != null && element != scriptTree; element = VisualTreeHelper.GetParent(element))
            {
                if (element is TreeViewItem item)
                {
                    return item.DataContext as ScriptNode;
                }
            }
            return null;
        }

        // 右クリックした項目を選ぶ
        private void ScriptTree_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var node = GetNodeAt(e.OriginalSource as DependencyObject);
            if (node != null)
            {
                node.IsSelected = true;
            }
        }

        private void ScriptTree_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.OriginalSource is TextBox || ViewModel.SelectedNode == null)
            {
                return;
            }
            if (e.Key == Key.F2)
            {
                ViewModel.BeginRenameCommand.Execute(null);
                e.Handled = true;
            }
            else if (e.Key == Key.Return && !ViewModel.SelectedNode.IsFolder)
            {
                ViewModel.OpenSelectedCommand.Execute(null);
                e.Handled = true;
            }
        }

        private void ScriptTree_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var node = GetNodeAt(e.OriginalSource as DependencyObject);
            if (node != null && node == ViewModel.SelectedNode)
            {
                ViewModel.OpenSelectedCommand.Execute(null);
            }
        }

        // 名前の変更を始めたら、すぐ入力できるようにする
        private void NameEditor_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var editor = (TextBox)sender;
            if (editor.IsVisible)
            {
                editor.Focus();
                editor.SelectAll();
            }
        }

        private void NameEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Escape)
            {
                e.Handled = true;
                var node = (ScriptNode)((FrameworkElement)sender).DataContext;
                node.EndRenameCommand.Execute(e.Key == Key.Return);
                scriptTree.Focus();
            }
        }

        private void NameEditor_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ((ScriptNode)((FrameworkElement)sender).DataContext).EndRenameCommand.Execute(true);
        }

        private void ScriptTree_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            dragStartPoint = e.GetPosition(scriptTree);
            dragStartNode = GetNodeAt(e.OriginalSource as DependencyObject);
        }

        // 関数を、譜面のスクリプトのフォルダへドラッグしてコピーできる
        private void ScriptTree_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || dragStartPoint == null || dragStartNode == null)
            {
                return;
            }
            var diff = e.GetPosition(scriptTree) - dragStartPoint.Value;
            if (Math.Abs(diff.X) < SystemParameters.MinimumHorizontalDragDistance && Math.Abs(diff.Y) < SystemParameters.MinimumVerticalDragDistance)
            {
                return;
            }
            var node = dragStartNode;
            dragStartPoint = null;
            dragStartNode = null;
            if (!ViewModel.CanDragFunction(node))
            {
                return;
            }
            node.IsSelected = true;
            scriptTree.Focus();
            DragDrop.DoDragDrop(scriptTree, new DataObject(DragDataFormat, node), DragDropEffects.Copy);
        }

        private ScriptNode GetDropTarget(DragEventArgs e, out ScriptNode source)
        {
            source = e.Data.GetDataPresent(DragDataFormat) ? e.Data.GetData(DragDataFormat) as ScriptNode : null;
            var target = GetNodeAt(e.OriginalSource as DependencyObject);
            return ViewModel.CanDropFunction(source, target) ? target : null;
        }

        private void ScriptTree_DragOver(object sender, DragEventArgs e)
        {
            var target = GetDropTarget(e, out _);
            e.Effects = target == null ? DragDropEffects.None : DragDropEffects.Copy;
            target?.IsSelected = true;
            e.Handled = true;
        }

        private void ScriptTree_Drop(object sender, DragEventArgs e)
        {
            var target = GetDropTarget(e, out var source);
            e.Handled = true;
            if (target == null)
            {
                e.Effects = DragDropEffects.None;
                return;
            }
            ViewModel.CopyFunction(source, target);
        }

        bool IScriptListHost.IsProjectLoaded
        {
            get { return WindowUtility.MainForm.IsProjectLoaded; }
        }

        string IScriptListHost.ProjectDir
        {
            get { return WindowUtility.MainForm.CurrentProjectDir; }
        }

        AvailableDifficulty IScriptListHost.CurrentDifficulty
        {
            get { return WindowUtility.MainForm.CurrentDifficulty; }
        }

        void IScriptListHost.ShowMessage(string text)
        {
            MessageBox.Show(Window.GetWindow(this), text);
        }

        bool IScriptListHost.Confirm(string text, string caption)
        {
            return MessageBox.Show(Window.GetWindow(this), text, caption, MessageBoxButton.OKCancel) == MessageBoxResult.OK;
        }
    }
}
