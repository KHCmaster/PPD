using PPDEditor.Forms;
using PPDFramework;
using PPDFramework.PPDStructure;
using PPDSound;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace PPDEditor
{
    public partial class SoundManager : DockView
    {
        Sound sou;
        List<string> data = new(100);
        readonly ComboBox[] cbs = new ComboBox[11];
        readonly SortedList<float, UInt16[]> changedata = [];
        int currentnum;
        string nosound = "無音";
        string wavefilter = "サウンドファイル(*.wav;*ogg)|*.wav;*ogg|すべてのファイル(*.*)|*.*";
        string cannotdeletesound = "無音は削除できません";
        string cannotdeletefirst = "最初の設定は削除できません";
        string alreadyadded = "すでにこの時間に変更が設定されています。削除してからもう一度追加してください";
        public List<string> Sounddatapaths
        {
            get
            {
                return data;
            }
        }
        public void SetEvent()
        {
            WindowUtility.Seekmain.Seeked += this.Seekpointchange;
        }
        public void SetLang()
        {
            this.TabText = Utility.Language["SM"];
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
            this.button1.Content = Utility.Language["SMButton1"];
            this.button2.Content = Utility.Language["SMButton2"];
            this.button3.Content = Utility.Language["SMButton3"];
            this.button4.Content = Utility.Language["SMButton4"];
            this.button5.Content = Utility.Language["SMButton5"];
            this.button6.Content = Utility.Language["SMButton6"];
            this.button7.Content = Utility.Language["EditSelectedRow"];
            this.checkBox1Text.Text = Utility.Language["SMCheckBox"];
            this.Column1.Header = Utility.Language["SMDataGridColumn1"];
            this.Column2.Header = Utility.Language["SMDataGridColumn2"];
            nosound = Utility.Language["SMNoSound"];
            wavefilter = Utility.Language["SMWaveFilter"];
            cannotdeletesound = Utility.Language["SMDeleteNosound"];
            cannotdeletefirst = Utility.Language["SMDeleteFirst"];
            alreadyadded = Utility.Language["SMAlreadyAdded"];
            data.RemoveAt(0);
            data.Insert(0, nosound);
            // 以前はここで無音を各コンボボックスの最後にもう一つ加えていて、項目の番号がずれていたので、加えないようにした
            for (int i = 0; i < cbs.Length; i++)
            {
                cbs[i].Items.Clear();
                cbs[i].Items.Add(new SoundItem(data[0]));
                foreach (var s in data.Skip(1))
                {
                    cbs[i].Items.Add(new SoundItem(Path.GetFileNameWithoutExtension(s)));
                }
                cbs[i].SelectedIndex = 0;
            }
            Updatedatagrid();
        }
        public void SetSkin()
        {
            var brush = WpfSkin.TransparentTextBrush;
            foreach (var label in new TextBlock[] { label1, label2, label3, label4, label5, label6, label7, label8, label9, label10, checkBox1Text })
            {
                label.Foreground = brush;
            }
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
                cbs[i].Items.Add(new SoundItem(data[0]));
                cbs[i].SelectedIndex = 0;
            }
            Addchange(0);
            ContentSaved();
        }
        public bool Fixedcombo
        {
            get
            {
                return this.checkBox1.IsChecked == true;
            }
            set
            {
                this.checkBox1.IsChecked = value;
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
                changedata.RemoveAt(changedata.Count - 1);
            }
            Updatedatagrid();
            ContentSaved();
        }
        public void SetSound(Sound sou)
        {
            this.sou = sou;
        }
        public string Savesoundset()
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
            return [.. ret];
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
            return [.. ret];
        }
        public string GetFormattedContent(float time)
        {
            string ret;
            if (changedata.TryGetValue(time, out ushort[] indexes))
            {
                var sb = new StringBuilder();
                for (int i = 0; i < indexes.Length; i++)
                {
                    sb.Append(comboBox11.Items[indexes[i]].ToString());
                    sb.Append('\n');
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
                    dataGridView1.SelectedIndex = index;
                    dataGridView1.ScrollIntoView(dataGridView1.SelectedItem);
                    ShowDock();
                }
            }
        }
        public void ChangeTime(float previoustime, float newtime)
        {
            if (previoustime <= 0) return;
            if (changedata.TryGetValue(previoustime, out ushort[] change) && !changedata.ContainsKey(newtime))
            {
                changedata.Remove(previoustime);
                changedata.Add(newtime, change);
                Updatedatagrid();
                ContentChanged();
                WindowUtility.Seekmain.DrawAndRefresh();
            }
        }
        public void Playsound(bool[] b)
        {
            for (int i = 0; i < b.Length; i++)
            {
                if (b[i])
                {
                    sou.Play(data[changedata.Values[currentnum][i]], WindowUtility.EventManager.GetVolume(i));
                }
            }
        }
        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog1 = new OpenFileDialog
            {
                Filter = wavefilter,
                Multiselect = true
            };
            if (openFileDialog1.ShowDialog(Window.GetWindow(this)) == true)
            {
                Readsounds(openFileDialog1.FileNames);
            }
        }
        public void Readsounds(string[] filenames)
        {
            for (int i = 0; i < filenames.Length; i++)
            {
                var s = Path.GetFileNameWithoutExtension(filenames[i]);
                for (int j = 0; j < cbs.Length; j++)
                {
                    cbs[j].Items.Add(new SoundItem(s));
                }
                if (!data.Contains(filenames[i]))
                {
                    sou.AddSound(filenames[i]);
                }
                data.Add(filenames[i]);
                ContentChanged();
            }
        }
        public void Readchangedata(float[] times, ushort[][] changes)
        {
            for (int i = 0; i < times.Length; i++)
            {
                if (changedata.ContainsKey(times[i]))
                {
                    //MessageBox.Show("すでにこの時間に変更が設定されています。削除してからもう一度追加してください");
                }
                else
                {
                    changedata.Add(times[i], changes[i]);
                }
            }
            Updatedatagrid();
        }
        private void Button5_Click(object sender, RoutedEventArgs e)
        {
            if (this.comboBox11.SelectedIndex > 0)
            {
                sou.Play(data[this.comboBox11.SelectedIndex], -1000);
            }
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            if (this.comboBox11.SelectedIndex < 0)
            {
                return;
            }
            if (this.comboBox11.SelectedIndex == 0)
            {
                MessageBox.Show(Window.GetWindow(this), cannotdeletesound);
                return;
            }
            bool changeOtherDifficultySoundSetting = false;
            if (WindowUtility.MainForm.IsProjectLoaded && WindowUtility.MainForm.AvailableDifficultyCount > 1)
            {
                if (MessageBox.Show(Window.GetWindow(this), Utility.Language["DeleteSoundConfirmContent"], Utility.Language["DeleteSoundConfirm"], MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
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
                foreach (var diff in Enum.GetValues<AvailableDifficulty>())
                {
                    if (diff == AvailableDifficulty.None || !WindowUtility.MainForm.AvailableDifficulty.HasFlag(diff) || WindowUtility.MainForm.CurrentDifficulty == diff)
                    {
                        continue;
                    }
                    ChangeDifficultySound(diff, removedIndex);
                }
            }
            Updatedatagrid();
            ContentChanged();
        }

        private static void ChangeDifficultySound(AvailableDifficulty difficulty, int removedIndex)
        {
            var path = Path.Combine(WindowUtility.MainForm.CurrentProjectDir, String.Format("{0}.scd", difficulty));
            if (!File.Exists(path))
            {
                return;
            }
            var data = SCDReader.Read(path);
            SCDWriter.Write(path, [.. data.Select(d =>
            {
                var newIndex = removedIndex == d.SoundIndex ? 0 : (d.SoundIndex > removedIndex ? d.SoundIndex - 1 : d.SoundIndex);
                return new SCDData(d.Time, d.ButtonType, (ushort)newIndex);
            })]);
        }

        private UInt16[] Cbselectednums()
        {
            UInt16[] ret = new UInt16[10];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = (UInt16)cbs[i].SelectedIndex;
            }
            return ret;
        }
        private string Cbvalue(int num)
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
        private void Addchange(float time)
        {
            if (changedata.ContainsKey(time))
            {
                MessageBox.Show(Window.GetWindow(this), alreadyadded);
            }
            else
            {
                changedata.Add(time, Cbselectednums());
                Updatedatagrid();
                ContentChanged();
            }
        }
        private void Updatedatagrid()
        {
            // 選んでいる行は、行の番号で保つ（以前の DataGridView と同じ）
            var selectedIndex = dataGridView1.SelectedIndex;
            var rows = new List<ChangeRow>();
            for (int i = 0; i < changedata.Count; i++)
            {
                rows.Add(new ChangeRow(changedata.Keys[i], Cbvalue(i)));
            }
            dataGridView1.ItemsSource = rows;
            if (selectedIndex >= 0 && selectedIndex < rows.Count)
            {
                dataGridView1.SelectedIndex = selectedIndex;
            }
        }

        /// <summary>
        /// 選んでいる行の番号です。選んでいないときは -1 です。
        /// </summary>
        private int CurrentIndex
        {
            get { return dataGridView1.SelectedIndex; }
        }

        /// <summary>
        /// 変更の一覧の 1 行です。
        /// </summary>
        record ChangeRow(float Time, string Sounds);

        /// <summary>
        /// コンボボックスの項目です。同じ名前の音があっても選ぶ項目がずれないように、文字列ではなくこれを入れる。
        /// </summary>
        sealed class SoundItem(string name)
        {
            public string Name { get; } = name;

            public override string ToString()
            {
                return Name;
            }
        }
        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            Addchange((float)WindowUtility.Seekmain.Currenttime);
        }
        private void Seekpointchange(object sender, EventArgs e)
        {
            var time = (float)WindowUtility.Seekmain.Currenttime;
            Changedatagridselectindex(time);
            if (this.checkBox1.IsChecked == true)
            {
                Synccombobox();
            }
        }
        private void Changedatagridselectindex(float time)
        {
            int i;
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
            if (i < changedata.Count)
            {
                dataGridView1.SelectedIndex = i;
            }
            currentnum = i;
        }
        private void Synccombobox()
        {
            if (CurrentIndex >= 0)
            {
                int num = CurrentIndex;
                UInt16[] temp = changedata.Values[num];
                for (int i = 0; i < temp.Length; i++)
                {
                    cbs[i].SelectedIndex = temp[i];
                }
            }
        }
        private void Button4_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentIndex >= 0)
            {
                if (CurrentIndex == 0)
                {
                    MessageBox.Show(Window.GetWindow(this), cannotdeletefirst);
                }
                else
                {
                    changedata.RemoveAt(CurrentIndex);
                    Updatedatagrid();
                    ContentChanged();
                }
            }
        }

        private void Button7_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentIndex > 0)
            {
                var time = changedata.Keys[CurrentIndex];
                var change = changedata[time];
                var form = new SoundChangeEditWindow();
                form.SetLang();
                form.SetInfo(change, [.. data]);
                form.PlaySound += Form_PlaySound;
                if (!WpfDialog.ShowDialog(form))
                {
                    return;
                }

                changedata[time] = form.Change;
                Updatedatagrid();
                ContentChanged();
            }
        }

        void Form_PlaySound(string obj)
        {
            sou.Play(obj, -1000);
        }

        private void DataGridView1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.checkBox1.IsChecked == true)
            {
                Synccombobox();
            }
        }

        private void Button6_Click(object sender, RoutedEventArgs e)
        {
            if (this.comboBox11.SelectedIndex == 0)
            {
                MessageBox.Show(Window.GetWindow(this), cannotdeletesound);
            }
            else if (this.comboBox11.SelectedIndex > 0)
            {
                var openFileDialog1 = new OpenFileDialog
                {
                    Filter = wavefilter
                };
                if (openFileDialog1.ShowDialog(Window.GetWindow(this)) == true)
                {
                    ChangeSound(openFileDialog1.FileName);
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
                // 選んでいる項目を入れ替えると選択が外れるので、選び直す
                var selected = cbs[j].SelectedIndex;
                cbs[j].Items[num] = new SoundItem(s);
                cbs[j].SelectedIndex = selected;
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
