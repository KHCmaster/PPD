using System;
using DigitalRune.Windows.Docking;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace testgame
{
    public partial class soundmanager : ScrollableForm
    {
        Sound sou;
        Seekmain sm;
        ArrayList data = new ArrayList(100);
        ComboBox[] cbs = new ComboBox[11];
        SortedList<float, UInt16[]> changedata = new SortedList<float, UInt16[]>();
        int currentnum = 0;
        string nosound = "無音";
        string wavefilter = "Waveファイル(*.wav)|*.wav|すべてのファイル(*.*)|*.*";
        string cannotdeletesound = "無音は削除できません";
        string cannotdeletefirst = "最初の設定は削除できません";
        string alreadyadded = "すでにこの時間に変更が設定されています。削除してからもう一度追加してください";
        public ArrayList sounddatapaths
        {
            get
            {
                return data;
            }
        }
        public void setlang(settinganalyzer sanal)
        {
            this.Text = sanal.getData("SM");
            this.label1.Text = sanal.getData("SMLabel1");
            this.label2.Text = sanal.getData("SMLabel2");
            this.label3.Text = sanal.getData("SMLabel3");
            this.label4.Text = sanal.getData("SMLabel4");
            this.label5.Text = sanal.getData("SMLabel5");
            this.label6.Text = sanal.getData("SMLabel6");
            this.label7.Text = sanal.getData("SMLabel7");
            this.label8.Text = sanal.getData("SMLabel8");
            this.label9.Text = sanal.getData("SMLabel9");
            this.label10.Text = sanal.getData("SMLabel10");
            this.button1.Text = sanal.getData("SMButton1");
            this.button2.Text = sanal.getData("SMButton2");
            this.button3.Text = sanal.getData("SMButton3");
            this.button4.Text = sanal.getData("SMButton4");
            this.button5.Text = sanal.getData("SMButton5");
            this.checkBox1.Text = sanal.getData("SMCheckBox");
            this.dataGridView1.Columns[0].HeaderText = sanal.getData("SMDataGridColumn1");
            this.dataGridView1.Columns[1].HeaderText = sanal.getData("SMDataGridColumn2");
            nosound = sanal.getData("SMNoSound");
            wavefilter = sanal.getData("SMWaveFilter");
            cannotdeletesound = sanal.getData("SMDeleteNosound");
            cannotdeletefirst = sanal.getData("SMDeleteFirst");
            alreadyadded = sanal.getData("SMAlreadyAdded");
            data.Clear(); 
            for (int i = 0; i < cbs.Length; i++)
            {
                cbs[i].Items.Clear();
            }
            data.Add(nosound);
            changedata.Clear();
            this.dataGridView1.RowCount = 0;
            setcomboboxdata();
            addchange(0);
            ContentSaved();
        }
        public soundmanager()
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
            setcomboboxdata();
            addchange(0);
            foreach (DataGridViewColumn c in dataGridView1.Columns) c.SortMode = DataGridViewColumnSortMode.NotSortable;
            InitializeScroll();
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
            sou.deleteallsound();
            while (changedata.Count != 1)
            {
                changedata.RemoveAt(changedata.Count-1);
                dataGridView1.Rows.RemoveAt(changedata.Count - 1);
            }
            for (int i = 0; i < 11; i++)
            {
                while (cbs[i].Items.Count != 1)
                {
                    cbs[i].Items.RemoveAt(cbs[i].Items.Count - 1);
                }
                cbs[i].SelectedIndex = 0;
            }
            data = new ArrayList(100);
            data.Add(nosound);
        }
        public void setseekmain(Seekmain sm)
        {
            this.sm = sm;
            sm.onmoveseek += new EventHandler(this.seekpointchange);
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
                ret += Path.GetFileName(data[i] as string);
                if (i < data.Count - 1) ret += System.Environment.NewLine;
            }
            return ret;
        }
        public void savescddata(ref FileStream fs)
        {
            UInt16[] lastdata = changedata.Values[0];
            for (int i = 1; i < changedata.Count; i++)
            {
                UInt16[] nowdata = changedata.Values[i];
                float time = changedata.Keys[i];
                for (int j = 0; j < nowdata.Length; j++)
                {
                    if (lastdata[j] != nowdata[j])
                    {
                        byte[] tbyte = System.BitConverter.GetBytes(time);
                        fs.Write(tbyte, 0, tbyte.Length);
                        fs.WriteByte((byte)j);
                        byte[] cbyte = System.BitConverter.GetBytes(nowdata[j]);
                        fs.Write(cbyte, 0, cbyte.Length);
                    }
                }
                lastdata = nowdata;
            }
        }
        public void playsound(bool[] b)
        {
            for (int i = 0; i < b.Length; i++)
            {
                if (b[i])
                {
                    sou.Play(data[changedata.Values[currentnum][i]] as string);
                }
            }
        }
        private void setcomboboxdata()
        {
            for (int i = 0; i < cbs.Length; i++)
            {
                cbs[i].Items.Add(data[0]);
                cbs[i].SelectedIndex = 0;
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
                string s = Path.GetFileNameWithoutExtension(filenames[i]);
                for (int j = 0; j < cbs.Length; j++)
                {
                    cbs[j].Items.Add(s);
                }
                if (!data.Contains(filenames[i]))
                {
                    sou.addSound(filenames[i]);
                }
                data.Add(filenames[i]);
                ContentChanged();
            }
        }
        public void readchangedata(float[] times,ushort[][] changes)
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
                sou.Play(data[this.comboBox11.SelectedIndex] as string);
            }       
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (this.comboBox11.SelectedIndex == 0)
            {
                MessageBox.Show(cannotdeletesound);
            }
            else if (this.comboBox11.SelectedIndex > 0)
            {
                int num = this.comboBox11.SelectedIndex;
                for (int i = 0; i < cbs.Length; i++)
                {
                    cbs[i].Items.RemoveAt(num);
                    if (cbs[i].SelectedIndex < 0)
                    {
                        cbs[i].SelectedIndex = 0;
                    }
                }
                string s = data[num] as string;
                data.RemoveAt(num);
                if (!data.Contains(s))
                {
                    sou.deletesound(s);
                }
                for (int i = 0; i < changedata.Count; i++)
                {
                    UInt16[] temp = changedata.Values[i];
                    for (int j = 0; j < temp.Length; j++)
                    {
                        if (temp[j] == num)
                        {
                            temp[j] = 0;
                        }
                        if (temp[j] > num)
                        {
                            temp[j]--;
                        }
                    }
                    float time = changedata.Keys[i];
                    changedata.RemoveAt(i);
                    changedata.Add(time, temp);
                }
                updatedatagrid();
                ContentChanged();
            }
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
                if(temp[i] != 0){
                    ret += Path.GetFileNameWithoutExtension(data[temp[i]] as string);
                }else{
                    ret += data[temp[i]] as string;
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
            addchange((float)sm.Currenttime);
        }
        private void seekpointchange(object sender, EventArgs e)
        {
            float time = (float)sm.Currenttime;
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
                }
            }
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
    }
}
