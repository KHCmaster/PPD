using Microsoft.Win32;
using PPDEditor.Command.PPDSheet;
using PPDEditor.Forms;
using PPDEditor.ViewModels.Dialogs;
using PPDEditor.ViewModels.Docks;
using PPDEditorCommon;
using System;
using System.Collections.Generic;
using System.Windows;

namespace PPDEditor
{
    /// <summary>
    /// 動画とマークの情報、マークのパラメーターを表示します。処理は <see cref="InfoViewModel"/> にあります。
    /// </summary>
    public sealed partial class InfoForm : DockView, IInfoEditor
    {
        public InfoForm()
        {
            InitializeComponent();
            AttachViewModel(new InfoViewModel(this));
        }

        public InfoViewModel ViewModel
        {
            get { return (InfoViewModel)DataContext; }
        }

        public DateTime PresetsLoadTime
        {
            get { return ViewModel.PresetsLoadTime; }
        }

        public ParameterPresetFolder PresetRoot
        {
            get { return ViewModel.PresetRoot; }
        }

        public ParameterPreset[] Presets
        {
            get { return ViewModel.Presets; }
        }

        public void SetSkin()
        {
        }

        public void SetMovieInfo(string width, string height, string length)
        {
            ViewModel.SetMovieInfo(width, height, length);
        }

        public void SetMarkInfo(string pos, string angle, string id)
        {
            ViewModel.SetMarkInfo(pos, angle, id);
        }

        public void UpdateParameters(Mark[] marks)
        {
            ViewModel.UpdateParameters(marks);
        }

        public void ApplyParameters(KeyValuePair<string, string>[] parameters)
        {
            ViewModel.ApplyParameters(parameters);
        }

        public void UnapplyParameters(KeyValuePair<string, string>[] parameters)
        {
            ViewModel.UnapplyParameters(parameters);
        }

        // サブメニューの中のメニューを開いたときにも来るので、自分のときだけ処理する
        private void EditMenu_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource == sender)
            {
                ViewModel.RefreshEditMenu();
            }
        }

        private void PresetMenu_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource == sender)
            {
                ViewModel.RefreshPresetMenu();
            }
        }

        Mark[] IInfoEditor.GetSelectedMarks()
        {
            return Utility.GetSelectedMarks();
        }

        PPDSheet IInfoEditor.SelectedSheet
        {
            get { return WindowUtility.LayerManager.SelectedPpdSheet; }
        }

        void IInfoEditor.ShowMessage(string text)
        {
            MessageBox.Show(Window.GetWindow(this), text);
        }

        Dictionary<string, string> IInfoEditor.EditParameters(Dictionary<string, string> parameters, string[] existKeys)
        {
            var viewModel = new EditParameterViewModel(parameters, existKeys);
            return WpfDialog.ShowDialog(new EditParameterWindow(viewModel)) ? viewModel.Parameters : null;
        }

        string IInfoEditor.AskPresetFilePath(string initialDirectory, string fileName)
        {
            var dialog = new SaveFileDialog
            {
                InitialDirectory = initialDirectory,
                FileName = fileName
            };
            return dialog.ShowDialog(Window.GetWindow(this)) == true ? dialog.FileName : null;
        }
    }
}
