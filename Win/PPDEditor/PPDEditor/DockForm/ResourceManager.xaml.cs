using Microsoft.Win32;
using PPDEditor.Controls.Resource;
using PPDEditor.Forms;
using PPDEditor.Models;
using PPDEditor.ViewModels.Docks;
using PPDSound;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PPDEditor
{
    /// <summary>
    /// プロジェクトのリソースの一覧です。処理は <see cref="ResourceManagerViewModel"/> にあります。
    /// </summary>
    /// <remarks>
    /// 画像やエフェクトの中身の表示（コントロールにファイルを開かせる）はここで行う。
    /// </remarks>
    public sealed partial class ResourceManager : DockView, IResourceHost
    {
        Sound sound;

        public ResourceManager()
        {
            InitializeComponent();
            var viewModel = new ResourceManagerViewModel(this);
            AttachViewModel(viewModel);
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        public ResourceManagerViewModel ViewModel
        {
            get { return (ResourceManagerViewModel)DataContext; }
        }

        public void SetSound(Sound sound)
        {
            this.sound = sound;
        }

        public void CreateResourceFolders()
        {
            ViewModel.CreateResourceFolders();
        }

        public void Clear()
        {
            ViewModel.Clear();
        }

        public void ReadResource(string projectDirPath, string disableList)
        {
            ViewModel.ReadResource(projectDirPath, disableList);
        }

        public string GetDisableList()
        {
            return ViewModel.GetDisableList();
        }

        public string[] GetEnabledList()
        {
            return ViewModel.GetEnabledList();
        }

        public string[] GetEffectList()
        {
            return ViewModel.GetEffectList();
        }

        public string[] GetImageList()
        {
            return ViewModel.GetImageList();
        }

        public string[] GetSoundList()
        {
            return ViewModel.GetSoundList();
        }

        public string[] GetOthersList()
        {
            return ViewModel.GetOthersList();
        }

        public void WriteResource(Stream stream, string fileName)
        {
            ViewModel.WriteResource(stream, fileName);
        }

        // 画像やエフェクトを選んだら、その中身を表示する
        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(ResourceManagerViewModel.Preview) || ViewModel.Preview == null)
            {
                return;
            }
            if (ViewModel.Preview.Kind == ResourceKind.Image)
            {
                var imagePanel = new ImagePanel();
                imagePanel.OpenFile(ViewModel.Preview.Path);
                previewHost.Content = imagePanel;
            }
            else
            {
                var effectPanel = new EffectPanel();
                effectPanel.SetLang();
                effectPanel.OpenFile(ViewModel.Preview.Path);
                previewHost.Content = effectPanel;
            }
        }

        private void AddMenu_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            ViewModel.RefreshCanAdd();
        }

        // TreeView の選択は結び付けられないので、ここで ViewModel に渡す
        private void ResourceTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ViewModel.SelectedNode = e.NewValue as ResourceNode;
        }

        // 右クリックした項目を選ぶ
        private void ResourceTree_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            for (var element = e.OriginalSource as DependencyObject; element != null && element != resourceTree; element = VisualTreeHelper.GetParent(element))
            {
                if (element is TreeViewItem item)
                {
                    item.IsSelected = true;
                    item.Focus();
                    break;
                }
            }
        }

        // メニューはファイルの項目にだけ出す
        private void ResourceTree_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (ViewModel.SelectedNode == null || ViewModel.SelectedNode.IsFolder)
            {
                e.Handled = true;
            }
        }

        bool IResourceHost.IsProjectLoaded
        {
            get { return WindowUtility.Editor.IsProjectLoaded; }
        }

        string IResourceHost.ProjectDir
        {
            get { return WindowUtility.Editor.CurrentProjectDir; }
        }

        string[] IResourceHost.AskFiles(string filter)
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = filter
            };
            return dialog.ShowDialog(Window.GetWindow(this)) == true ? dialog.FileNames : [];
        }

        void IResourceHost.ShowMessage(string text)
        {
            MessageBox.Show(Window.GetWindow(this), text);
        }

        bool IResourceHost.Confirm(string text, string caption)
        {
            return MessageBox.Show(Window.GetWindow(this), text, caption, MessageBoxButton.OKCancel) == MessageBoxResult.OK;
        }

        void IResourceHost.SetClipboardText(string text)
        {
            Clipboard.SetText(text);
        }

        List<KeyValuePair<object, object>> IResourceHost.GetSoundInfo(string path)
        {
            sound.GetSoundInfo(path, out FMODEX.SOUND_TYPE soundType, out _, out int channelCount, out _, out float length);
            return
            [
                new(Utility.Language["SoundLength"], length),
                new(Utility.Language["ChannelCount"], channelCount),
                new("", soundType)
            ];
        }
    }
}
