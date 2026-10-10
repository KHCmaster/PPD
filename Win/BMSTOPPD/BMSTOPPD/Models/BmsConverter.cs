using System;
using System.Collections.Generic;
using System.IO;

namespace BMSTOPPD.Models
{
    /// <summary>
    /// BMS（または DTX）の譜面を PPD の譜面（trans.ppd）に変換します。
    /// </summary>
    public static class BmsConverter
    {
        /// <summary>
        /// 変換して、変換元と同じフォルダーに trans.ppd を書きます。
        /// </summary>
        /// <param name="data">譜面のテキスト（改行は \n）。</param>
        /// <param name="filePath">変換元のファイル。</param>
        /// <param name="timeOffset">すべてのノーツの時間に足す秒数。</param>
        /// <param name="isDtx">DTX の形式かどうか。</param>
        /// <param name="settings">ボタンごとのノーツの位置と角度。</param>
        /// <returns>書いたファイルのパス。</returns>
        public static string Convert(string data, string filePath, double timeOffset, bool isDtx, IReadOnlyList<MarkSetting> settings)
        {
            var times = Analyze(data, timeOffset, isDtx);
            // 変換したファイルと同じフォルダーに出力する
            string path = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(filePath)), "trans.ppd");
            Write(path, times, settings);
            return path;
        }

        /// <summary>
        /// 譜面を読んで、ボタンごとのノーツの時間（秒）を返します。
        /// </summary>
        public static List<double>[] Analyze(string data, double timeOffset, bool isDtx)
        {
            double bpm;
            var bpmpos = data.IndexOf("#BPM");
            int zure = isDtx ? 1 : 0;
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
            var times = new List<double>[10];
            for (int i = 0; i < times.Length; i++)
            {
                times[i] = new List<double>(100);
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
                        var channel = int.Parse(data.Substring(anchor + 4, 2), isDtx ? System.Globalization.NumberStyles.HexNumber : System.Globalization.NumberStyles.Integer);
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
                                    bpmchangedata.Add(syousetu + (double)j * 2 / sdata.Length, int.Parse(sdata.Substring(2 * j, 2), System.Globalization.NumberStyles.HexNumber));
                                }
                            }
                        }

                        if ((!isDtx && channel >= 11 && channel <= 19) || (isDtx && channel >= 11 && channel <= 26))
                        {
                            for (int j = 0; j * 2 < sdata.Length; j++)
                            {
                                if (sdata.Substring(2 * j, 2) != "00")
                                {
                                    times[Conv(channel, isDtx)].Add(Calctime(bpmchangedata, scalechangedata, syousetu + (double)j * 2 / sdata.Length) + timeOffset);
                                }
                            }
                        }
                    }
                }
            }
            return times;
        }

        /// <summary>
        /// ノーツを時間の順に、PPD の形式で書きます。
        /// </summary>
        private static void Write(string path, List<double>[] times, IReadOnlyList<MarkSetting> settings)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            using var fs = File.Create(path);
            var sign = "PPD"u8.ToArray();
            fs.Write(sign, 0, sign.Length);
            int[] iters = new int[10];
            while (true)
            {
                int minimum = -1;
                double minimumtime = double.MaxValue;
                for (int i = 0; i < 10; i++)
                {
                    if (iters[i] < times[i].Count)
                    {
                        if (minimumtime >= times[i][iters[i]])
                        {
                            minimum = i;
                            minimumtime = times[i][iters[i]];
                        }
                    }
                }
                if (minimum == -1)
                {
                    break;
                }
                var t = BitConverter.GetBytes((float)times[minimum][iters[minimum]]);
                fs.Write(t, 0, t.Length);
                t = BitConverter.GetBytes(settings[minimum].GetX());
                fs.Write(t, 0, t.Length);
                t = BitConverter.GetBytes(settings[minimum].GetY());
                fs.Write(t, 0, t.Length);
                t = BitConverter.GetBytes(settings[minimum].GetAngleInRadians());
                fs.Write(t, 0, t.Length);
                fs.WriteByte((byte)minimum);
                iters[minimum]++;
            }
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

        private static int Conv(int num, bool isDtx)
        {
            if (isDtx)
            {
                return num - 17;
            }
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
}
