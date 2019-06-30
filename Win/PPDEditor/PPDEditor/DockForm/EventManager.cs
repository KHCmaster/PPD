using PPDEditor.Forms;
using PPDEditorCommon;
using PPDFramework.PPDStructure.EVDData;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PPDEditor
{
    public partial class EventManager : ScrollableForm
    {
        SortedList<float, EventData> changeData;
        int currentnum;
        int[] Volpercents = new int[10];
        bool[] ReleaseSounds = new bool[10];
        DisplayState dstate = DisplayState.Normal;
        List<float[]> stopList;
        string alreadyadded = "すでにこの時間にイベントが設定されています。削除してからもう一度追加してください";
        string confirmdelete = "削除してもよろしいですか？";
        string confirm = "確認";
        string noselectedline = "選択された行がありません";
        public EventManager()
        {
            InitializeComponent();
            changeData = new SortedList<float, EventData>();
            stopList = new List<float[]>();
            for (int i = 0; i < Volpercents.Length; i++)
            {
                Volpercents[i] = 90;
                ReleaseSounds[i] = false;
            }
            dstate = DisplayState.Normal;
        }
        public void SetEvent()
        {
            WindowUtility.Seekmain.Seeked += sm_onmoveseek;
        }
        public void SetLang()
        {
            this.dataGridView1.Columns[0].HeaderText = Utility.Language["EMDataGridColumn1"];
            this.dataGridView1.Columns[1].HeaderText = Utility.Language["EMDataGridColumn2"];
            this.button1.Text = Utility.Language["EMButton1"];
            this.button2.Text = Utility.Language["EMButton2"];
            this.button3.Text = Utility.Language["EMButton3"];
            this.button4.Text = Utility.Language["EMButton4"];
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
                    this.dataGridView1.RowCount = 0;
                    this.dataGridView1.Rows.Add(changeData.Count);
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
            return ret.ToArray();
        }
        public string GetFormattedContent(float time)
        {
            string ret;
            if (changeData.ContainsKey(time))
            {
                EventData evd = changeData[time];
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
                    dataGridView1.Rows[index].Selected = true;
                    dataGridView1.FirstDisplayedScrollingRowIndex = index;
                    ShowOrHideWindow(this);
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
            if (changeData.ContainsKey(previoustime) && !changeData.ContainsKey(newtime))
            {
                EventData ed = changeData[previoustime];
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
            this.dataGridView1.RowCount = 0;
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

            return ret.ToArray();
        }
        public void AddChange(float time, EventData data)
        {
            if (!changeData.ContainsKey(time))
            {
                changeData.Add(time, data);
                this.dataGridView1.Rows.Add(1);
                UpdateDataGrid();
            }
        }
        private void sm_onmoveseek(object sender, EventArgs e)
        {
            var time = (float)WindowUtility.Seekmain.Currenttime;
            ChangeDataGridSelectedIndex(time);
        }
        private void ChangeDataGridSelectedIndex(float time)
        {
            int i = 0;
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
            if (i >= 0 && i < dataGridView1.RowCount)
            {
                dataGridView1.CurrentCell = dataGridView1[0, i];
                SetEvent(changeData.Values[i]);
                currentnum = i;
            }
            else
            {
                dataGridView1.CurrentCell = null;
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
        private void button1_Click(object sender, EventArgs e)
        {
            //add
            var time = (float)WindowUtility.Seekmain.Currenttime;
            if (!changeData.ContainsKey(time))
            {
                var eef = new EventEditForm();
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
                if (eef.ShowDialog() == DialogResult.OK)
                {
                    changeData.Add(time, eef.EventData);
                    this.dataGridView1.Rows.Add(1);
                    UpdateDataGrid();
                    ContentChanged();
                }
            }
            else
            {
                MessageBox.Show(alreadyadded);
            }
        }
        private void UpdateDataGrid()
        {
            stopList.Clear();
            float stopstarttime = -1;
            for (int i = 0; i < changeData.Count; i++)
            {
                this.dataGridView1[0, i].Value = changeData.Keys[i];
                this.dataGridView1[1, i].Value = changeData.Values[i].ToString();
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

        public void CheckInitialBpm(float bpm)
        {
            if (changeData.ContainsKey(0))
            {
                if (changeData[0].BPM != bpm)
                {
                    if (MessageBox.Show(Utility.Language["EMCheckBpmText"], Utility.Language["EMCheckBpm"], MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        changeData[0].BPM = bpm;
                        UpdateDataGrid();
                        ContentChanged();
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //edit
            if (dataGridView1.CurrentRow != null)
            {
                var eef = new EventEditForm();
                eef.SetLang();
                eef.EventData = changeData.Values[dataGridView1.CurrentRow.Index].Clone();
                if (eef.ShowDialog() == DialogResult.OK)
                {
                    float time = changeData.Keys[dataGridView1.CurrentRow.Index];
                    changeData.RemoveAt(dataGridView1.CurrentRow.Index);
                    dataGridView1.Rows.RemoveAt(this.dataGridView1.CurrentRow.Index);
                    changeData.Add(time, eef.EventData);
                    this.dataGridView1.Rows.Add(1);
                    UpdateDataGrid();
                    ContentChanged();
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //delete
            if (dataGridView1.CurrentRow != null)
            {
                if (MessageBox.Show(confirmdelete, confirm, MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    changeData.RemoveAt(dataGridView1.CurrentRow.Index);
                    dataGridView1.Rows.RemoveAt(this.dataGridView1.CurrentRow.Index);
                    UpdateDataGrid();
                    ContentChanged();
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (this.dataGridView1.CurrentRow == null)
            {
                MessageBox.Show(noselectedline);
            }
            else
            {
                float time = changeData.Keys[this.dataGridView1.CurrentRow.Index];
                if (!changeData.ContainsKey(time + (float)this.numericUpDown1.Value))
                {
                    EventData eve = changeData.Values[this.dataGridView1.CurrentRow.Index];
                    changeData.RemoveAt(this.dataGridView1.CurrentRow.Index);
                    changeData.Add(time + (float)this.numericUpDown1.Value, eve);
                    UpdateDataGrid();
                    ContentChanged();
                }
            }
        }
    }
}
