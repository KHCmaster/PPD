using PPDEditor.Forms;
using PPDEditor.ViewModels.Dialogs;
using PPDEditor.ViewModels.Docks;
using PPDEditorCommon;
using PPDFramework.PPDStructure.EVDData;
using System.Collections.Generic;
using System.Windows;

namespace PPDEditor
{
    /// <summary>
    /// 時間ごとのイベントを編集します。処理は <see cref="EventManagerViewModel"/> にあります。
    /// </summary>
    public sealed partial class EventManager : DockView, IEventManagerHost
    {
        public EventManager()
        {
            InitializeComponent();
            AttachViewModel(new EventManagerViewModel(this));
        }

        public EventManagerViewModel ViewModel
        {
            get { return (EventManagerViewModel)DataContext; }
        }

        public void SetEvent()
        {
            WindowUtility.Seekmain.Seeked += (sender, e) => ViewModel.OnSeeked((float)WindowUtility.Seekmain.Currenttime);
        }

        public int GetVolume(int i)
        {
            return ViewModel.GetVolume(i);
        }

        public bool ReleaseSound(int i)
        {
            return ViewModel.ReleaseSound(i);
        }

        public DisplayState DisplayState
        {
            get { return ViewModel.DisplayState; }
        }

        public SortedList<float, EventData> ChangeData
        {
            get { return ViewModel.ChangeData; }
            set { ViewModel.ChangeData = value; }
        }

        public float[] GetEventsWithinTime(float starttime, float endtime)
        {
            return ViewModel.GetEventsWithinTime(starttime, endtime);
        }

        public string GetFormattedContent(float time)
        {
            return ViewModel.GetFormattedContent(time);
        }

        public void FocusWithTimeData(float time)
        {
            ViewModel.FocusWithTimeData(time);
        }

        public EventData GetEvent(float time)
        {
            return ViewModel.GetEvent(time);
        }

        public void ChangeTime(float previoustime, float newtime)
        {
            ViewModel.ChangeTime(previoustime, newtime);
        }

        public float GetCorrectTime(float currenttime, float marktime)
        {
            return ViewModel.GetCorrectTime(currenttime, marktime);
        }

        public void Clear()
        {
            ViewModel.Clear();
        }

        public IEVDData[] GetEvdData()
        {
            return ViewModel.GetEvdData();
        }

        public void AddChange(float time, EventData data)
        {
            ViewModel.AddChange(time, data);
        }

        public void CheckInitialBpm(float bpm)
        {
            ViewModel.CheckInitialBpm(bpm);
        }

        float IEventManagerHost.CurrentTime
        {
            get { return (float)WindowUtility.Seekmain.Currenttime; }
        }

        float IEventManagerHost.DefaultBpm
        {
            get { return WindowUtility.IniFileWriter.BPM; }
        }

        EventData IEventManagerHost.EditEvent(EventData eventData)
        {
            var viewModel = new EventEditViewModel(eventData);
            return WpfDialog.ShowDialog(new EventEditWindow(viewModel)) ? viewModel.EventData : null;
        }

        void IEventManagerHost.ShowMessage(string text)
        {
            MessageBox.Show(Window.GetWindow(this), text);
        }

        bool IEventManagerHost.Confirm(string text, string caption, bool yesNo)
        {
            var result = MessageBox.Show(Window.GetWindow(this), text, caption, yesNo ? MessageBoxButton.YesNo : MessageBoxButton.OKCancel);
            return result == MessageBoxResult.Yes || result == MessageBoxResult.OK;
        }

        void IEventManagerHost.ShowSelectedRow()
        {
            eventGrid.ScrollIntoView(eventGrid.SelectedItem);
            ShowDock();
        }

        void IEventManagerHost.RefreshTimeline()
        {
            WindowUtility.Seekmain.DrawAndRefresh();
        }

        void IEventManagerHost.ChangeMovieVolume(int volume)
        {
            WindowUtility.MainForm.ChangeVolume(volume);
        }
    }
}
