using PPDConfiguration;
using PPDEditor.Forms;
using PPDEditor.ViewModels.Docks;
using PPDFramework;
using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace PPDEditor
{
    /// <summary>
    /// 譜面の設定ファイル（ini）に書く値を編集します。処理は <see cref="IniFileWriterViewModel"/> にあります。
    /// </summary>
    public partial class IniFileWriter : DockView
    {
        public IniFileWriter()
        {
            InitializeComponent();
            AttachViewModel(new IniFileWriterViewModel(
                () => (float)WindowUtility.Seekmain.Currenttime,
                bpm => WindowUtility.EventManager.CheckInitialBpm(bpm),
                text => MessageBox.Show(Window.GetWindow(this), text)));
        }

        public IniFileWriterViewModel ViewModel
        {
            get { return (IniFileWriterViewModel)DataContext; }
        }

        public void SetSkin()
        {
            TextElement.SetForeground(rowsControl, WpfSkin.TransparentTextBrush);
        }

        // BPM を変えたら、イベントの最初の BPM を確かめる（以前は表の編集を終えたとき）
        private void ValueBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is IniRow { IsBpm: true })
            {
                ViewModel.CheckBpm();
            }
        }

        public bool SaveIni(SettingWriter sw)
        {
            return ViewModel.SaveIni(sw);
        }

        public void Clear()
        {
            ViewModel.Clear();
        }

        public void SetIni(float[] times, string[] difficults, float bpm, string bpmstring, MovieTrimmingData trimmingData)
        {
            ViewModel.SetIni(times, difficults, bpm, bpmstring, trimmingData);
        }

        public void SetTrimmingData(MovieTrimmingData trimmingData)
        {
            ViewModel.SetTrimmingData(trimmingData);
        }

        public float BPM
        {
            get { return ViewModel.BPM; }
        }

        public float StartTime
        {
            get { return ViewModel.StartTime; }
        }

        public float EndTime
        {
            get { return ViewModel.EndTime; }
        }

        public Guid SongGuid
        {
            get { return ViewModel.SongGuid; }
            set { ViewModel.SongGuid = value; }
        }
    }
}
