using PPDEditor.Forms;
using PPDFramework;
using PPDFramework.PPDStructure;
using PPDSound;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PPDEditor
{
    public partial class SoundManager : ScrollableForm
    {
        Sound sou;
        List<string> data = new List<string>(100);
        ComboBox[] cbs = new ComboBox[11];
        SortedList<float, UInt16[]> changedata = new SortedList<float, UInt16[]>();
        int currentnum;
        string nosound = "無音";
        string wavefilter = "サウンドファイル(*.wav;*ogg)|*.wav;*ogg|すべてのファイル(*.*)|*.*";
        string cannotdeletesound = "無音は削除できません";
        string cannotdeletefirst = "最初の設定は削除できません";
        string alreadyadded = "すでにこの時間に変更が設定されています。削除してからもう一度追加してください";
        public List<string> sounddatapaths
        {
            get
            {
                return data;
            }
        }
        public void SetEvent()
        {
            WindowUtility.Seekmain.Seeked += this.seekpointchange;
        }
        public void SetLang()
        {
            this.Text = Utility.Language["SM"];
            this.label1.Text = Utility.Language["SMLabel1"];
            this.label2.Text = Utility.Language["SMLabel2"];
            this.label3.Text = Utility.Language["SMLabel3"];
            this.label4.Text = Utility.Language["SMLabel4"];
            this.label5.Text = Utility.Language["SMLabel5"];
            this.label6.Text = Utility.Language["SMLabel6"];
            this.label7.Text = Utility.Language["SMLabel7"];
            this.label8.Text = Utility.Language["SMLabel8"];
            this.label9.Text = Utility.Language["SMLabel9"];
            this.label10.Text = Utility.Language["SMLabel10"];
            this.button1.Text = Utility.Language["SMButton1"];
            this.button2.Text = Utility.Language["SMButton2"];
            this.button3.Text = Utility.Language["SMButton3"];
            this.button4.Text = Utility.Language["SMButton4"];
            this.button5.Text = Utility.Language["SMButton5"];
            this.button6.Text = Utility.Language["SMButton6"];
            this.button7.Text = Utility.Language["EditSelectedRow"];
            this.checkBox1.Text = Utility.Language["SMCheckBox"];
            this.dataGridView1.Columns[0].HeaderText = Utility.Language["SMDataGridColumn1"];
            this.dataGridView1.Columns[1].HeaderText = Utility.Language["SMDataGridColumn2"];
            nosound = Utility.Language["SMNoSound"];
            wavefilter = Utility.Language["SMWaveFilter"];
            cannotdeletesound = Utility.Language["SMDeleteNosound"];
            cannotdeletefirst = Utility.Language["SMDeleteFirst"];
            alreadyadded = Utility.Language["SMAlreadyAdded"];
            data.RemoveAt(0);
            data.Insert(0, nosound);
            for (int i = 0; i < cbs.Length; i++)
            {
                var index = cbs[i].SelectedIndex;
                cbs[i].Items.Clear();
                cbs[i].Items.AddRange(data.Select(s => Path.GetFileNameWithoutExtension(s)).ToArray());
                cbs[i].SelectedIndex = index;
            }
            for (int i = 0; i < cbs.Length; i++)
            {
                cbs[i].Items.Add(data[0]);
                cbs[i].SelectedIndex = 0;
            }
            updatedatagrid();
        }
        public void SetSkin()
        {
            this.label1.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.label2.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.label3.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.label4.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.label5.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.label6.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.label7.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.label8.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.label9.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.label10.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.checkBox1.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
        }
        public SoundManager()
        {
            InitializeComponent();
            data.Add(nosound);
            cbs[0] = this.comboBox1;
            cbs[1] = this.comboBox2;
            cbs[2] = this.comboBox3;
            cbs[3] = this.comboBox4;
            cbs[4] = this.comboBox5;
            cbs[5] = this.comboBox6;
            cbs[6] = this.comboBox7;
            cbs[7] = this.comboBox8;
            cbs[8] = this.comboBox9;
            cbs[9] = this.comboBox10;
            cbs[10] = this.comboBox11;
            for (int i = 0; i < cbs.Length; i++)
            {
                cbs[i].Items.Add(data[0]);
                cbs[i].SelectedIndex = 0;
            }
            addchange(0);
            foreach (DataGridViewColumn c in dataGridView1.Columns) c.SortMode = DataGridViewColumnSortMode.NotSortable;
            ContentSaved();
        }
        public bool fixedcombo
        {
            get
            {
                return this.checkBox1.Checked;
            }
            set
            {
                this.checkBox1.Checked = value;
            }
        }
        public void Clear()
        {
            sou.DeleteAllSound();
            for (int i = 0; i < 11; i++)
            {
                while (cbs[i].Items.Count != 1)
                {
                    cbs[i].Items.RemoveAt(cbs[i].Items.Count - 1);
                }
                cbs[i].SelectedIndex = 0;
            }
            data = new List<string>(100)
            {
                nosound
            };
            ClearSetting();
        }
        public void ClearSetting()
        {
            while (changedata.Count > 1)
            {
                var removeIndex = changedata.Count - 1;
                changedata.RemoveAt(removeIndex);
                dataGridView1.Rows.RemoveAt(removeIndex);
            }
            ContentSaved();
        }
        public void setSound(Sound sou)
        {
            this.sou = sou;
        }
        public string savesoundset()
        {
            string ret = "";
            for (int i = 1; i < data.Count; i++)
            {
                ret += Path.GetFileName(data[i]);
                if (i < data.Count - 1) ret += System.Environment.NewLine;
            }
            return ret;
        }
        public SCDData[] GetSCDData()
        {
            var ret = new List<SCDData>();
            UInt16[] lastdata = changedata.Values[0];
            for (int i = 1; i < changedata.Count; i++)
            {
                UInt16[] nowdata = changedata.Values[i];
                float time = changedata.Keys[i];
                for (int j = 0; j < nowdata.Length; j++)
                {
                    if (lastdata[j] != nowdata[j])
                    {
                        ret.Add(new SCDData(time, (ButtonType)j, nowdata[j]));
                    }
                }
                lastdata = nowdata;
            }
            return ret.ToArray();
        }
        public float[] GetSoundChangeWithinTime(float starttime, float endtime)
        {
            var ret = new List<float>();
            foreach (float time in changedata.Keys)
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
            if (changedata.ContainsKey(time))
            {
                ushort[] indexes = changedata[time];
                var sb = new StringBuilder();
                for (int i = 0; i < indexes.Length; i++)
                {
                    sb.Append(comboBox11.Items[indexes[i]].ToString());
                    sb.Append("\n");
                }
                ret = sb.ToString();
            }
            else
            {
                ret = "";
            }
            return ret;
        }
        public void FocusWithTimeData(float time)
        {
            if (changedata.ContainsKey(time))
            {
                var index = changedata.IndexOfKey(time);
                if (index >= 0)
                {
                    dataGridView1.Rows[index].Selected = true;
                    dataGridView1.FirstDisplayedScrollingRowIndex = index;
                    ShowOrHideWindow(this);
                }
            }
        }
        public void ChangeTime(float previoustime, float newtime)
        {
            if (previoustime <= 0) return;
            if (changedata.ContainsKey(previoustime) && !changedata.ContainsKey(newtime))
            {
                ushort[] change = changedata[previoustime];
                changedata.Remove(previoustime);
                changedata.Add(newtime, change);
                updatedatagrid();
                ContentChanged();
                WindowUtility.Seekmain.DrawAndRefresh();
            }
        }
        public void playsound(bool[] b)
        {
            for (int i = 0; i < b.Length; i++)
            {
                if (b[i])
                {
                    sou.Play(data[changedata.Values[currentnum][i]], WindowUtility.EventManager.GetVolume(i));
                }
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            this.openFileDialog1.Filter = wavefilter;
            this.openFileDialog1.RestoreDirectory = true;
            this.openFileDialog1.FileName = "";
            this.openFileDialog1.Multiselect = true;
            if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                readsounds(this.openFileDialog1.FileNames);
            }
        }
        public void readsounds(string[] filenames)
        {
            for (int i = 0; i < filenames.Length; i++)
            {
                var s = Path.GetFileNameWithoutExtension(filenames[i]);
                for (int j = 0; j < cbs.Length; j++)
                {
                    cbs[j].Items.Add(s);
                }
                if (!data.Contains(filenames[i]))
                {
                    sou.AddSound(filenames[i]);
                }
                data.Add(filenames[i]);
                ContentChanged();
            }
        }
        public void readchangedata(float[] times, ushort[][] changes)
        {
            for (int i = 0; i < times.Length; i++)
            {
                if (changedata.ContainsKey(times[i]))
                {
                    //MessageBox.Show("すでにこの時間に変更が設定されています。削除してからもう一度追加してください");
                }
                else
                {
                    this.dataGridView1.Rows.Add(1);
                    changedata.Add(times[i], changes[i]);
                }
            }
            updatedatagrid();
        }
        private void button5_Click(object sender, EventArgs e)
        {
            if (this.comboBox11.SelectedIndex > 0)
            {
                sou.Play(data[this.comboBox11.SelectedIndex], -1000);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (this.comboBox11.SelectedIndex < 0)
            {
                return;
            }
            if (this.comboBox11.SelectedIndex == 0)
            {
                MessageBox.Show(cannotdeletesound);
                return;
            }
            bool changeOtherDifficultySoundSetting = false;
            if (WindowUtility.MainForm.IsProjectLoaded && WindowUtility.MainForm.AvailableDifficultyCount > 1)
            {
                if (MessageBox.Show(Utility.Language["DeleteSoundConfirmContent"], Utility.Language["DeleteSoundConfirm"], MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.Cancel)
                {
                    return;
                }
                changeOtherDifficultySoundSetting = true;
            }

            int removedIndex = this.comboBox11.SelectedIndex;
            for (int i = 0; i < cbs.Length; i++)
            {
                cbs[i].Items.RemoveAt(removedIndex);
                if (cbs[i].SelectedIndex < 0)
                {
                    cbs[i].SelectedIndex = 0;
                }
            }
            string s = data[removedIndex];
            data.RemoveAt(removedIndex);
            if (!data.Contains(s))
            {
                sou.DeleteSound(s);
            }
            for (int i = 0; i < changedata.Count; i++)
            {
                UInt16[] temp = changedata.Values[i];
                for (int j = 0; j < temp.Length; j++)
                {
                    if (temp[j] == removedIndex)
                    {
                        temp[j] = 0;
                    }
                    if (temp[j] > removedIndex)
                    {
                        temp[j]--;
                    }
                }
                float time = changedata.Keys[i];
                changedata.RemoveAt(i);
                changedata.Add(time, temp);
            }
            if (changeOtherDifficultySoundSetting)
            {
                foreach (var diff in ((AvailableDifficulty[])Enum.GetValues(typeof(AvailableDifficulty))))
                {
                    if (diff == AvailableDifficulty.None || !WindowUtility.MainForm.AvailableDifficulty.HasFlag(diff) || WindowUtility.MainForm.CurrentDifficulty == diff)
                    {
                        continue;
                    }
                    ChangeDifficultySound(diff, removedIndex);
                }
            }
            updatedatagrid();
            ContentChanged();
        }

        private void ChangeDifficultySound(AvailableDifficulty difficulty, int removedIndex)
        {
            var path = Path.Combine(WindowUtility.MainForm.CurrentProjectDir, String.Format("{0}.scd", difficulty));
            if (!File.Exists(path))
            {
                return;
            }
            var data = SCDReader.Read(path);
            SCDWriter.Write(path, data.Select(d =>
            {
                var newIndex = removedIndex == d.SoundIndex ? 0 : (d.SoundIndex > removedIndex ? d.SoundIndex - 1 : d.SoundIndex);
                return new SCDData(d.Time, d.ButtonType, (ushort)newIndex);
            }).ToArray());
        }

        private UInt16[] cbselectednums()
        {
            UInt16[] ret = new UInt16[10];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = (UInt16)cbs[i].SelectedIndex;
            }
            return ret;
        }
        private string cbvalue(int num)
        {
            string ret = "";
            UInt16[] temp = changedata.Values[num];
            for (int i = 0; i < 10; i++)
            {
                if (temp[i] != 0)
                {
                    ret += Path.GetFileNameWithoutExtension(data[temp[i]]);
                }
                else
                {
                    ret += data[temp[i]];
                }
                if (i < 9)
                {
                    ret += ",";
                }
            }
            return ret;
        }
        private void addchange(float time)
        {
            if (changedata.ContainsKey(time))
            {
                MessageBox.Show(alreadyadded);
            }
            else
            {
                this.dataGridView1.Rows.Add(1);
                changedata.Add(time, cbselectednums());
                updatedatagrid();
            }
        }
        private void updatedatagrid()
        {
            for (int i = 0; i < changedata.Count; i++)
            {
                this.dataGridView1[0, i].Value = changedata.Keys[i];
                this.dataGridView1[1, i].Value = cbvalue(i);
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            addchange((float)WindowUtility.Seekmain.Currenttime);
        }
        private void seekpointchange(object sender, EventArgs e)
        {
            var time = (float)WindowUtility.Seekmain.Currenttime;
            changedatagridselectindex(time);
            if (this.checkBox1.Checked)
            {
                synccombobox();
            }
        }
        private void changedatagridselectindex(float time)
        {
            int i = 0;
            for (i = 0; i < changedata.Count; i++)
            {
                if (changedata.Keys[i] <= time) continue;
                if (changedata.Keys[i] >= time)
                {
                    i--;
                    break;
                }
            }
            if (i == changedata.Count)
            {
                i--;
            }
            if (i < 0)
            {
                i = 0;
            }
            dataGridView1.CurrentCell = dataGridView1[0, i];
            currentnum = i;
        }
        private void synccombobox()
        {
            if (dataGridView1.CurrentRow != null)
            {
                int num = dataGridView1.CurrentRow.Index;
                UInt16[] temp = changedata.Values[num];
                for (int i = 0; i < temp.Length; i++)
                {
                    cbs[i].SelectedIndex = temp[i];
                }
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow != null)
            {
                if (dataGridView1.CurrentRow.Index == 0)
                {
                    MessageBox.Show(cannotdeletefirst);
                }
                else
                {
                    changedata.RemoveAt(dataGridView1.CurrentRow.Index);
                    dataGridView1.Rows.RemoveAt(this.dataGridView1.CurrentRow.Index);
                    updatedatagrid();
                    ContentChanged();
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow != null && dataGridView1.CurrentRow.Index > 0)
            {
                var time = (float)dataGridView1[0, dataGridView1.CurrentRow.Index].Value;
                var change = changedata[time];
                var form = new SoundChangeEditForm();
                form.SetLang();
                form.SetInfo(change, data.ToArray());
                form.PlaySound += form_PlaySound;
                if (form.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                var index = dataGridView1.CurrentRow.Index;
                changedata[time] = form.Change;
                updatedatagrid();
                ContentChanged();
            }
        }

        void form_PlaySound(string obj)
        {
            sou.Play(obj, -1000);
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (this.checkBox1.Checked)
            {
                synccombobox();
            }
        }

        private void dataGridView1_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            ContentChanged();
        }

        private void dataGridView1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            ContentChanged();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (this.comboBox11.SelectedIndex == 0)
            {
                MessageBox.Show(cannotdeletesound);
            }
            else if (this.comboBox11.SelectedIndex > 0)
            {
                this.openFileDialog1.Filter = wavefilter;
                this.openFileDialog1.RestoreDirectory = true;
                this.openFileDialog1.FileName = "";
                this.openFileDialog1.Multiselect = false;
                if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    ChangeSound(this.openFileDialog1.FileName);
                }
            }
        }
        private void ChangeSound(string filename)
        {
            int num = this.comboBox11.SelectedIndex;
            if (data[num] == filename)
            {
                sou.DeleteSound(filename);
                data[num] = "ASOJKDIOAFOUIABH";
            }
            var s = Path.GetFileNameWithoutExtension(filename);
            for (int j = 0; j < cbs.Length; j++)
            {
                cbs[j].Items[num] = s;
            }
            if (!data.Contains(filename))
            {
                sou.AddSound(filename);
            }
            data[num] = filename;
            ContentChanged();

        }
    }
}
