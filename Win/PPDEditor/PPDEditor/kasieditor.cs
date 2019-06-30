using System;
using DigitalRune.Windows.Docking;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace testgame
{
    public partial class kasieditor : ScrollableForm
    {
        SortedList<float, string> data = new SortedList<float, string>(100);
        Seekmain sm;
        settinganalyzer sanal;
        string alreadyadd = "すでにこの時間にはテキストが指定されています";
        string noselectedline = "選択された行が存在しません";
        public kasieditor()
        {
            InitializeComponent();
            foreach (DataGridViewColumn c in dataGridView1.Columns)
                c.SortMode = DataGridViewColumnSortMode.NotSortable;
            InitializeScroll();
        }
        public void setlang(settinganalyzer sanal)
        {
            this.sanal = sanal;
            this.Text = sanal.getData("KE");
            this.dataGridView1.Columns[0].HeaderText = sanal.getData("KEGridColumn1");
            this.dataGridView1.Columns[1].HeaderText = sanal.getData("KEGridColumn2");
            this.label1.Text = sanal.getData("KELabel1");
            this.label2.Text = sanal.getData("KELabel2");
            this.button1.Text = sanal.getData("KEButton1");
            this.button2.Text = sanal.getData("KEButton2");
            this.button3.Text = sanal.getData("KEButton3");
            this.button4.Text = sanal.getData("KEButton4");
            this.alreadyadd = sanal.getData("KEAlreadyAdded");
            this.noselectedline = sanal.getData("KENoSelectedLine");
        }
        public void setseekmain(Seekmain sm)
        {
            this.sm = sm;
        }
        public void setdata(string s)
        {
            s = removeextra(s);
            int anchor = 0;
            float time = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == ':')
                {
                    if (float.TryParse(s.Substring(anchor, i - anchor), out time))
                    {

                    }
                    anchor = i + 1;
                }
                if (s[i] == '\n')
                {
                    if (!data.ContainsKey(time))
                    {
                        data.Add(time, s.Substring(anchor, i - anchor));
                        this.dataGridView1.Rows.Add(1);
                    }
                    anchor = i + 1;
                }
                if (i == s.Length - 1)
                {
                    if (!data.ContainsKey(time))
                    {
                        data.Add(time, s.Substring(anchor));
                        this.dataGridView1.Rows.Add(1);
                    }
                }
            }
            updatedatagrid();
        }
        public string askcurrentkasi()
        {
            float time = (float)sm.Currenttime;
            string ret = "";
            int i = 0;
            for (i = 0; i < data.Count; i++)
            {
                if (data.Keys[i] <= time) continue;
                break;
            }
            if (i == 0) i = 1;
            if (data.Count > 0)
            {
                ret = data.Values[i - 1];
            }
            return ret;
        }
        public string savedata()
        {
            string ret = "";
            for (int i = 0; i < data.Count; i++)
            {
                ret += data.Keys[i] + ":" + data.Values[i];
                if (i != data.Count - 1)
                {
                    ret += System.Environment.NewLine;
                }
            }
            return ret;
        }
        public void clear()
        {
            data = new SortedList<float, string>(100);
            this.dataGridView1.RowCount = 0;
        }
        private string removeextra(string st)
        {
            int a = st.IndexOf("\t");
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
            if (!data.ContainsKey((float)sm.Currenttime))
            {
                string text = textBox1.Text.Replace("\n", "");
                data.Add((float)sm.Currenttime, text);
                this.dataGridView1.Rows.Add(1);
                updatedatagrid();
            }
            else
            {
                MessageBox.Show(alreadyadd);
            }
        }
        private void updatedatagrid()
        {
            for (int i = 0; i < data.Count; i++)
            {
                this.dataGridView1[0, i].Value = data.Keys[i];
                this.dataGridView1[1, i].Value = data.Values[i];
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
                data.RemoveAt(this.dataGridView1.CurrentRow.Index);
                this.dataGridView1.Rows.RemoveAt(this.dataGridView1.CurrentRow.Index);
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
                float time = data.Keys[this.dataGridView1.CurrentRow.Index];
                string text = data.Values[this.dataGridView1.CurrentRow.Index];
                data.RemoveAt(this.dataGridView1.CurrentRow.Index);
                data.Add(time + (float)this.numericUpDown1.Value, text);
                updatedatagrid();
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
                Form4 fm4 = new Form4();
                fm4.setlang(sanal);
                fm4.beforetext = data.Values[this.dataGridView1.CurrentRow.Index];
                if (fm4.ShowDialog() == DialogResult.OK)
                {
                    float time = data.Keys[this.dataGridView1.CurrentRow.Index];
                    string newtext = fm4.newtext.Replace("\n", "");
                    data.RemoveAt(this.dataGridView1.CurrentRow.Index);
                    data.Add(time + (float)this.numericUpDown1.Value, newtext);
                    updatedatagrid();
                }
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
    }
}
