using PPDConfiguration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Keys = System.Windows.Forms.Keys;

namespace PPDEditor.Models
{
    /// <summary>
    /// エディターの設定（PPDEditor.ini）、ショートカット、最近使ったプロジェクトを読み書きします。
    /// </summary>
    public static class EditorSettingsStore
    {
        public const string IniFileName = "PPDEditor.ini";
        const string shortcutFileName = "PPDEditor_keybind.ini";
        const string projectcache = "projectcache.dat";

        /// <summary>
        /// ショートカットを読み込みます。ファイルがないか読めないときは、既定のショートカットにします。
        /// </summary>
        public static void LoadShortcuts(ShortcutManager manager)
        {
            manager.ClearShortcut();
            try
            {
                var path = Path.Combine(Utility.AppDir, shortcutFileName);
                if (File.Exists(path))
                {
                    foreach (string line in File.ReadAllLines(path))
                    {
                        var info = ShortcutInfo.Deserialize(line);
                        if (info != null)
                        {
                            manager.RegisterShortcut(info);
                        }
                    }
                    return;
                }
            }
            catch
            {
            }

            manager.RegisterShortcut(new ShortcutInfo(Keys.Left, false, false, false, ShortcutType.Left0));
            manager.RegisterShortcut(new ShortcutInfo(Keys.Right, false, false, false, ShortcutType.Right0));
            manager.RegisterShortcut(new ShortcutInfo(Keys.Down, false, false, false, ShortcutType.Down0));
            manager.RegisterShortcut(new ShortcutInfo(Keys.Up, false, false, false, ShortcutType.Up0));
            manager.RegisterShortcut(new ShortcutInfo(Keys.Left, true, false, false, ShortcutType.Left1));
            manager.RegisterShortcut(new ShortcutInfo(Keys.Right, true, false, false, ShortcutType.Right1));
            manager.RegisterShortcut(new ShortcutInfo(Keys.Down, true, false, false, ShortcutType.Down1));
            manager.RegisterShortcut(new ShortcutInfo(Keys.Up, true, false, false, ShortcutType.Up1));
            manager.RegisterShortcut(new ShortcutInfo(Keys.Left, false, true, false, ShortcutType.Left2));
            manager.RegisterShortcut(new ShortcutInfo(Keys.Right, false, true, false, ShortcutType.Right2));
            manager.RegisterShortcut(new ShortcutInfo(Keys.Down, false, true, false, ShortcutType.Down2));
            manager.RegisterShortcut(new ShortcutInfo(Keys.Up, false, true, false, ShortcutType.Up2));
            manager.RegisterShortcut(new ShortcutInfo(Keys.Delete, false, false, false, ShortcutType.DeleteMark));
            manager.RegisterShortcut(new ShortcutInfo(Keys.Tab, false, false, false, ShortcutType.Next));
            manager.RegisterShortcut(new ShortcutInfo(Keys.Tab, true, false, false, ShortcutType.Previous));
            manager.RegisterShortcut(new ShortcutInfo(Keys.A, false, true, false, ShortcutType.PreviousAll));
            manager.RegisterShortcut(new ShortcutInfo(Keys.D, false, true, false, ShortcutType.NextAll));
            manager.RegisterShortcut(new ShortcutInfo(Keys.R, false, true, false, ShortcutType.CopyAngle));
            manager.RegisterShortcut(new ShortcutInfo(Keys.Q, false, true, false, ShortcutType.CopyPosition));
            manager.RegisterShortcut(new ShortcutInfo(Keys.F, false, true, false, ShortcutType.Fusion));
            manager.RegisterShortcut(new ShortcutInfo(Keys.F, true, true, false, ShortcutType.Defusion));
            manager.RegisterShortcut(new ShortcutInfo(Keys.Z, false, true, false, ShortcutType.Undo));
            manager.RegisterShortcut(new ShortcutInfo(Keys.Y, false, true, false, ShortcutType.Redo));
            manager.RegisterShortcut(new ShortcutInfo(Keys.C, false, true, false, ShortcutType.CopyMark));
            manager.RegisterShortcut(new ShortcutInfo(Keys.C, true, true, false, ShortcutType.ClearCopyBuffer));
        }

