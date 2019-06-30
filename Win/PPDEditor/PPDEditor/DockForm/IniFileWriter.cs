using PPDConfiguration;
using PPDEditor.Forms;
using PPDFramework;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace PPDEditor
{
    public partial class IniFileWriter : ScrollableForm
    {
        Guid guid = Guid.NewGuid();
        string numbererror = "エラー。数値でない可能性があります";
        string gettimefromtimeline = "タイムラインから読み込み";
        string parseerror = "数値変換エラーです。@";
        public IniFileWriter()
        {
            InitializeComponent();
            CreateRow("サムネの開始時間", "0");
            CreateRow("サムネの終了時間", "0");
            CreateRow("開始時間", "0");
            CreateRow("終了時間", "0");
            this.dataGridView1.Rows.Add(new object[] { "難易度 EASY", "" });
            this.dataGridView1.Rows.Add(new object[] { "難易度 NORMAL", "" });
            this.dataGridView1.Rows.Add(new object[] { "難易度 HARD", "" });
            this.dataGridView1.Rows.Add(new object[] { "難易度 EXTREME", "" });
            this.dataGridView1.Rows.Add(new object[] { "BPM", "100" });
            this.dataGridView1.Rows.Add(new object[] { "BPMの文字列表記", "" });
            this.dataGridView1.Rows.Add(new object[] { "動画の左カット(px)", "0" });
            this.dataGridView1.Rows.Add(new object[] { "動画の右カット(px)", "0" });
            this.dataGridView1.Rows.Add(new object[] { "動画の上カット(px)", "0" });
            this.dataGridView1.Rows.Add(new object[] { "動画の下カット(px)", "0" });
            this.dataGridView1.Size = new Size(this.dataGridView1.Width, this.dataGridView1.RowCount * this.dataGridView1.Rows[0].Height + 5);
            dataGridView1.CurrentCellDirtyStateChanged += dataGridView1_CurrentCellDirtyStateChanged;
            dataGridView1.DataError += dataGridView1_DataError;
        }

        void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.Cancel = true;
        }

        void dataGridView1_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataGridView1.IsCurrentCellDirty)
            {
                dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }

        }
        private DataGridViewRow CreateRow(string name, string defaultvalue)
        {
            var row = new DataGridViewRow();
            var c = new DataGridViewTextBoxCell
            {
                Value = name
            };
            row.Cells.Add(c);
            var cell = new ExDataGridViewComboBoxCell();
            cell.SelectIndexChanged += cell_SelectIndexChanged;
            cell.Items.Add(gettimefromtimeline);
            cell.Items.Add(defaultvalue);
            cell.Value = defaultvalue;
            row.Cells.Add(cell);
            this.dataGridView1.Rows.Add(row);
            cell.ColumnName = "Column2";
            cell.SetEvent();
            return row;
        }
        void cell_SelectIndexChanged(DataGridViewComboBoxEditingControl cb)
        {
            if (cb.SelectedItem != null && cb.SelectedItem.ToString() == gettimefromtimeline)
            {
                var adddata = ((float)WindowUtility.Seekmain.Currenttime).ToString();
                cb.Items.Add(adddata);
                if (cb.Items.Count > 0) cb.SelectedIndex = cb.Items.Count - 1;
                cb.EditingControlDataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);
                cb.EditingControlDataGridView[1, cb.EditingControlRowIndex].Value = cb.SelectedItem.ToString();
            }
        }
        public void SetLang()
        {
            this.Text = Utility.Language["SFW"];
            string newtext = Utility.Language["SFWButton"];
            for (int i = 0; i < 4; i++)
            {
                var c = this.dataGridView1[1, i] as DataGridViewComboBoxCell;
                c.Items.Remove(gettimefromtimeline);
                c.Items.Insert(0, newtext);
            }
            gettimefromtimeline = newtext;
            this.numbererror = Utility.Language["SFWNumberError"];
            this.dataGridView1[0, 0].Value = Utility.Language["SFWLabel1"];
            this.dataGridView1[0, 1].Value = Utility.Language["SFWLabel2"];
            this.dataGridView1[0, 2].Value = Utility.Language["SFWLabel3"];
            this.dataGridView1[0, 3].Value = Utility.Language["SFWLabel4"];
            this.dataGridView1[0, 4].Value = Utility.Language["SFWLabel5"];
            this.dataGridView1[0, 5].Value = Utility.Language["SFWLabel6"];
            this.dataGridView1[0, 6].Value = Utility.Language["SFWLabel7"];
            this.dataGridView1[0, 8].Value = Utility.Language["SFWLabel8"];
            this.dataGridView1[0, 10].Value = Utility.Language["SFWLabel9"];
            this.dataGridView1[0, 11].Value = Utility.Language["SFWLabel10"];
            this.dataGridView1[0, 12].Value = Utility.Language["SFWLabel11"];
            this.dataGridView1[0, 13].Value = Utility.Language["SFWLabel12"];
            this.dataGridView1[0, 7].Value = Utility.Language["SFWLabel13"];
            this.dataGridView1[0, 9].Value = Utility.Language["BPMString"];
        }
        public void SetSkin()
        {
        }

        public bool SaveIni(SettingWriter sw)
        {
            dataGridView1_CurrentCellDirtyStateChanged(dataGridView1, EventArgs.Empty);
            floatreaddata(0, out float thumbtimestart);
            floatreaddata(1, out float thumbtimeend);
            floatreaddata(2, out float start);
            floatreaddata(3, out float end);
            stringreaddata(4, out string easy);
            stringreaddata(5, out string normal);
            stringreaddata(6, out string hard);
            stringreaddata(7, out string extreme);
            floatreaddata(8, out float bpm);
            stringreaddata(9, out string bpmstring);
            floatreaddata(10, out float left);
            floatreaddata(11, out float right);
            floatreaddata(12, out float top);
            floatreaddata(13, out float bottom);
            sw.Write("thumbtimestart", thumbtimestart);
            sw.Write("thumbtimeend", thumbtimeend);
            sw.Write("start", start);
            sw.Write("end", end);
            sw.Write("bpm", bpm);
            sw.Write("bpmstring", bpmstring);
            sw.Write("difficulty easy", easy);
            sw.Write("difficulty normal", normal);
            sw.Write("difficulty hard", hard);
            sw.Write("difficulty extreme", extreme);
            sw.Write("moviecutleft", left);
            sw.Write("moviecutright", right);
            sw.Write("moviecuttop", top);
            sw.Write("moviecutbottom", bottom);
            sw.Write("guid", guid.ToString());
            sw.Write("authorname", PPDStaticSetting.AuthorName);
            sw.Write("latency", 0);
            return true;
        }

        public void Clear()
        {
            this.guid = Guid.NewGuid();
            for (int i = 0; i < 4; i++)
            {
                if (!(this.dataGridView1[1, i] as DataGridViewComboBoxCell).Items.Contains("0"))
                {
                    (this.dataGridView1[1, i] as DataGridViewComboBoxCell).Items.Add("0");
                }
            }
            this.dataGridView1[1, 0].Value = "0";
            this.dataGridView1[1, 1].Value = "0";
            this.dataGridView1[1, 2].Value = "0";
            this.dataGridView1[1, 3].Value = "0";
            this.dataGridView1[1, 4].Value = "";
            this.dataGridView1[1, 5].Value = "";
            this.dataGridView1[1, 6].Value = "";
            this.dataGridView1[1, 7].Value = "";
            this.dataGridView1[1, 8].Value = "100";
            this.dataGridView1[1, 9].Value = "";
            this.dataGridView1[1, 10].Value = "0";
            this.dataGridView1[1, 11].Value = "0";
            this.dataGridView1[1, 12].Value = "0";
            this.dataGridView1[1, 13].Value = "0";
        }

        private void floatreaddata(int rowindex, out float outvalue)
        {
            try
            {
                outvalue = float.Parse(this.dataGridView1[1, rowindex].Value.ToString());
            }
            catch
            {
                MessageBox.Show(parseerror + this.dataGridView1[0, rowindex].Value);
                outvalue = 0;
            }
        }
        private void intreaddata(int rowindex, out int outvalue)
        {
            try
            {
                outvalue = int.Parse(this.dataGridView1[1, rowindex].Value.ToString());
            }
            catch
            {
                MessageBox.Show(parseerror + this.dataGridView1[0, rowindex].Value);
                outvalue = 0;
            }
        }
        private void stringreaddata(int rowindex, out string outvalue)
        {
            try
            {
                outvalue = this.dataGridView1[1, rowindex].Value.ToString();
            }
            catch
            {
                outvalue = "";
            }
        }
        public void SetIni(float[] times, string[] difficults, float bpm, string bpmstring, MovieTrimmingData trimmingData)
        {
            setdata(times[0], 0, 1);
            setdata(times[1], 1, 1);
            setdata(times[2], 2, 1);
            setdata(times[3], 3, 1);
            this.dataGridView1[1, 4].Value = difficults[0];
            this.dataGridView1[1, 5].Value = difficults[1];
            this.dataGridView1[1, 6].Value = difficults[2];
            this.dataGridView1[1, 7].Value = difficults[3];
            this.dataGridView1[1, 8].Value = bpm.ToString();
            this.dataGridView1[1, 9].Value = bpmstring;
            SetTrimmingData(trimmingData);
        }
        public void SetTrimmingData(MovieTrimmingData trimmingData)
        {
            this.dataGridView1[1, 10].Value = trimmingData.Left;
            this.dataGridView1[1, 11].Value = trimmingData.Right;
            this.dataGridView1[1, 12].Value = trimmingData.Top;
            this.dataGridView1[1, 13].Value = trimmingData.Bottom;
        }
        private void setdata(float value, int rowindex, int columnindex)
        {
            var cell = this.dataGridView1[columnindex, rowindex] as DataGridViewComboBoxCell;
            cell.Items.Add(value.ToString());
            cell.Value = value.ToString();
            for (int i = cell.Items.Count - 1; i >= 0; i--)
            {
                if (cell.Items[i].ToString() != gettimefromtimeline && cell.Items[i].ToString() != value.ToString()) cell.Items.RemoveAt(i);
            }
        }
        public float BPM
        {
            get
            {
                if (!float.TryParse(this.dataGridView1[1, 8].Value.ToString(), out float ret))
                {
                    ret = 100;
                }
                return ret;
            }
        }
        public float StartTime
        {
            get
            {
                float.TryParse(this.dataGridView1[1, 2].Value.ToString(), out float ret);
                return ret;
            }
        }
        public float EndTime
        {
            get
            {
                float.TryParse(this.dataGridView1[1, 3].Value.ToString(), out float ret);
                return ret;
            }
        }
        public Guid SongGuid
        {
            get
            {
                return guid;
            }
            set
            {
                guid = value;
            }
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == 8)
            {
                CheckBpm();
            }
        }

        private void CheckBpm()
        {
            WindowUtility.EventManager.CheckInitialBpm(BPM);
        }
    }
}
