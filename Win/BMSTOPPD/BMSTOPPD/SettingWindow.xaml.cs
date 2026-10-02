using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace BMSTOPPD
{
    /// <summary>
    /// ボタンごとのノーツの位置と角度を設定するウィンドウです（setting.ini に保存します）。
    /// </summary>
    public partial class SettingWindow : Window
    {
        private static readonly string[] buttonNames = ["square", "cross", "circle", "triangle", "left", "down", "right", "up", "R", "L"];
        private readonly List<MarkSetting> settings = [];

        public SettingWindow()
        {
            InitializeComponent();
            if (!File.Exists("setting.ini"))
            {
                using var sw = new StreamWriter("setting.ini");
                for (int i = 0; i < buttonNames.Length; i++)
                {
                    sw.Write("400,255,0\n");
                }
            }
            using (var sr = new StreamReader("setting.ini"))
            {
                foreach (var name in buttonNames)
                {
                    var strs = sr.ReadLine().Split(',');
                    settings.Add(new MarkSetting { Name = name, X = strs[0], Y = strs[1], Angle = strs[2] });
                }
            }
            dataGrid.ItemsSource = settings;
        }

        /// <summary>
        /// 設定を setting.ini に保存します。
        /// </summary>
        public void Savedata()
        {
            dataGrid.CommitEdit();
            using var sw = new StreamWriter("setting.ini");
            foreach (var setting in settings)
            {
                sw.Write(setting.X + "," + setting.Y + "," + setting.Angle + "\n");
            }
        }

        // 閉じても設定を保持するため、隠すだけにする（アプリの終了時に保存する）
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        public float X(int num)
        {
            return float.Parse(settings[num].X);
        }

        public float Y(int num)
        {
            return float.Parse(settings[num].Y);
        }

        public float Angle(int num)
        {
            return (float)Math.PI * float.Parse(settings[num].Angle) / 180;
        }

        public class MarkSetting
        {
            public string Name { get; set; }

            public string X { get; set; }

            public string Y { get; set; }

            public string Angle { get; set; }
        }
    }
}
