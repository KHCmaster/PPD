using Microsoft.Win32;
using PPDEditor.Forms;
using PPDEditor.ViewModels.Dialogs;
using PPDEditor.ViewModels.Docks;
using PPDFramework.PPDStructure;
using PPDSound;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;

namespace PPDEditor
{
    /// <summary>
    /// ボタンの音と、その割り当てを編集します。処理は <see cref="SoundManagerViewModel"/> にあります。
    /// </summary>
    public sealed partial class SoundManager : DockView, ISoundManagerHost
    {
        Sound sound;

        public SoundManager()
        {
            InitializeComponent();
            AttachViewModel(new SoundManagerViewModel(this));
            ContentSaved();
        }

        public SoundManagerViewModel ViewModel
        {
            get { return (SoundManagerViewModel)DataContext; }
        }

        /// <summary>
        /// 音のファイルのパスです（0 番目は無音）。
        /// </summary>
        public List<string> Sounddatapaths
        {
            get { return ViewModel.SoundPaths; }
        }

        public bool Fixedcombo
        {
            get { return ViewModel.SyncSelection; }
            set { ViewModel.SyncSelection = value; }
        }

        public void SetEvent()
        {
            WindowUtility.Seekmain.Seeked += (sender, e) => ViewModel.OnSeeked((float)WindowUtility.Seekmain.Currenttime);
        }

        public void SetSkin()
        {
            TextElement.SetForeground(skinPanel, WpfSkin.TransparentTextBrush);
        }

        public void SetSound(Sound sound)
        {
            this.sound = sound;
        }

        public void Clear()
        {
            sound.DeleteAllSound();
            ViewModel.ClearAll();
            ContentSaved();
        }

        public void ClearSetting()
        {
            ViewModel.ClearSetting();
            ContentSaved();
        }

        public string Savesoundset()
        {
            return ViewModel.SaveSoundSet();
        }

        public SCDData[] GetSCDData()
        {
            return ViewModel.GetSCDData();
        }

        public float[] GetSoundChangeWithinTime(float starttime, float endtime)
        {
            return ViewModel.GetSoundChangeWithinTime(starttime, endtime);
        }

        public string GetFormattedContent(float time)
        {
            return ViewModel.GetFormattedContent(time);
        }

        public void FocusWithTimeData(float time)
        {
            ViewModel.FocusWithTimeData(time);
        }

        public void ChangeTime(float previoustime, float newtime)
        {
            ViewModel.ChangeTime(previoustime, newtime);
        }

        public void Playsound(bool[] b)
        {
            ViewModel.PlaySounds(b);
        }

        public void Readsounds(string[] filenames)
        {
            ViewModel.ReadSounds(filenames);
        }

        public void Readchangedata(float[] times, ushort[][] changes)
        {
            ViewModel.ReadChangeData(times, changes);
        }

        float ISoundManagerHost.CurrentTime
        {
            get { return (float)WindowUtility.Seekmain.Currenttime; }
        }

        int ISoundManagerHost.GetVolume(int button)
        {
            return WindowUtility.EventManager.GetVolume(button);
        }

        void ISoundManagerHost.LoadSound(string path)
        {
            sound.AddSound(path);
        }

        void ISoundManagerHost.UnloadSound(string path)
        {
            sound.DeleteSound(path);
        }

        void ISoundManagerHost.PlaySound(string path, int volume)
        {
            sound.Play(path, volume);
        }

        string[] ISoundManagerHost.AskSoundFiles(string filter, bool multiselect)
        {
            var dialog = new OpenFileDialog
            {
                Filter = filter,
                Multiselect = multiselect
            };
            return dialog.ShowDialog(Window.GetWindow(this)) == true ? dialog.FileNames : [];
        }

        void ISoundManagerHost.ShowMessage(string text)
        {
            MessageBox.Show(Window.GetWindow(this), text);
        }

        bool ISoundManagerHost.Confirm(string text, string caption)
        {
            return MessageBox.Show(Window.GetWindow(this), text, caption, MessageBoxButton.OKCancel) == MessageBoxResult.OK;
        }

        ushort[] ISoundManagerHost.EditChange(SoundChangeEditViewModel viewModel)
        {
            viewModel.PlaySound += ViewModel.PlayPreview;
            return WpfDialog.ShowDialog(new SoundChangeEditWindow(viewModel)) ? viewModel.Change : null;
        }

        void ISoundManagerHost.ShowSelectedRow()
        {
            changeGrid.ScrollIntoView(changeGrid.SelectedItem);
            ShowDock();
        }

        void ISoundManagerHost.RefreshTimeline()
        {
            WindowUtility.Seekmain.DrawAndRefresh();
        }

        // プロジェクトを開いていて難易度が 2 つ以上あるときの、今の難易度以外の .scd
        string[] ISoundManagerHost.GetOtherDifficultyScdPaths()
        {
            var main = WindowUtility.Editor;
            if (!main.IsProjectLoaded || main.AvailableDifficultyCount <= 1)
            {
                return [];
            }
            return [.. Enum.GetValues<AvailableDifficulty>()
                .Where(diff => diff != AvailableDifficulty.None && main.AvailableDifficulty.HasFlag(diff) && main.CurrentDifficulty != diff)
                .Select(diff => Path.Combine(main.CurrentProjectDir, String.Format("{0}.scd", diff)))];
        }
    }
}
