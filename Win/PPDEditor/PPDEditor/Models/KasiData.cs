using System;
using System.Collections.Generic;
using System.Globalization;

namespace PPDEditor.Models
{
    /// <summary>
    /// 歌詞（時間ごとのテキスト）です。kasi.txt の「時間:テキスト」の行と相互に変換します。
    /// </summary>
    public class KasiData
    {
        readonly SortedList<float, string> data = new(100);

        public int Count
        {
            get { return data.Count; }
        }

        public IList<float> Times
        {
            get { return data.Keys; }
        }

        public IList<string> Texts
        {
            get { return data.Values; }
        }

        public IEnumerable<KeyValuePair<float, string>> Items
        {
            get { return data; }
        }

        public bool Contains(float time)
        {
            return data.ContainsKey(time);
        }

        public int IndexOf(float time)
        {
            return data.IndexOfKey(time);
        }

        /// <summary>
        /// 歌詞を足します。同じ時間に歌詞があるときは足さずに false を返します。
        /// </summary>
        public bool Add(float time, string text)
        {
            if (data.ContainsKey(time))
            {
                return false;
            }
            data.Add(time, text);
            return true;
        }

        public void RemoveAt(int index)
        {
            data.RemoveAt(index);
        }

        /// <summary>
        /// 歌詞の時間を変えます。前の時間に歌詞がないときや、新しい時間に歌詞があるときは変えずに false を返します。
        /// </summary>
        public bool Move(float previousTime, float newTime)
        {
            if (!data.TryGetValue(previousTime, out string text) || data.ContainsKey(newTime))
            {
                return false;
            }
            data.Remove(previousTime);
            data.Add(newTime, text);
            return true;
        }

        public void Clear()
        {
            data.Clear();
        }

        /// <summary>
        /// 時間の範囲の中で歌詞が変わる時間です。
        /// </summary>
        public float[] GetTimesWithin(float startTime, float endTime)
        {
            var ret = new List<float>();
            foreach (float time in data.Keys)
            {
                if (time > endTime)
                {
                    break;
                }
                if (startTime <= time)
                {
                    ret.Add(time);
                }
            }
            return [.. ret];
        }

        /// <summary>
        /// 表示する歌詞です。その時間に歌詞がないときは空、空の歌詞のときは空白を返します。
        /// </summary>
        public string GetFormattedContent(float time)
        {
            if (!data.TryGetValue(time, out string value))
            {
                return "";
            }
            return value == "" ? " " : value;
        }

        /// <summary>
        /// その時間に表示している歌詞です（その時間より前で最後の歌詞。なければ最初の歌詞）。
        /// </summary>
        public string GetCurrent(float time)
        {
            if (data.Count == 0)
            {
                return "";
            }
            int i;
            for (i = 0; i < data.Count; i++)
            {
                if (data.Keys[i] > time)
                {
                    break;
                }
            }
            return data.Values[Math.Max(i, 1) - 1];
        }

        /// <summary>
        /// kasi.txt の内容を読み込んで足します。読めない行は飛ばします。
        /// </summary>
        public void Parse(string text)
        {
            text = text.Replace("\t", "").Replace("\r", "");
            foreach (string line in text.Split('\n'))
            {
                var parts = line.Split(':');
                try
                {
                    data.Add(float.Parse(parts[0], CultureInfo.InvariantCulture), parts.Length >= 2 ? parts[1] : "");
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// kasi.txt の内容にします。
        /// </summary>
        public string Serialize()
        {
            var lines = new List<string>();
            foreach (var pair in data)
            {
                lines.Add(pair.Key.ToString(CultureInfo.InvariantCulture) + ":" + pair.Value);
            }
            return String.Join(Environment.NewLine, lines);
        }
    }
}
