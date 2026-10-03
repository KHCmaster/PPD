using PPDEditor.Forms;
using PPDEditorCommon;
using PPDFramework.PPDStructure.EVDData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace PPDEditor
{
    public partial class EventManager : DockView
    {
        SortedList<float, EventData> changeData;
        int currentnum;
        readonly int[] Volpercents = new int[10];
        readonly bool[] ReleaseSounds = new bool[10];
        DisplayState dstate = DisplayState.Normal;
        readonly List<float[]> stopList;
        string alreadyadded = "すでにこの時間にイベントが設定されています。削除してからもう一度追加してください";
        string confirmdelete = "削除してもよろしいですか？";
        string confirm = "確認";
        string noselectedline = "選択された行がありません";
        public EventManager()
        {
            InitializeComponent();
            changeData = [];
            stopList = [];
            for (int i = 0; i < Volpercents.Length; i++)
            {
                Volpercents[i] = 90;
                ReleaseSounds[i] = false;
            }
            dstate = DisplayState.Normal;
        }
        public void SetEvent()
        {
            WindowUtility.Seekmain.Seeked += Sm_onmoveseek;
        }
        public void SetLang()
        {
            this.Column1.Header = Utility.Language["EMDataGridColumn1"];
            this.Column2.Header = Utility.Language["EMDataGridColumn2"];
            this.button1.Content = Utility.Language["EMButton1"];
            this.button2.Content = Utility.Language["EMButton2"];
            this.button3.Content = Utility.Language["EMButton3"];
            this.button4.Content = Utility.Language["EMButton4"];
            this.label1.Text = Utility.Language["EMLabel1"];
            alreadyadded = Utility.Language["EMAlreadyAdded"];
            confirmdelete = Utility.Language["EMDeleteConfirm"];
            confirm = Utility.Language["EMConfirm"];
            noselectedline = Utility.Language["EMNoSelectedLine"];
        }
        public int GetVolume(int i)
        {
            return -100 * (100 - Volpercents[i]);
        }
        public bool ReleaseSound(int i)
        {
            return ReleaseSounds[i];
        }
        public DisplayState DisplayState
        {
            get
            {
                return dstate;
            }
        }
        public SortedList<float, EventData> ChangeData
        {
            get
            {
                var ret = new SortedList<float, EventData>();
                foreach (KeyValuePair<float, EventData> kvp in changeData)
                {
                    ret.Add(kvp.Key, kvp.Value);
                }
                return ret;
            }
            set
            {
                if (changeData != value)
                {
                    changeData = value;
                    UpdateDataGrid();
                    ContentChanged();
                }
            }
        }
        public float[] GetEventsWithinTime(float starttime, float endtime)
        {
            var ret = new List<float>();
            foreach (float time in changeData.Keys)
            {
                if (starttime <= time)
                {
                    if (time <= endtime)
                    {
                        ret.Add(time);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return [.. ret];
        }
        public string GetFormattedContent(float time)
        {
            string ret;
            if (changeData.TryGetValue(time, out EventData evd))
            {
                ret = evd.GetFormattedContent();
            }
            else
            {
                ret = "";
            }
            return ret;
        }
        public void FocusWithTimeData(float time)
        {
            if (changeData.ContainsKey(time))
            {
                var index = changeData.IndexOfKey(time);
                if (index >= 0)
                {
                    dataGridView1.SelectedIndex = index;
                    dataGridView1.ScrollIntoView(dataGridView1.SelectedItem);
                    ShowDock();
                }
            }
        }
        public EventData GetEvent(float time)
        {
            EventData last = null;
            foreach (KeyValuePair<float, EventData> pair in changeData)
            {
                if (pair.Key > time)
                {
                    break;
                }
                last = pair.Value;
            }
            return last;
        }
        public void ChangeTime(float previoustime, float newtime)
        {
            if (previoustime < 0) return;
            if (changeData.TryGetValue(previoustime, out EventData ed) && !changeData.ContainsKey(newtime))
            {
                changeData.Remove(previoustime);
                changeData.Add(newtime, ed);
                UpdateDataGrid();
                ContentChanged();
                WindowUtility.Seekmain.DrawAndRefresh();
            }
        }
        public float GetCorrectTime(float currenttime, float marktime)
        {
            float ret = currenttime;
            for (int i = 0; i < stopList.Count; i++)
            {
                float[] temp = stopList[i];
                if (marktime <= temp[1]) break;
                if (currenttime >= temp[1]) continue;
                if (currenttime >= temp[0] && currenttime <= temp[1])
                {
                    ret = temp[1];
                }
                if (currenttime < temp[0])
                {
                    ret += temp[1] - temp[0];
                }
            }
            return ret;
        }
        public void Clear()
        {
            stopList.Clear();
            changeData.Clear();
            UpdateDataGrid();
            for (int i = 0; i < Volpercents.Length; i++)
            {
                Volpercents[i] = 90;
                ReleaseSounds[i] = false;
            }
            dstate = DisplayState.Normal;
        }
        public IEVDData[] GetEvdData()
        {
            var ret = new List<IEVDData>();
            var lastevent = new EventData
            {
                BPM = WindowUtility.IniFileWriter.BPM
            };
            for (int i = 0; i < changeData.Count; i++)
            {
                EventData nowevent = changeData.Values[i];
                float time = changeData.Keys[i];
                if (nowevent.MovieVolumePercent != lastevent.MovieVolumePercent)
                {
                    ret.Add(new ChangeVolumeEvent(time, (byte)nowevent.MovieVolumePercent, 0));
                }
                if (nowevent.SquareVolumePercent != lastevent.SquareVolumePercent)
                {
                    ret.Add(new ChangeVolumeEvent(time, (byte)nowevent.SquareVolumePercent, 1));
                }
                if (nowevent.CrossVolumePercent != lastevent.CrossVolumePercent)
                {
                    ret.Add(new ChangeVolumeEvent(time, (byte)nowevent.CrossVolumePercent, 2));
                }
                if (nowevent.CircleVolumePercent != lastevent.CircleVolumePercent)
                {
                    ret.Add(new ChangeVolumeEvent(time, (byte)nowevent.CircleVolumePercent, 3));
                }
                if (nowevent.TriangleVolumePercent != lastevent.TriangleVolumePercent)
                {
                    ret.Add(new ChangeVolumeEvent(time, (byte)nowevent.TriangleVolumePercent, 4));
                }
                if (nowevent.LeftVolumePercent != lastevent.LeftVolumePercent)
                {
                    ret.Add(new ChangeVolumeEvent(time, (byte)nowevent.LeftVolumePercent, 5));
                }
                if (nowevent.DownVolumePercent != lastevent.DownVolumePercent)
                {
                    ret.Add(new ChangeVolumeEvent(time, (byte)nowevent.DownVolumePercent, 6));
                }
                if (nowevent.RightVolumePercent != lastevent.RightVolumePercent)
                {
                    ret.Add(new ChangeVolumeEvent(time, (byte)nowevent.RightVolumePercent, 7));
                }
                if (nowevent.UpVolumePercent != lastevent.UpVolumePercent)
                {
                    ret.Add(new ChangeVolumeEvent(time, (byte)nowevent.UpVolumePercent, 8));
                }
                if (nowevent.RVolumePercent != lastevent.RVolumePercent)
                {
                    ret.Add(new ChangeVolumeEvent(time, (byte)nowevent.RVolumePercent, 9));
                }
                if (nowevent.LVolumePercent != lastevent.LVolumePercent)
                {
                    ret.Add(new ChangeVolumeEvent(time, (byte)nowevent.LVolumePercent, 10));
                }
                if (nowevent.BPM != lastevent.BPM)
                {
                    if (!nowevent.BPMRapidChange)
                    {
                        ret.Add(new ChangeBPMEvent(time, nowevent.BPM));
                    }
                    else
                    {
                        ret.Add(new RapidChangeBPMEvent(time, nowevent.BPM, nowevent.BPMRapidChange));
                    }
                }
                if (nowevent.SquareKeepPlaying != lastevent.SquareKeepPlaying)
                {
                    ret.Add(new ChangeSoundPlayModeEvent(time, nowevent.SquareKeepPlaying, 1));
                }
                if (nowevent.CrossKeepPlaying != lastevent.CrossKeepPlaying)
                {
                    ret.Add(new ChangeSoundPlayModeEvent(time, nowevent.CrossKeepPlaying, 2));
                }
                if (nowevent.CircleKeepPlaying != lastevent.CircleKeepPlaying)
                {
                    ret.Add(new ChangeSoundPlayModeEvent(time, nowevent.CircleKeepPlaying, 3));
                }
                if (nowevent.TriangleKeepPlaying != lastevent.TriangleKeepPlaying)
                {
                    ret.Add(new ChangeSoundPlayModeEvent(time, nowevent.TriangleKeepPlaying, 4));
                }
                if (nowevent.LeftKeepPlaying != lastevent.LeftKeepPlaying)
                {
                    ret.Add(new ChangeSoundPlayModeEvent(time, nowevent.LKeepPlaying, 5));
                }
                if (nowevent.DownKeepPlaying != lastevent.DownKeepPlaying)
                {
                    ret.Add(new ChangeSoundPlayModeEvent(time, nowevent.DownKeepPlaying, 6));
                }
                if (nowevent.RightKeepPlaying != lastevent.RightKeepPlaying)
                {
                    ret.Add(new ChangeSoundPlayModeEvent(time, nowevent.RightKeepPlaying, 7));
                }
                if (nowevent.UpKeepPlaying != lastevent.UpKeepPlaying)
                {
                    ret.Add(new ChangeSoundPlayModeEvent(time, nowevent.UpKeepPlaying, 8));
                }
                if (nowevent.RKeepPlaying != lastevent.RKeepPlaying)
                {
                    ret.Add(new ChangeSoundPlayModeEvent(time, nowevent.RKeepPlaying, 9));
                }
                if (nowevent.LKeepPlaying != lastevent.LKeepPlaying)
                {
                    ret.Add(new ChangeSoundPlayModeEvent(time, nowevent.LKeepPlaying, 10));
                }

                if (nowevent.DisplayState != lastevent.DisplayState)
                {
                    ret.Add(new ChangeDisplayStateEvent(time, nowevent.DisplayState));
                }
                if (nowevent.MoveState != lastevent.MoveState)
                {
                    ret.Add(new ChangeMoveStateEvent(time, nowevent.MoveState));
                }

                if (nowevent.SquareReleaseSound != lastevent.SquareReleaseSound)
                {
                    ret.Add(new ChangeReleaseSoundEvent(time, nowevent.SquareReleaseSound, 1));
                }
                if (nowevent.CrossReleaseSound != lastevent.CrossReleaseSound)
                {
                    ret.Add(new ChangeReleaseSoundEvent(time, nowevent.CrossReleaseSound, 2));
                }
                if (nowevent.CircleReleaseSound != lastevent.CircleReleaseSound)
                {
                    ret.Add(new ChangeReleaseSoundEvent(time, nowevent.CircleReleaseSound, 3));
                }
                if (nowevent.TriangleReleaseSound != lastevent.TriangleReleaseSound)
                {
                    ret.Add(new ChangeReleaseSoundEvent(time, nowevent.TriangleReleaseSound, 4));
                }
                if (nowevent.LeftReleaseSound != lastevent.LeftReleaseSound)
                {
                    ret.Add(new ChangeReleaseSoundEvent(time, nowevent.LeftReleaseSound, 5));
                }
                if (nowevent.DownReleaseSound != lastevent.DownReleaseSound)
                {
                    ret.Add(new ChangeReleaseSoundEvent(time, nowevent.DownReleaseSound, 6));
                }
                if (nowevent.RightReleaseSound != lastevent.RightReleaseSound)
                {
                    ret.Add(new ChangeReleaseSoundEvent(time, nowevent.RightReleaseSound, 7));
                }
                if (nowevent.UpReleaseSound != lastevent.UpReleaseSound)
                {
                    ret.Add(new ChangeReleaseSoundEvent(time, nowevent.UpReleaseSound, 8));
                }
                if (nowevent.RReleaseSound != lastevent.RReleaseSound)
                {
                    ret.Add(new ChangeReleaseSoundEvent(time, nowevent.RReleaseSound, 9));
                }
                if (nowevent.LReleaseSound != lastevent.LReleaseSound)
                {
                    ret.Add(new ChangeReleaseSoundEvent(time, nowevent.LReleaseSound, 10));
                }
                if (nowevent.NoteType != lastevent.NoteType)
                {
                    ret.Add(new ChangeNoteTypeEvent(time, nowevent.NoteType));
                }
                if (nowevent.SlideScale != lastevent.SlideScale)
                {
                    ret.Add(new ChangeSlideScaleEvent(time, nowevent.SlideScale));
                }
                if (!Utility.IsSameArray(nowevent.InitializeOrder, lastevent.InitializeOrder))
                {
                    ret.Add(new ChangeInitializeOrderEvent(time, nowevent.InitializeOrder));
                }

                lastevent = nowevent;
            }

            return [.. ret];
        }
        public void AddChange(float time, EventData data)
        {
            if (!changeData.ContainsKey(time))
            {
                changeData.Add(time, data);
                UpdateDataGrid();
                ContentChanged();
            }
        }
        private void Sm_onmoveseek(object sender, EventArgs e)
        {
            var time = (float)WindowUtility.Seekmain.Currenttime;
            ChangeDataGridSelectedIndex(time);
        }
        private void ChangeDataGridSelectedIndex(float time)
        {
            int i;
            for (i = 0; i < changeData.Count; i++)
            {
                if (changeData.Keys[i] <= time) continue;
                if (changeData.Keys[i] >= time)
                {
                    i--;
                    break;
                }
            }
            if (i == changeData.Count)
            {
                i--;
            }
            /*if (i < 0)
            {
                i = 0;
            }*/
            if (i >= 0 && i < changeData.Count)
            {
                dataGridView1.SelectedIndex = i;
                SetEvent(changeData.Values[i]);
                currentnum = i;
            }
            else
            {
                dataGridView1.SelectedIndex = -1;
                var temp = new EventData();
                SetEvent(temp);
                currentnum = -1;
            }
        }
        private void SetEvent(EventData data)
        {
            Volpercents[0] = data.SquareVolumePercent;
            Volpercents[1] = data.CrossVolumePercent;
            Volpercents[2] = data.CircleVolumePercent;
            Volpercents[3] = data.TriangleVolumePercent;
            Volpercents[4] = data.LeftVolumePercent;
            Volpercents[5] = data.DownVolumePercent;
            Volpercents[6] = data.RightVolumePercent;
            Volpercents[7] = data.UpVolumePercent;
            Volpercents[8] = data.RVolumePercent;
            Volpercents[9] = data.LVolumePercent;

            ReleaseSounds[0] = data.SquareReleaseSound;
            ReleaseSounds[1] = data.CrossReleaseSound;
            ReleaseSounds[2] = data.CircleReleaseSound;
            ReleaseSounds[3] = data.TriangleReleaseSound;
            ReleaseSounds[4] = data.LeftReleaseSound;
            ReleaseSounds[5] = data.DownReleaseSound;
            ReleaseSounds[6] = data.RightReleaseSound;
            ReleaseSounds[7] = data.UpReleaseSound;
            ReleaseSounds[8] = data.RReleaseSound;
            ReleaseSounds[9] = data.LReleaseSound;
            WindowUtility.MainForm.ChangeVolume(-100 * (100 - data.MovieVolumePercent));
            dstate = data.DisplayState;
        }
        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            //add
            var time = (float)WindowUtility.Seekmain.Currenttime;
            if (!changeData.ContainsKey(time))
            {
                var eef = new EventEditWindow();
                eef.SetLang();
                if (changeData.Count == 0 || currentnum < 0)
                {
                    var temp = new EventData
                    {
                        BPM = WindowUtility.IniFileWriter.BPM
                    };
                    eef.EventData = temp;
                }
                else
                {
                    if (currentnum >= 0 && currentnum < changeData.Count)
                    {
                        eef.EventData = changeData.Values[currentnum].Clone();
                    }
                }
                if (WpfDialog.ShowDialog(eef))
                {
                    changeData.Add(time, eef.EventData);
                    UpdateDataGrid();
                    ContentChanged();
                }
            }
            else
            {
                MessageBox.Show(Window.GetWindow(this), alreadyadded);
            }
        }
        private void UpdateDataGrid()
        {
            stopList.Clear();
            float stopstarttime = -1;
            // 選んでいる行は、行の番号で保つ（以前の DataGridView と同じ）
            var selectedIndex = dataGridView1.SelectedIndex;
            dataGridView1.ItemsSource = changeData.Select(p => new KeyValuePair<float, string>(p.Key, p.Value.ToString())).ToList();
            if (selectedIndex >= 0 && selectedIndex < changeData.Count)
            {
                dataGridView1.SelectedIndex = selectedIndex;
            }
            for (int i = 0; i < changeData.Count; i++)
            {
                if (stopstarttime < 0)
                {
                    if (changeData.Values[i].MoveState == MoveState.Stop)
                    {
                        stopstarttime = changeData.Keys[i];
                    }
                }
                else
                {
                    if (changeData.Values[i].MoveState == MoveState.Normal)
                    {
                        var temp = new float[] { stopstarttime, changeData.Keys[i] };
                        stopList.Add(temp);
                        stopstarttime = -1;
                    }
                }
            }
        }

        /// <summary>
        /// 選んでいる行の番号です。選んでいないときは -1 です。
        /// </summary>
        private int CurrentIndex
        {
            get { return dataGridView1.SelectedIndex; }
        }

        public void CheckInitialBpm(float bpm)
        {
            if (changeData.ContainsKey(0))
            {
                if (changeData[0].BPM != bpm)
                {
                    if (MessageBox.Show(Window.GetWindow(this), Utility.Language["EMCheckBpmText"], Utility.Language["EMCheckBpm"], MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        changeData[0].BPM = bpm;
                        UpdateDataGrid();
                        ContentChanged();
                    }
                }
            }
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            //edit
            if (CurrentIndex >= 0)
            {
                var eef = new EventEditWindow();
                eef.SetLang();
                eef.EventData = changeData.Values[CurrentIndex].Clone();
                if (WpfDialog.ShowDialog(eef))
                {
                    float time = changeData.Keys[CurrentIndex];
                    changeData.RemoveAt(CurrentIndex);
                    changeData.Add(time, eef.EventData);
                    UpdateDataGrid();
                    ContentChanged();
                }
            }
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            //delete
            if (CurrentIndex >= 0)
            {
                if (MessageBox.Show(Window.GetWindow(this), confirmdelete, confirm, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    changeData.RemoveAt(CurrentIndex);
                    UpdateDataGrid();
                    ContentChanged();
                }
            }
        }

        private void Button4_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentIndex < 0)
            {
                MessageBox.Show(Window.GetWindow(this), noselectedline);
            }
            else
            {
                float time = changeData.Keys[CurrentIndex];
                if (!changeData.ContainsKey(time + (float)this.numericUpDown1.Value))
                {
                    EventData eve = changeData.Values[CurrentIndex];
                    changeData.RemoveAt(CurrentIndex);
                    changeData.Add(time + (float)this.numericUpDown1.Value, eve);
                    UpdateDataGrid();
                    ContentChanged();
                }
            }
        }
    }
}
