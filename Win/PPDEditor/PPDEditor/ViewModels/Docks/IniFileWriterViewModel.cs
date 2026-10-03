using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PPDConfiguration;
using PPDFramework;
using System;

namespace PPDEditor.ViewModels.Docks
{
    /// <summary>
    /// 譜面の設定ファイル（ini）に書く値（時間、難易度、BPM、動画の切り取り）を編集します。
    /// </summary>
    /// <param name="getCurrentTime">タイムラインの今の時間を返します（時間の行に読み込むときに使う）。</param>
    /// <param name="checkBpm">BPM を変えたときに、イベントの最初の BPM を確かめます。</param>
    /// <param name="showError">値が数字でないことを知らせます。</param>
    public partial class IniFileWriterViewModel(Func<float> getCurrentTime, Action<float> checkBpm, Action<string> showError) : DockViewModel
    {
        const int TimeRowCount = 4;
        const int BpmRow = 8;
        const string ParseError = "数値変換エラーです。@";

        // 言語ファイルのキーと初期値（行の順）
        static readonly (string Key, string Default)[] rowDefinitions = [
            ("SFWLabel1", "0"), ("SFWLabel2", "0"), ("SFWLabel3", "0"), ("SFWLabel4", "0"),
            ("SFWLabel5", ""), ("SFWLabel6", ""), ("SFWLabel7", ""), ("SFWLabel13", ""),
            ("SFWLabel8", "100"), ("BPMString", ""),
            ("SFWLabel9", "0"), ("SFWLabel10", "0"), ("SFWLabel11", "0"), ("SFWLabel12", "0")
        ];

        public IniRow[] Rows { get; } = CreateRows();

        /// <summary>
        /// 曲の ID です。
        /// </summary>
        public Guid SongGuid
        {
            get;
            set;
        } = Guid.NewGuid();

        public float BPM
        {
            get { return float.TryParse(Rows[BpmRow].Value, out float value) ? value : 100; }
        }

        public float StartTime
        {
            get { return ParseOrZero(Rows[2].Value); }
        }

        public float EndTime
        {
            get { return ParseOrZero(Rows[3].Value); }
        }

        private static IniRow[] CreateRows()
        {
            var rows = new IniRow[rowDefinitions.Length];
            for (int i = 0; i < rows.Length; i++)
            {
                rows[i] = new IniRow(rowDefinitions[i].Key, rowDefinitions[i].Default, i < TimeRowCount, i == BpmRow);
            }
            return rows;
        }

        // 以前は値の候補の「タイムラインから読み込み」を選んで、今の時間を入れていた
        [RelayCommand]
        private void LoadTime(IniRow row)
        {
            row.Value = getCurrentTime().ToString();
        }

        /// <summary>
        /// BPM の欄の編集が終わったときに呼びます。
        /// </summary>
        public void CheckBpm()
        {
            checkBpm(BPM);
        }

        public void Clear()
        {
            SongGuid = Guid.NewGuid();
            for (int i = 0; i < Rows.Length; i++)
            {
                Rows[i].Value = rowDefinitions[i].Default;
            }
        }

        public void SetIni(float[] times, string[] difficulties, float bpm, string bpmString, MovieTrimmingData trimmingData)
        {
            for (int i = 0; i < TimeRowCount; i++)
            {
                Rows[i].Value = times[i].ToString();
            }
            for (int i = 0; i < 4; i++)
            {
                Rows[4 + i].Value = difficulties[i];
            }
            Rows[BpmRow].Value = bpm.ToString();
            Rows[9].Value = bpmString;
            SetTrimmingData(trimmingData);
        }

        public void SetTrimmingData(MovieTrimmingData trimmingData)
        {
            Rows[10].Value = trimmingData.Left.ToString();
            Rows[11].Value = trimmingData.Right.ToString();
            Rows[12].Value = trimmingData.Top.ToString();
            Rows[13].Value = trimmingData.Bottom.ToString();
        }

        /// <summary>
        /// 設定ファイルに書きます。数字でない欄は 0 にして知らせます。
        /// </summary>
        public bool SaveIni(SettingWriter sw)
        {
            float ReadFloat(int index)
            {
                if (float.TryParse(Rows[index].Value, out float value))
                {
                    return value;
                }
                showError(ParseError + Rows[index].Name);
                return 0;
            }
            var thumbTimeStart = ReadFloat(0);
            var thumbTimeEnd = ReadFloat(1);
            var start = ReadFloat(2);
            var end = ReadFloat(3);
            var bpm = ReadFloat(BpmRow);
            var left = ReadFloat(10);
            var right = ReadFloat(11);
            var top = ReadFloat(12);
            var bottom = ReadFloat(13);
            sw.Write("thumbtimestart", thumbTimeStart);
            sw.Write("thumbtimeend", thumbTimeEnd);
            sw.Write("start", start);
            sw.Write("end", end);
            sw.Write("bpm", bpm);
            sw.Write("bpmstring", Rows[9].Value ?? "");
            sw.Write("difficulty easy", Rows[4].Value ?? "");
            sw.Write("difficulty normal", Rows[5].Value ?? "");
            sw.Write("difficulty hard", Rows[6].Value ?? "");
            sw.Write("difficulty extreme", Rows[7].Value ?? "");
            sw.Write("moviecutleft", left);
            sw.Write("moviecutright", right);
            sw.Write("moviecuttop", top);
            sw.Write("moviecutbottom", bottom);
            sw.Write("guid", SongGuid.ToString());
            sw.Write("authorname", PPDStaticSetting.AuthorName);
            sw.Write("latency", 0);
            return true;
        }

        private static float ParseOrZero(string text)
        {
            _ = float.TryParse(text, out float value);
            return value;
        }
    }

    /// <summary>
    /// 設定ファイルの 1 つの値です。
    /// </summary>
    public class IniRow(string nameKey, string value, bool isTime, bool isBpm) : ObservableObject
    {
        public string NameKey { get; } = nameKey;

        public string Name
        {
            get { return LanguageTexts.Instance[NameKey]; }
        }

        private string text = value;

        public string Value
        {
            get { return text; }
            set { SetProperty(ref text, value); }
        }

        /// <summary>
        /// 時間の行かです（タイムラインから読み込むボタンを出す）。
        /// </summary>
        public bool IsTime { get; } = isTime;

        public bool IsBpm { get; } = isBpm;
    }
}
