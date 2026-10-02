using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

namespace BMSTOPPD
{
    public partial class MainWindow : Window
    {
        double bpm = 130;
        readonly SettingWindow setting = new();

        public MainWindow()
        {
            InitializeComponent();
        }

        private bool IsDtx
        {
            get { return dtxCheckBox.IsChecked == true; }
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "BMSファイル(*.bms):DTXファイル(*.dtx)|*.bms;*.dtx|すべてのファイル(*.*)|*.*",
                RestoreDirectory = true,
            };
            if (dialog.ShowDialog(this) == true)
            {
                this.fileNameTextBox.Text = dialog.FileName;
            }
        }

        private void ConvertButton_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(this.fileNameTextBox.Text))
            {

                var sr = new StreamReader(this.fileNameTextBox.Text, Encoding.GetEncoding(932));
                var data = sr.ReadToEnd();
                sr.Close();
                data = data.Replace("\r", "");
                try
                {
                    Analysis(data, this.fileNameTextBox.Text);
                }
                catch (Exception)
                {
                    MessageBox.Show("error");
                }
            }
            else
            {
                MessageBox.Show("file not exist");
            }
        }
        private void Analysis(string data, string filePath)
        {
            _ = double.TryParse(this.timeOffsetTextBox.Text, out double timeoffset);
            var bpmpos = data.IndexOf("#BPM");
            int zure = (IsDtx ? 1 : 0);
            if (bpmpos != -1)
            {
                var kaigyou = data.IndexOf('\n', bpmpos);
                bpm = double.Parse(data.Substring(bpmpos + 5 + zure, kaigyou - bpmpos - 5 - zure));
            }
            else
            {
                bpm = 130;
            }
            int anchor = -1;
            ArrayList[] times = new ArrayList[10];
            for (int i = 0; i < times.Length; i++)
            {
                times[i] = new ArrayList(100);
            }
            var bpmchangedata = new SortedList<double, double>(100)
            {
                { 0, bpm }
            };
            var scalechangedata = new SortedList<int, double>(100);

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == '#') anchor = i;
                if (data[i] == ':')
                {
                    if (int.TryParse(data.AsSpan(anchor + 1, 3), out int a))
                    {
                        int syousetu = a;
                        var channel = int.Parse(data.Substring(anchor + 4, 2), (IsDtx ? System.Globalization.NumberStyles.HexNumber : System.Globalization.NumberStyles.Integer));
                        int start = i + 1 + zure;
                        while (data[i] != '\n')
                        {
                            i++;
                        }
                        var sdata = data[start..i];
                        if (channel == 2)
                        {
                            double scale = double.Parse(sdata);
                            scalechangedata.Add(syousetu, scale);
                        }
                        if (channel == 3)
                        {
                            for (int j = 0; j * 2 < sdata.Length; j++)
                            {
                                if (sdata.Substring(2 * j, 2) != "00")
                                {
                                    bpmchangedata.Add(syousetu + (double)j * 2 / sdata.Length, (double)int.Parse(sdata.Substring(2 * j, 2), System.Globalization.NumberStyles.HexNumber));
                                }
                            }
                        }

                        if ((!IsDtx && channel >= 11 && channel <= 19) || (IsDtx && channel >= 11 && channel <= 26))
                        {
                            for (int j = 0; j * 2 < sdata.Length; j++)
                            {
                                if (sdata.Substring(2 * j, 2) != "00")
                                {
                                    times[Conv(channel)].Add(Calctime(bpmchangedata, scalechangedata, syousetu + (double)j * 2 / sdata.Length) + timeoffset);
                                }
                            }
                        }
                    }
                }

            }
            // 変換したファイルと同じフォルダーに出力する
            string path = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(filePath)), "trans.ppd");
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            var fs = File.Create(path);
            var sign = "PPD"u8.ToArray();
            fs.Write(sign, 0, sign.Length);
            int markcount = 0;
            for (int i = 0; i < times.Length; i++)
            {
                markcount += times[i].Count;
            }
            int[] iters = new int[10];
            while (true)
            {
                int minimum = -1;
                double minimumtime = double.MaxValue;
                for (int i = 0; i < 10; i++)
                {
                    if (iters[i] < times[i].Count)
                    {
                        if (minimumtime >= (double)times[i][iters[i]])
                        {
                            minimum = i;
                            minimumtime = (double)times[i][iters[i]];
                        }
                    }
                }
                if (minimum == -1)
                {
                    break;
                }
                else
                {
                    var t = System.BitConverter.GetBytes((float)((double)times[minimum][iters[minimum]]));
                    fs.Write(t, 0, t.Length);
                    var x = setting.X(minimum);
                    var y = setting.Y(minimum);
                    t = System.BitConverter.GetBytes(x);
                    fs.Write(t, 0, t.Length);
                    t = System.BitConverter.GetBytes(y);
                    fs.Write(t, 0, t.Length);
                    t = System.BitConverter.GetBytes(setting.Angle(minimum));
                    fs.Write(t, 0, t.Length);
                    fs.WriteByte((byte)minimum);
                    iters[minimum]++;
                }
            }
            fs.Close();
            MessageBox.Show(path + "\nを生成しました");
        }
        private static double Calctime(SortedList<double, double> bpmchangedata, SortedList<int, double> scalechangedata, double syousetu)
        {
            double ret = 0;
            var numsyousetu = (int)syousetu;
            double bpm = bpmchangedata.Values[0];
            for (int i = 0; i < numsyousetu; i++)
            {
                if (!scalechangedata.TryGetValue(i, out double scale))
                {
                    scale = 1;
                }
                bpm = bpmchangedata.Values[0];
                int j;
                for (j = 0; j < bpmchangedata.Count; j++)
                {
                    if (bpmchangedata.Keys[j] < i)
                    {
                        bpm = bpmchangedata.Values[j];
                    }
                    else
                    {
                        break;
                    }
                }
                if (j != bpmchangedata.Count)
                {
                    double ss = i;
                    while (bpmchangedata.Keys[j] <= i + 1)
                    {
                        ret += 60 / bpm * 4 * (bpmchangedata.Keys[j] - ss) * scale;
                        ss = bpmchangedata.Keys[j];
                        bpm = bpmchangedata.Values[j];
                        j++;
                        if (j == bpmchangedata.Count)
                        {
                            break;
                        }
                    }
                    ret += 60 / bpm * 4 * (i + 1 - ss) * scale;
                }
                else
                {
                    ret += 60 / bpm * 4 * scale;
                }
            }
            double sss = numsyousetu;
            if (!scalechangedata.TryGetValue(numsyousetu, out double Scale))
            {
                Scale = 1;
            }
            foreach (double s in bpmchangedata.Keys)
            {
                if (numsyousetu <= s && s <= syousetu)
                {
                    ret += 60 / bpm * 4 * (s - sss) * Scale;
                    sss = s;
                    bpmchangedata.TryGetValue(sss, out bpm);
                }
                else
                {
                    if (s < numsyousetu) continue;
                    if (s > syousetu) break;
                }
            }
            ret += 60 / bpm * 4 * (syousetu - sss) * Scale;
            return ret;
        }
        private int Conv(int num)
        {
            if (IsDtx)
            {
                return num - 17;
            }
            else
            {
                if (num <= 15)
                {
                    return num - 11;
                }
                else if (num >= 18)
                {
                    return num - 13;
                }
                else if (num == 16)
                {
                    return 8;
                }
                else
                {
                    return 9;
                }
            }
        }

        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            setting.Owner ??= this;
            setting.Show();
            setting.Activate();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            setting.Savedata();
        }
    }
}
