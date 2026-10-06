using PPDEditor.Models;
using PPDFramework.Mod;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnitTests.FlowScript;
using Xunit;

namespace UnitTests.PPDEditor
{
    /// <summary>
    /// エディターで Mod を発行する（ModPackage）テストです。作った Mod を、ゲームの ModInfo で読み直して確かめる。
    /// </summary>
    /// <remarks>
    /// Mod には FlowScript の dll の版を書く。dll は今のフォルダーから読むので、一時フォルダーに写しておく。
    /// </remarks>
    public class ModPackageTests
    {
        static readonly string[] engineDlls = ["FlowScriptEngineBasic", "FlowScriptEnginePPD", "FlowScriptEngineSlimDX", "FlowScriptEngineBasicExtension", "FlowScriptEngineData"];

        [Fact]
        public void Write_IsReadByGame()
        {
            using var folder = new TempFolder(setCurrentDirectory: true);
            File.Copy(Path.Combine(AppContext.BaseDirectory, "FlowScriptEngine.dll"), "FlowScriptEngine.dll");
            Directory.CreateDirectory("dlls");
            foreach (var dll in engineDlls)
            {
                File.Copy(Path.Combine(AppContext.BaseDirectory, dll + ".dll"), Path.Combine("dlls", dll + ".dll"));
            }
            ScriptRunner.RegisterNodes();
            _ = FlowScriptEngine.FlowSourceEnumerator.EnumerateFromFile(typeof(FlowScriptEnginePPD.ToolTipText).Assembly.Location, [typeof(FlowScriptEngine.FlowSourceObjectBase)]).ToList();
            var s = new ScriptRunner();
            s.Node(typeof(FlowScriptEnginePPD.FlowSourceObjects.Mod.EntryPointFlowSourceObject));
            var script = Encoding.UTF8.GetBytes(s.ToXml());
            var name = "Test" + Guid.NewGuid().ToString("N");
            var path = folder.Combine("test.mod");
            var written = new List<string>();

            ModPackage.Write(path, name, "作者", "1.5", [@"Scripts\main.fsml"], [@"Resource\image.png"],
                (stream, file) =>
                {
                    written.Add(file);
                    stream.Write(script, 0, script.Length);
                },
                (stream, file) =>
                {
                    written.Add(file);
                    stream.Write([1, 2, 3], 0, 3);
                });

            Assert.Equal([@"Scripts\main.fsml", @"Resource\image.png"], written);
            var constructor = typeof(ModInfo).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)[0];
            var mod = (ModInfo)constructor.Invoke([path, ModManager.Instance, Array.Empty<PPDFramework.Web.WebModInfo>()]);
            Assert.Equal((name, "作者", "1.5"), (mod.DisplayName, mod.AuthorName, mod.Version));
            // FlowScript の版は、写した dll の版
            Assert.Equal(new Version(FileVersionInfo.GetVersionInfo("FlowScriptEngine.dll").FileVersion), mod.FlowScriptVersion);
            Assert.Equal(new Version(FileVersionInfo.GetVersionInfo(Path.Combine("dlls", "FlowScriptEnginePPD.dll")).FileVersion), mod.FlowScriptPPDVersion);
            Assert.False(mod.ContainsModifyData);
        }
    }
}