        public static void SaveShortcuts(ShortcutManager manager)
        {
            using StreamWriter writer = new(Path.Combine(Utility.AppDir, shortcutFileName));
            foreach (ShortcutInfo info in manager.Shortcuts)
            {
                writer.WriteLine(info.Serialize());
            }
        }

        /// <summary>
        /// 最近使ったプロジェクトを、新しい順に返します（なくなったファイルは除く）。
        /// </summary>
        public static string[] LoadRecentProjects()
        {
            var path = Path.Combine(Utility.AppDir, projectcache);
            if (!File.Exists(path))
            {
                return [];
            }
            return [.. File.ReadAllLines(path).Where(File.Exists)];
        }

        public static void SaveRecentProjects(IEnumerable<string> paths)
        {
            File.WriteAllLines(Path.Combine(Utility.AppDir, projectcache), paths);
        }

        /// <summary>
        /// エディターの設定を PPDEditor.ini に書きます。
        /// </summary>
        /// <param name="bounds">ウィンドウの位置と大きさ（最大化しているときは元の大きさ）。</param>
        /// <param name="maximized">最大化しているかどうか。</param>
        public static void SaveConfig(Rect bounds, bool maximized)
        {
            var path = Path.Combine(Utility.AppDir, IniFileName);
            var setting = new SettingReader(File.ReadAllText(path));
            setting.ReplaceOrAdd("Language", PPDStaticSetting.langFileISO);
            setting.ReplaceOrAdd("Maximised", maximized);
            setting.ReplaceOrAdd("Size", String.Format("{0},{1}", (int)bounds.Width, (int)bounds.Height));
            setting.ReplaceOrAdd("Location", String.Format("{0},{1}", (int)bounds.X, (int)bounds.Y));
            setting.ReplaceOrAdd("AuthorName", PPDStaticSetting.AuthorName);
            setting.ReplaceOrAdd("MovieLatency", PPDStaticSetting.MovieLatency);
            setting.ReplaceOrAdd("HideToggleRectangle", PPDStaticSetting.HideToggleRectangle);
            setting.ReplaceOrAdd("EnableToChangeMarkTypeAndTime", PPDStaticSetting.EnableToChangeMarkTypeAndTime);
            setting.ReplaceOrAdd("HideToggleArrow", PPDStaticSetting.HideToggleArrow);
            setting.ReplaceOrAdd("FixDockPanel", PPDStaticSetting.FixDockPanel);
            setting.ReplaceOrAdd("MemoFontSize", PPDStaticSetting.MemoFontSize);
            setting.ReplaceOrAdd("ApplyToIniMovieTrimming", PPDStaticSetting.ApplyToIniMovieTrimming);
            setting.ReplaceOrAdd("CustomCanvasBackColor", String.Format("{0},{1},{2}",
                PPDStaticSetting.CustomCanvasBackColor.R,
                PPDStaticSetting.CustomCanvasBackColor.G,
                PPDStaticSetting.CustomCanvasBackColor.B
                ));
            setting.ReplaceOrAdd("CanvasSizeIndex", PPDStaticSetting.CanvasSizeIndex);
            setting.ReplaceOrAdd("CanvasColorIndex", PPDStaticSetting.CanvasColorIndex);
            for (int i = 0; i < 8; i++)
            {
                setting.ReplaceOrAdd(String.Format("Move{0}", i), PPDStaticSetting.Moves[i]);
                setting.ReplaceOrAdd(String.Format("Angle{0}", i), PPDStaticSetting.Angles[i]);
            }
            using SettingWriter sw = new(path, false);
            foreach (KeyValuePair<string, string> kvp in setting.Dictionary)
            {
                sw.Write(kvp.Key, kvp.Value);
            }
        }
    }
}
