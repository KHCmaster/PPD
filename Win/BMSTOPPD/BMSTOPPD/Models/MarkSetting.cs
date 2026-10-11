using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;

namespace BMSTOPPD.Models
{
    /// <summary>
    /// ボタンごとのノーツの位置と角度です。表で編集するので、値は入力した文字列のまま持ちます。
    /// </summary>
    public partial class MarkSetting : ObservableObject
    {
        [ObservableProperty]
        public partial string Name { get; set; }

        [ObservableProperty]
        public partial string X { get; set; }

        [ObservableProperty]
        public partial string Y { get; set; }

        [ObservableProperty]
        public partial string Angle { get; set; }

        public float GetX()
        {
            return float.Parse(X);
        }

        public float GetY()
        {
            return float.Parse(Y);
        }

        /// <summary>
        /// 角度（度で入力したもの）をラジアンで返します。
        /// </summary>
        public float GetAngleInRadians()
        {
            return (float)Math.PI * float.Parse(Angle) / 180;
        }
    }

    /// <summary>
    /// ノーツの位置と角度の設定を setting.ini に読み書きします（1 行に「X,Y,角度」）。
    /// </summary>
    public static class MarkSettingStore
    {
        const string FileName = "setting.ini";
        static readonly string[] buttonNames = ["square", "cross", "circle", "triangle", "left", "down", "right", "up", "R", "L"];

        public static List<MarkSetting> Load()
        {
            if (!File.Exists(FileName))
            {
                using var sw = new StreamWriter(FileName);
                for (int i = 0; i < buttonNames.Length; i++)
                {
                    sw.Write("400,255,0\n");
                }
            }
            var settings = new List<MarkSetting>();
            using var sr = new StreamReader(FileName);
            foreach (var name in buttonNames)
            {
                var strs = sr.ReadLine().Split(',');
                settings.Add(new MarkSetting { Name = name, X = strs[0], Y = strs[1], Angle = strs[2] });
            }
            return settings;
        }

        public static void Save(IEnumerable<MarkSetting> settings)
        {
            using var sw = new StreamWriter(FileName);
            foreach (var setting in settings)
            {
                sw.Write(setting.X + "," + setting.Y + "," + setting.Angle + "\n");
            }
        }
    }
}
