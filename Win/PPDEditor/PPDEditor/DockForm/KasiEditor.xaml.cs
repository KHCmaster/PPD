using PPDEditor.Forms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace PPDEditor
{
    public partial class KasiEditor : DockView
    {
        readonly SortedList<float, string> changedata = new(100);
        string alreadyadd = "すでにこの時間にはテキストが指定されています";
        string noselectedline = "選択された行が存在しません";
        public KasiEditor()
        {
            InitializeComponent();
        }
        public void SetLang()
        {
            this.TabText = Utility.Language["KE"];
            this.Column1.Header = Utility.Language["KEGridColumn1"];
            this.Column2.Header = Utility.Language["KEGridColumn2"];
            this.label1.Text = Utility.Language["KELabel1"];
            this.label2.Text = Utility.Language["KELabel2"];
            this.button1.Content = Utility.Language["KEButton1"];
            this.button2.Content = Utility.Language["KEButton2"];
            this.button3.Content = Utility.Language["KEButton3"];
            this.button4.Content = Utility.Language["KEButton4"];
            this.button5.Content = Utility.Language["KEButton5"];
            this.alreadyadd = Utility.Language["KEAlreadyAdded"];
            this.noselectedline = Utility.Language["KENoSelectedLine"];
        }
        public void SetSkin()
        {
            this.label1.Foreground = WpfSkin.TransparentTextBrush;
            this.label2.Foreground = WpfSkin.TransparentTextBrush;
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
            return [.. ret];
        }
        public string GetFormattedContent(float time)
        {
            string ret;
            if (changedata.TryGetValue(time, out string value))
            {
                ret = value;
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
                    dataGridView1.SelectedIndex = index;
                    dataGridView1.ScrollIntoView(dataGridView1.SelectedItem);
                    ShowDock();
                }
            }
        }
        public void ChangeTime(float previoustime, float newtime)
        {
            if (previoustime < 0) return;
            if (changedata.TryGetValue(previoustime, out string kasi) && !changedata.ContainsKey(newtime))
            {
                changedata.Remove(previoustime);
                changedata.Add(newtime, kasi);
                Updatedatagrid();
                ContentChanged();
                WindowUtility.Seekmain.DrawAndRefresh();
            }
        }
        public void Setdata(string s)
        {
            s = Removeextra(s);
            var datas = s.Split('\n');
            foreach (string eachdata in datas)
            {
                var temp = eachdata.Split(':');
                try
                {
                    if (temp.Length >= 2)
                    {
                        changedata.Add(float.Parse(temp[0], CultureInfo.InvariantCulture), temp[1]);
                    }
                    else changedata.Add(float.Parse(temp[0], CultureInfo.InvariantCulture), "");
                }
                catch
                {
                }
            }
            Updatedatagrid();
        }
        public string Askcurrentkasi()
        {
            var time = (float)WindowUtility.Seekmain.Currenttime;
            string ret = "";
            int i;
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
        public string Savedata()
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
            Updatedatagrid();
        }
        private static string Removeextra(string st)
        {
            var a = st.IndexOf('\t');
            while (a != -1)
            {
                st = st.Remove(a, 1);
                a = st.IndexOf('\t');
            }
            a = st.IndexOf('\r');
            while (a != -1)
            {
                st = st.Remove(a, 1);
                a = st.IndexOf('\r');
            }
            return st;
        }
        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            if (!changedata.ContainsKey((float)WindowUtility.Seekmain.Currenttime))
            {
                var text = textBox1.Text.Replace("\n", "");
                changedata.Add((float)WindowUtility.Seekmain.Currenttime, text);
                Updatedatagrid();
                ContentChanged();
            }
            else
            {
                MessageBox.Show(Window.GetWindow(this), alreadyadd);
            }
        }
        private void Updatedatagrid()
        {
            // 選んでいる行は、行の番号で保つ（以前の DataGridView と同じ）
            var selectedIndex = dataGridView1.SelectedIndex;
            dataGridView1.ItemsSource = changedata.ToList();
            if (selectedIndex >= 0 && selectedIndex < changedata.Count)
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

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentIndex < 0)
            {
                MessageBox.Show(Window.GetWindow(this), noselectedline);
            }
            else
            {
                changedata.RemoveAt(CurrentIndex);
                Updatedatagrid();
                ContentChanged();
            }
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentIndex < 0)
            {
                MessageBox.Show(Window.GetWindow(this), noselectedline);
            }
            else
            {
                float time = changedata.Keys[CurrentIndex];
                if (!changedata.ContainsKey(time + (float)this.numericUpDown1.Value))
                {
                    string text = changedata.Values[CurrentIndex];
                    changedata.RemoveAt(CurrentIndex);
                    changedata.Add(time + (float)this.numericUpDown1.Value, text);
                    Updatedatagrid();
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
                var fm4 = new ChangeKasiWindow();
                fm4.SetLang();
                fm4.Beforetext = changedata.Values[CurrentIndex];
                if (WpfDialog.ShowDialog(fm4))
                {
                    float time = changedata.Keys[CurrentIndex];
                    var newtext = fm4.Newtext.Replace("\n", "");
                    changedata.RemoveAt(CurrentIndex);
                    changedata.Add(time, newtext);
                    Updatedatagrid();
                    ContentChanged();
                }
            }
        }

        private void Button5_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentIndex < 0)
            {
                MessageBox.Show(Window.GetWindow(this), noselectedline);
            }
            else
            {
                string text = changedata.Values[CurrentIndex];
                this.textBox1.Text = text;
                this.Button1_Click(this, e);
                ContentChanged();
            }
        }
        private void Button6_Click(object sender, RoutedEventArgs e)
        {
            this.textBox1.Text = "";
            this.textBox1.Focus();
        }
    }
}
