using PPDConfiguration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Color = System.Drawing.Color;
using ColorTranslator = System.Drawing.ColorTranslator;

namespace Effect2DEditor.Models
{
    /// <summary>
    /// カンバスの背景の種類です（設定ファイルには数で保存する）。
    /// </summary>
    public enum BackGroundMode
    {
        Default = 0,
        Color = 1,
        Image = 2
    }

    /// <summary>
    /// エディターの設定（Effect2DEditor.ini）と、最近使ったファイル（recentused.ini）です。
    /// </summary>
    public class EditorSettings
    {
        const string iniFileName = "Effect2DEditor.ini";
        const string recentUsedFileName = "recentused.ini";
        public const int DefaultCanvasSize = 512;

        /// <summary>
        /// 言語（言語ファイルの名前の最後の部分）。選んでいないときは日本語。
        /// </summary>
        public string Language { get; set; } = "jp";

        public double? WindowWidth { get; set; }

        public double? WindowHeight { get; set; }

        public bool Maximized { get; set; }

        public int CanvasWidth { get; set; } = DefaultCanvasSize;

        public int CanvasHeight { get; set; } = DefaultCanvasSize;

        public BackGroundMode BackGroundMode { get; set; }

        public Color BackGroundColor { get; set; } = Color.White;

        public string BackGroundImage { get; set; } = "";

        /// <summary>
        /// 最近使ったファイル（新しい順）。
        /// </summary>
        public List<string> RecentFiles { get; } = [];

        /// <summary>
        /// 設定を読み込みます。読めない値は既定のままにします。
        /// </summary>
        public static EditorSettings Load()
        {
            var settings = new EditorSettings();
            try
            {
                if (File.Exists(iniFileName))
                {
                    var setting = new SettingReader(File.ReadAllText(iniFileName));
                    if (!string.IsNullOrEmpty(setting["Language"])) settings.Language = setting["Language"];
                    if (int.TryParse(setting["WindowWidth"], out int temp)) settings.WindowWidth = temp;
                    if (int.TryParse(setting["WindowHeight"], out temp)) settings.WindowHeight = temp;
                    // 最小化のまま起動しないようにする（値は WinForms の FormWindowState と同じ）
                    settings.Maximized = int.TryParse(setting["WindowState"], out temp) && (WindowState)temp == WindowState.Maximized;
                    if (int.TryParse(setting["CanvasWidth"], out temp)) settings.CanvasWidth = Math.Max(1, temp);
                    if (int.TryParse(setting["CanvasHeight"], out temp)) settings.CanvasHeight = Math.Max(1, temp);
                    if (int.TryParse(setting["CanvasBackGround"], out temp))
                    {
                        settings.BackGroundMode = (BackGroundMode)temp;
                        try
                        {
                            settings.BackGroundColor = ColorTranslator.FromHtml(setting["CanvasBackGroundColor"]);
                        }
                        catch
                        {
                        }
                        settings.BackGroundImage = setting["CanvasBackGroundImage"];
                    }
                }
                if (File.Exists(recentUsedFileName))
                {
                    settings.RecentFiles.AddRange(File.ReadAllLines(recentUsedFileName).Where(File.Exists));
                }
            }
            catch
            {
            }
            return settings;
        }

        /// <summary>
        /// 設定を保存します（ファイルにあるほかの値は残す）。
        /// </summary>
        public void Save()
        {
            var setting = new SettingReader(File.Exists(iniFileName) ? File.ReadAllText(iniFileName) : "");
            setting.ReplaceOrAdd("Language", Language);
            if (WindowWidth.HasValue) setting.ReplaceOrAdd("WindowWidth", (int)WindowWidth.Value);
            if (WindowHeight.HasValue) setting.ReplaceOrAdd("WindowHeight", (int)WindowHeight.Value);
            setting.ReplaceOrAdd("WindowState", (int)(Maximized ? WindowState.Maximized : WindowState.Normal));
            setting.ReplaceOrAdd("CanvasWidth", CanvasWidth);
            setting.ReplaceOrAdd("CanvasHeight", CanvasHeight);
            setting.ReplaceOrAdd("CanvasBackGround", (int)BackGroundMode);
            setting.ReplaceOrAdd("CanvasBackGroundColor", ColorTranslator.ToHtml(BackGroundColor));
            setting.ReplaceOrAdd("CanvasBackGroundImage", BackGroundImage);
            using (SettingWriter sw = new(iniFileName, false))
            {
                foreach (KeyValuePair<string, string> kvp in setting.Dictionary)
                {
                    sw.Write(kvp.Key, kvp.Value);
                }
            }
            File.WriteAllLines(recentUsedFileName, RecentFiles);
        }
    }
}
