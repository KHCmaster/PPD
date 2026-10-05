using FlowScriptEngine;
using PPDFramework.Mod;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnitTests.FlowScript;
using Xunit;

namespace UnitTests.Libraries
{
    /// <summary>
    /// Mod のファイル（PPDPack の中に情報とスクリプト）を読む ModInfo のテストです。
    /// </summary>
    /// <remarks>
    /// 設定は PPDFramework.dll の隣の ModSetting.setting（テストの出力フォルダー）に入る。Mod の名前は毎回変える。
    /// ModManager の読み込み（Web から Mod の一覧を取る）は使わない。
    /// </remarks>
    public sealed class ModInfoTests : IDisposable
    {
        readonly string dir = Directory.CreateTempSubdirectory("PPDModTest").FullName;
        readonly string name = "Test" + Guid.NewGuid().ToString("N");

        public void Dispose()
        {
            Directory.Delete(dir, true);
        }

        // 設定を足すスクリプト（Mod を始めたときに動く）
        static string SettingScript(bool modifyData)
        {
            ScriptRunner.RegisterNodes();
            _ = FlowSourceEnumerator.EnumerateFromFile(typeof(FlowScriptEnginePPD.ToolTipText).Assembly.Location, [typeof(FlowSourceObjectBase)]).ToList();
            var s = new ScriptRunner();
            var entry = s.Node(typeof(FlowScriptEnginePPD.FlowSourceObjects.Mod.EntryPointFlowSourceObject));
            var seq = s.Node("Logic.Sequential");
            var speed = s.Node(typeof(FlowScriptEnginePPD.FlowSourceObjects.Mod.AddInt32SettingWithLimitFlowSourceObject),
                ("Key", "speed"), ("DisplayName", "速さ"), ("Default", "3"), ("Min", "1"), ("Max", "5"));
            var again = s.Node(typeof(FlowScriptEnginePPD.FlowSourceObjects.Mod.AddInt32SettingFlowSourceObject), ("Key", "speed"), ("Default", "9"));
            var color = s.Node(typeof(FlowScriptEnginePPD.FlowSourceObjects.Mod.AddBooleanSettingFlowSourceObject), ("Key", "on"), ("Default", "True"));
            s.Flow(entry, "Start", seq, "In").Flow(seq, "Out0", speed, "In").Flow(seq, "Out1", again, "In").Flow(seq, "Out2", color, "In");
            if (modifyData)
            {
                // データを変えるノード（ModifyData）があると、記録に残らない Mod になる
                s.Node(typeof(FlowScriptEnginePPD.FlowSourceObjects.Mark.DynamicCreateFlowSourceObject));
            }
            return s.ToXml();
        }

        string WriteMod(string fileName, string version = "0.0.0.1", bool modifyData = false)
        {
            var files = new Dictionary<string, byte[]>
            {
                [@"Mod\DisplayName"] = Encoding.UTF8.GetBytes(name),
                [@"Mod\AuthorName"] = Encoding.UTF8.GetBytes("作者"),
                [@"Mod\Version"] = Encoding.UTF8.GetBytes("1.2"),
                [@"Mod\FlowScriptVersion"] = Encoding.UTF8.GetBytes(version),
                [@"Scripts\main.fsml"] = Encoding.UTF8.GetBytes(SettingScript(modifyData)),
            };
            var path = Path.Combine(dir, fileName);
            using var writer = new PPDPack.PackWriter(path);
            foreach (var stream in writer.Write([.. files.Keys]))
            {
                stream.Write(files[stream.Filename], 0, files[stream.Filename].Length);
            }
            return path;
        }

        static ModInfo Load(string path)
        {
            var constructor = typeof(ModInfo).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)[0];
            return (ModInfo)constructor.Invoke([path, ModManager.Instance, Array.Empty<PPDFramework.Web.WebModInfo>()]);
        }

        [Fact]
        public void Load_ReadsInfoAndSettings()
        {
            var mod = Load(WriteMod("test.mod"));

            Assert.Equal(name, mod.DisplayName);
            Assert.Equal("作者", mod.AuthorName);
            Assert.Equal("1.2", mod.Version);
            Assert.Equal($"作者.{name}", mod.UniqueName);
            Assert.Equal("test.mod", mod.FileName);
            Assert.Equal(new Version(0, 0, 0, 1), mod.FlowScriptVersion);
            // 書いていないバージョンは 0
            Assert.Equal(new Version(0, 0, 0, 0), mod.FlowScriptPPDVersion);
            Assert.Equal(32, mod.FileHash.Length);
            Assert.Equal(64, mod.FileHashString.Length);
            Assert.False(mod.ContainsModifyData);
            Assert.False(mod.CanUpdate);
            Assert.False(mod.IsDir);
            Assert.NotEqual(Guid.Empty, mod.Guid);

            // 同じキーの設定は最初のものを使う
            Assert.Equal(["speed", "on"], mod.Settings.Select(s => s.Key));
            Assert.Equal(3, mod.ModSettingManager["speed"]);
            Assert.Equal(true, mod.ModSettingManager["on"]);
        }

        [Fact]
        public void ModifyDataNodes_AreDetected()
        {
            Assert.True(Load(WriteMod("modify.mod", modifyData: true)).ContainsModifyData);
        }

        [Fact]
        public void Settings_AreSavedAndValidated()
        {
            var path = WriteMod("setting.mod");
            var mod = Load(path);

            mod.ModSettingManager["speed"] = 4;
            mod.SaveSetting();
            Assert.Equal(4, Load(path).ModSettingManager["speed"]);

            // 範囲の外の値が残っていたら、既定の値に戻す
            mod.ModSettingManager["speed"] = 99;
            mod.SaveSetting();
            Assert.Equal(3, Load(path).ModSettingManager["speed"]);
        }

        [Fact]
        public void IsApplied_IsRemembered()
        {
            var path = WriteMod("applied.mod");
            var mod = Load(path);
            Assert.False(mod.IsApplied);

            mod.IsApplied = true;

            Assert.True(Load(path).IsApplied);
            mod.IsApplied = false;
            Assert.False(Load(path).IsApplied);
        }

        [Fact]
        public void NotModified_ComparesFileHash()
        {
            var path = WriteMod("hash.mod");
            var mod = Load(path);
            Assert.True(mod.NotModified());

            File.AppendAllText(path, "x");

            Assert.False(mod.NotModified());
        }

        [Fact]
        public void CanApply_ComparesFlowScriptVersions()
        {
            // ModManager を読み込んでいないときは、使える FlowScript のバージョンがわからないので使えない
            Assert.False(Load(WriteMod("new.mod", version: "99.0.0.0")).CanApply());
        }

        [Fact]
        public void ModTree()
        {
            var root = new DirModInfo(Path.Combine(dir, "mods"));
            var sub = new DirModInfo(Path.Combine(dir, "mods", "sub"));
            var mod = Load(WriteMod("tree.mod"));
            root.AddChild(sub);
            sub.AddChild(mod);

            Assert.Equal("mods", root.DisplayName);
            Assert.True(root.IsDir);
            Assert.Same(root, sub.Parent);
            Assert.Equal([sub, mod], root.Descendants());
            Assert.Equal([sub], root.Children);
        }
    }
}
