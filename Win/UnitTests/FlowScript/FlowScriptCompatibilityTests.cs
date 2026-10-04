using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace UnitTests.FlowScript
{
    /// <summary>
    /// FlowScript のノードが、以前に配った版（1.2.0.1）と同じ形でスクリプトから使えるかを確かめます（Mod の互換性）。
    /// </summary>
    /// <remarks>
    /// 以前の版の形は FlowScriptApi_1.2.0.1.txt に書き出してある（配った DLL は release フォルダーにあるが、git には入れていない）。
    /// ノードやプロパティを足すのはかまわないが、なくしたり名前や型を変えたりすると、以前のスクリプトや Mod が動かなくなる。
    /// </remarks>
    public class FlowScriptCompatibilityTests
    {
        const string SnapshotName = "FlowScriptApi_1.2.0.1.txt";

        static string SnapshotPath
        {
            get { return Path.Combine(AppContext.BaseDirectory, "FlowScript", SnapshotName); }
        }

        // SharpDX から System.Numerics などに移した型（文字列での値の書き方は同じにしてある）
        static readonly Dictionary<string, string> movedTypes = new()
        {
            ["SharpDX.Vector2"] = "System.Numerics.Vector2",
            ["SharpDX.Vector3"] = "System.Numerics.Vector3",
            ["SharpDX.Vector4"] = "System.Numerics.Vector4",
            ["SharpDX.Matrix"] = "System.Numerics.Matrix4x4",
            ["SharpDX.Quaternion"] = "System.Numerics.Quaternion",
            ["SharpDX.Color4"] = "Vortice.Mathematics.Color4",
        };

        static string MapTypes(string line)
        {
            foreach (var pair in movedTypes)
            {
                line = line.Replace(pair.Key, pair.Value);
            }
            return line;
        }

        [Fact]
        public void CurrentNodes_KeepEverythingFromReleasedVersion()
        {
            var released = File.ReadAllLines(SnapshotPath).Select(MapTypes).ToList();
            var current = FlowScriptApi.Extract(AppContext.BaseDirectory);

            var missing = Missing(released, current);

            Assert.True(missing.Count == 0, "以前の版にあって、今の版にないもの:" + Environment.NewLine + string.Join(Environment.NewLine, missing));
        }

        // ノードごとに、以前のメンバーが今もあるかを調べる（足したものは問わない）
        static List<string> Missing(List<string> released, List<string> current)
        {
            var currentNodes = Group(current);
            var missing = new List<string>();
            foreach (var (node, members) in Group(released))
            {
                if (!currentNodes.TryGetValue(node, out var currentMembers))
                {
                    missing.Add(node);
                    continue;
                }
                missing.AddRange(members.Except(currentMembers).Select(m => node + " " + m.Trim()));
            }
            return missing;
        }

        static Dictionary<string, List<string>> Group(List<string> lines)
        {
            var nodes = new Dictionary<string, List<string>>();
            List<string> members = null;
            foreach (var line in lines)
            {
                if (line.StartsWith("  "))
                {
                    members.Add(line);
                }
                else
                {
                    members = [];
                    nodes[line] = members;
                }
            }
            return nodes;
        }

        /// <summary>
        /// 書き出した以前の版の形が、配った DLL と合っているかを確かめます（release フォルダーがあるときだけ）。
        /// </summary>
        [Fact]
        public void Snapshot_MatchesReleasedDlls()
        {
            var ppd = ReleasedPPDDirectory();
            if (ppd == null)
            {
                return;
            }

            var extracted = FlowScriptApi.Extract(Path.Combine(ppd, "dlls"), ppd);

            if (!File.Exists(SnapshotPath) || !File.ReadAllLines(SnapshotPath).SequenceEqual(extracted))
            {
                var output = Path.Combine(Path.GetTempPath(), SnapshotName);
                File.WriteAllLines(output, extracted);
                Assert.Fail($"{SnapshotName} が配った DLL と違います。書き出したものを確かめて置き換えてください: {output}");
            }
        }

        static string ReleasedPPDDirectory()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "release", "releases")))
            {
                dir = dir.Parent;
            }
            if (dir == null)
            {
                return null;
            }
            var ppd = Path.Combine(dir.FullName, "release", "releases", "1.2.0.1", "PPDInstaller(x64)", "Data", "PPD");
            return Directory.Exists(ppd) ? ppd : null;
        }
    }
}
