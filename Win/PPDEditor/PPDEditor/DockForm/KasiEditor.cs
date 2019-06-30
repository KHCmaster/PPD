using PPDEditor.Forms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;

namespace PPDEditor
{
    public partial class KasiEditor : ScrollableForm
    {
        SortedList<float, string> changedata = new SortedList<float, string>(100);
        string alreadyadd = "すでにこの時間にはテキストが指定されています";
        string noselectedline = "選択された行が存在しません";
        public KasiEditor()
        {
            InitializeComponent();
            foreach (DataGridViewColumn c in dataGridView1.Columns)
                c.SortMode = DataGridViewColumnSortMode.NotSortable;
        }
        public void SetLang()
        {
            this.Text = Utility.Language["KE"];
            this.dataGridView1.Columns[0].HeaderText = Utility.Language["KEGridColumn1"];
            this.dataGridView1.Columns[1].HeaderText = Utility.Language["KEGridColumn2"];
            this.label1.Text = Utility.Language["KELabel1"];
            this.label2.Text = Utility.Language["KELabel2"];
            this.button1.Text = Utility.Language["KEButton1"];
            this.button2.Text = Utility.Language["KEButton2"];
            this.button3.Text = Utility.Language["KEButton3"];
            this.button4.Text = Utility.Language["KEButton4"];
            this.button5.Text = Utility.Language["KEButton5"];
            this.alreadyadd = Utility.Language["KEAlreadyAdded"];
            this.noselectedline = Utility.Language["KENoSelectedLine"];
        }
        public void SetSkin()
        {
            this.label1.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.label2.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
        }
        public float[] GetKasiChangeWithinTime(float starttime, float endtime)
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
                ret = changedata[time];
                if (ret == "") ret = " ";
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
            if (previoustime < 0) return;
            if (changedata.ContainsKey(previoustime) && !changedata.ContainsKey(newtime))
            {
                string kasi = changedata[previoustime];
                changedata.Remove(previoustime);
                changedata.Add(newtime, kasi);
                updatedatagrid();
                ContentChanged();
                WindowUtility.Seekmain.DrawAndRefresh();
            }
        }
        public void setdata(string s)
        {
            s = removeextra(s);
            var datas = s.Split('\n');
            foreach (string eachdata in datas)
            {
                var temp = eachdata.Split(':');
                try
                {
                    if (temp.Length >= 2)
                    {
                        changedata.Add(float.Parse(temp[0], CultureInfo.InvariantCulture), temp[1]);
                        dataGridView1.RowCount++;
                    }
                    else changedata.Add(float.Parse(temp[0], CultureInfo.InvariantCulture), "");
                }
                catch
                {
                }
            }
            updatedatagrid();
        }
        public string askcurrentkasi()
        {
            var time = (float)WindowUtility.Seekmain.Currenttime;
            string ret = "";
            int i = 0;
            for (i = 0; i < changedata.Count; i++)
            {
                if (changedata.Keys[i] <= time) continue;
                break;
            }
            if (i == 0) i = 1;
            if (changedata.Count > 0)
            {
                ret = changedata.Values[i - 1];
            }
            return ret;
        }
        public string savedata()
        {
            string ret = "";
            for (int i = 0; i < changedata.Count; i++)
            {
                ret += changedata.Keys[i].ToString(CultureInfo.InvariantCulture) + ":" + changedata.Values[i];
                if (i != changedata.Count - 1)
                {
                    ret += System.Environment.NewLine;
                }
            }
            return ret;
        }
        public void Clear()
        {
            changedata.Clear();
            this.dataGridView1.RowCount = 0;
        }
        private string removeextra(string st)
        {
            var a = st.IndexOf("\t");
            while (a != -1)
            {
                st = st.Remove(a, 1);
                a = st.IndexOf("\t");
            }
            a = st.IndexOf("\r");
            while (a != -1)
            {
                st = st.Remove(a, 1);
                a = st.IndexOf("\r");
            }
            return st;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (!changedata.ContainsKey((float)WindowUtility.Seekmain.Currenttime))
            {
                var text = textBox1.Text.Replace("\n", "");
                changedata.Add((float)WindowUtility.Seekmain.Currenttime, text);
                this.dataGridView1.Rows.Add(1);
                updatedatagrid();
                ContentChanged();
            }
            else
            {
                MessageBox.Show(alreadyadd);
            }
        }
        private void updatedatagrid()
        {
            for (int i = 0; i < changedata.Count; i++)
            {
                this.dataGridView1[0, i].Value = changedata.Keys[i];
                this.dataGridView1[1, i].Value = changedata.Values[i];
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (this.dataGridView1.CurrentRow == null)
            {
                MessageBox.Show(noselectedline);
            }
            else
            {
                changedata.RemoveAt(this.dataGridView1.CurrentRow.Index);
                this.dataGridView1.Rows.RemoveAt(this.dataGridView1.CurrentRow.Index);
                ContentChanged();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (this.dataGridView1.CurrentRow == null)
            {
                MessageBox.Show(noselectedline);
            }
            else
            {
                float time = changedata.Keys[this.dataGridView1.CurrentRow.Index];
                if (!changedata.ContainsKey(time + (float)this.numericUpDown1.Value))
                {
                    string text = changedata.Values[this.dataGridView1.CurrentRow.Index];
                    changedata.RemoveAt(this.dataGridView1.CurrentRow.Index);
                    changedata.Add(time + (float)this.numericUpDown1.Value, text);
                    updatedatagrid();
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
                var fm4 = new ChangeKasiForm();
                fm4.SetLang();
                fm4.beforetext = changedata.Values[this.dataGridView1.CurrentRow.Index];
                if (fm4.ShowDialog() == DialogResult.OK)
                {
                    float time = changedata.Keys[this.dataGridView1.CurrentRow.Index];
                    var newtext = fm4.newtext.Replace("\n", "");
                    changedata.RemoveAt(this.dataGridView1.CurrentRow.Index);
                    changedata.Add(time, newtext);
                    updatedatagrid();
                    ContentChanged();
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (this.dataGridView1.CurrentRow == null)
            {
                MessageBox.Show(noselectedline);
            }
            else
            {
                string text = changedata.Values[dataGridView1.CurrentRow.Index];
                this.textBox1.Text = text;
                this.button1_Click(this, e);
                ContentChanged();
            }
        }
        private void dataGridView1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            ContentChanged();
        }

        private void dataGridView1_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            ContentChanged();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = "";
            this.textBox1.Focus();
        }
    }
}
