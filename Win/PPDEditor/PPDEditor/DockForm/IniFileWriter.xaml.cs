using PPDConfiguration;
using PPDEditor.Forms;
using PPDFramework;
using System;
using System.Windows;
using System.Windows.Controls;

namespace PPDEditor
{
    /// <summary>
    /// 譜面の設定ファイル（ini）に書く値を編集します。
    /// </summary>
    public partial class IniFileWriter : DockView
    {
        const int TimeRowCount = 4;
        const int BpmRow = 8;

        static readonly string[] defaultNames = [
            "サムネの開始時間", "サムネの終了時間", "開始時間", "終了時間",
            "難易度 EASY", "難易度 NORMAL", "難易度 HARD", "難易度 EXTREME",
            "BPM", "BPMの文字列表記",
            "動画の左カット(px)", "動画の右カット(px)", "動画の上カット(px)", "動画の下カット(px)"
        ];
        static readonly string[] defaultValues = ["0", "0", "0", "0", "", "", "", "", "100", "", "0", "0", "0", "0"];

        // 言語ファイルのキー（行の順）。以前の表と同じ対応
        static readonly string[] nameKeys = [
            "SFWLabel1", "SFWLabel2", "SFWLabel3", "SFWLabel4",
            "SFWLabel5", "SFWLabel6", "SFWLabel7", "SFWLabel13",
            "SFWLabel8", "BPMString",
            "SFWLabel9", "SFWLabel10", "SFWLabel11", "SFWLabel12"
        ];

        readonly TextBlock[] nameLabels = new TextBlock[defaultNames.Length];
        readonly TextBox[] valueBoxes = new TextBox[defaultNames.Length];
        readonly Button[] timeButtons = new Button[TimeRowCount];

        Guid guid = Guid.NewGuid();
        string numbererror = "エラー。数値でない可能性があります";
        readonly string parseerror = "数値変換エラーです。@";

        public IniFileWriter()
        {
            InitializeComponent();
            for (int i = 0; i < defaultNames.Length; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(26) });
                var label = new TextBlock { Text = defaultNames[i], VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 6, 0) };
                Grid.SetRow(label, i);
                grid.Children.Add(label);
                nameLabels[i] = label;
                var box = new TextBox { Text = defaultValues[i], Height = 22, VerticalContentAlignment = VerticalAlignment.Center };
                Grid.SetRow(box, i);
                Grid.SetColumn(box, 1);
                grid.Children.Add(box);
                valueBoxes[i] = box;
                if (i < TimeRowCount)
                {
                    // 以前は値の候補の「タイムラインから読み込み」を選んで、今の時間を入れていた
                    var row = i;
                    var button = new Button { Content = "タイムラインから読み込み", Height = 22, Margin = new Thickness(4, 0, 0, 0), Padding = new Thickness(4, 0, 4, 0) };
                    button.Click += (sender, e) => valueBoxes[row].Text = ((float)WindowUtility.Seekmain.Currenttime).ToString();
                    Grid.SetRow(button, i);
                    Grid.SetColumn(button, 2);
                    grid.Children.Add(button);
                    timeButtons[i] = button;
                }
            }
            // BPM を変えたら、イベントの最初の BPM を確かめる（以前は表の編集を終えたとき）
            valueBoxes[BpmRow].LostKeyboardFocus += (sender, e) => CheckBpm();
        }

        public void SetLang()
        {
            this.TabText = Utility.Language["SFW"];
            foreach (var button in timeButtons)
            {
                button.Content = Utility.Language["SFWButton"];
            }
            this.numbererror = Utility.Language["SFWNumberError"];
            for (int i = 0; i < nameKeys.Length; i++)
            {
                nameLabels[i].Text = Utility.Language[nameKeys[i]];
            }
        }

        public void SetSkin()
        {
            var brush = WpfSkin.TransparentTextBrush;
            foreach (var label in nameLabels)
            {
                label.Foreground = brush;
            }
        }

        public bool SaveIni(SettingWriter sw)
        {
            Floatreaddata(0, out float thumbtimestart);
            Floatreaddata(1, out float thumbtimeend);
            Floatreaddata(2, out float start);
            Floatreaddata(3, out float end);
            Stringreaddata(4, out string easy);
            Stringreaddata(5, out string normal);
            Stringreaddata(6, out string hard);
            Stringreaddata(7, out string extreme);
            Floatreaddata(8, out float bpm);
            Stringreaddata(9, out string bpmstring);
            Floatreaddata(10, out float left);
            Floatreaddata(11, out float right);
            Floatreaddata(12, out float top);
            Floatreaddata(13, out float bottom);
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
            for (int i = 0; i < defaultValues.Length; i++)
            {
                valueBoxes[i].Text = defaultValues[i];
            }
        }

        private void Floatreaddata(int rowindex, out float outvalue)
        {
            if (!float.TryParse(valueBoxes[rowindex].Text, out outvalue))
            {
                MessageBox.Show(Window.GetWindow(this), parseerror + nameLabels[rowindex].Text);
                outvalue = 0;
            }
        }

        private void Stringreaddata(int rowindex, out string outvalue)
        {
            outvalue = valueBoxes[rowindex].Text ?? "";
        }

        public void SetIni(float[] times, string[] difficults, float bpm, string bpmstring, MovieTrimmingData trimmingData)
        {
            for (int i = 0; i < TimeRowCount; i++)
            {
                valueBoxes[i].Text = times[i].ToString();
            }
            valueBoxes[4].Text = difficults[0];
            valueBoxes[5].Text = difficults[1];
            valueBoxes[6].Text = difficults[2];
            valueBoxes[7].Text = difficults[3];
            valueBoxes[BpmRow].Text = bpm.ToString();
            valueBoxes[9].Text = bpmstring;
            SetTrimmingData(trimmingData);
        }

        public void SetTrimmingData(MovieTrimmingData trimmingData)
        {
            valueBoxes[10].Text = trimmingData.Left.ToString();
            valueBoxes[11].Text = trimmingData.Right.ToString();
            valueBoxes[12].Text = trimmingData.Top.ToString();
            valueBoxes[13].Text = trimmingData.Bottom.ToString();
        }

        public float BPM
        {
            get
            {
                if (!float.TryParse(valueBoxes[BpmRow].Text, out float ret))
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
                _ = float.TryParse(valueBoxes[2].Text, out float ret);
                return ret;
            }
        }

        public float EndTime
        {
            get
            {
                _ = float.TryParse(valueBoxes[3].Text, out float ret);
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

        private void CheckBpm()
        {
            WindowUtility.EventManager.CheckInitialBpm(BPM);
        }
    }
}
