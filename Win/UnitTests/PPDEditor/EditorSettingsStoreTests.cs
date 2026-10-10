using PPDConfiguration;
using PPDEditor;
using PPDEditor.Models;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Xunit;
using Keys = System.Windows.Forms.Keys;
using Utility = PPDEditor.Utility;

namespace UnitTests.PPDEditor
{
    /// <summary>
    /// エディターの設定ファイル（ショートカット、最近のプロジェクト、ウィンドウの設定）の読み書きのテストです。
    /// </summary>
    /// <remarks>
    /// 設定はエディターのフォルダー（Utility.AppDir）に置くので、テストの間は一時フォルダーにする。
    /// </remarks>
    public sealed class EditorSettingsStoreTests : IDisposable
    {
        static readonly FieldInfo appDirField = typeof(Utility).GetField("appDir", BindingFlags.Static | BindingFlags.NonPublic);
        readonly TempFolder folder = new();
        readonly object savedAppDir;

        public EditorSettingsStoreTests()
        {
            savedAppDir = appDirField.GetValue(null);
            appDirField.SetValue(null, folder.Path);
        }

        public void Dispose()
        {
            appDirField.SetValue(null, savedAppDir);
            folder.Dispose();
        }

        [Fact]
        public void Shortcuts_DefaultsAndRoundTrip()
        {
            var manager = new ShortcutManager();
            manager.RegisterShortcut(new ShortcutInfo(Keys.X, false, false, false, ShortcutType.Undo));

            // ファイルがなければ、決まったショートカット（前のものは消す）
            EditorSettingsStore.LoadShortcuts(manager);
            Assert.Equal(25, manager.Shortcuts.Length);
            Assert.Equal(ShortcutType.Undo, manager.GetShortcut(Keys.Z, true, false, false).ShortcutType);
            Assert.Null(manager.GetShortcut(Keys.X, false, false, false));

            manager.ClearShortcut();
            manager.RegisterShortcut(new ShortcutInfo(Keys.F5, false, true, true, ShortcutType.Redo));
            manager.RegisterShortcut(new ShortcutInfo(Keys.F6, true, false, false, "script.fsml"));
            EditorSettingsStore.SaveShortcuts(manager);
            // 読めない行は飛ばす
            File.AppendAllText(folder.Combine("PPDEditor_keybind.ini"), "broken" + Environment.NewLine);

            var loaded = new ShortcutManager();
            EditorSettingsStore.LoadShortcuts(loaded);
            Assert.Equal(2, loaded.Shortcuts.Length);
            Assert.Equal(ShortcutType.Redo, loaded.GetShortcut(Keys.F5, true, false, true).ShortcutType);
            Assert.Equal("script.fsml", loaded.GetShortcut(Keys.F6, false, true, false).ScriptPath);
        }

        [Fact]
        public void RecentProjects_SkipMissingFiles()
        {
            Assert.Empty(EditorSettingsStore.LoadRecentProjects());
            var project = folder.Write("song.ppdproj", "");

            EditorSettingsStore.SaveRecentProjects([project, folder.Combine("deleted.ppdproj")]);

            Assert.Equal([project], EditorSettingsStore.LoadRecentProjects());
        }

        [Fact]
        public void SaveConfig_KeepsOtherSettings()
        {
            var path = folder.Write(EditorSettingsStore.IniFileName, "[formattype]\nOther=1\n");

            EditorSettingsStore.SaveConfig(new Rect(10, 20, 640, 480), true);

            var setting = new SettingReader(File.ReadAllText(path));
            Assert.Equal("1", setting.ReadString("Other"));
            Assert.Equal("640,480", setting.ReadString("Size"));
            Assert.Equal("10,20", setting.ReadString("Location"));
            Assert.Equal("1", setting.ReadString("Maximised"));
            Assert.Equal(PPDStaticSetting.Moves[3].ToString(), setting.ReadString("Move3"));
            Assert.All(Enumerable.Range(0, 8), i => Assert.Equal(PPDStaticSetting.Angles[i].ToString(), setting.ReadString("Angle" + i)));
        }
    }
}
