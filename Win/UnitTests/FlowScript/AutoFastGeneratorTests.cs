using FlowScriptEngine;
using FlowScriptEngine.AutoFastGenerator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace UnitTests.FlowScript
{
    /// <summary>
    /// AutoFastGenerator（ノードのプロパティとイベントを名前で使うコードを作る）のテストです。
    /// </summary>
    /// <remarks>
    /// リポジトリの .AutoFast.cs は、作ったあとで書き方（switch 式など）を整えているので、文字列ではなく
    /// 「名前 → メンバー」の対応で、今のノードから作ったものと同じかを比べる。
    /// </remarks>
    public class AutoFastGeneratorTests
    {
        static readonly string[] engines =
        [
            "FlowScriptEngineBasic", "FlowScriptEngineConsole", "FlowScriptEngineBasicExtension", "FlowScriptEngineData",
            "FlowScriptEngineSlimDX", "FlowScriptEnginePPD", "FlowScriptEnginePPDEditor",
        ];

        static string WinDirectory()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "FlowScriptEngineBasic")))
            {
                dir = dir.Parent;
            }
            return dir?.FullName;
        }

        /// <summary>
        /// コードの GetPropertyValue、SetPropertyValue、ConnectEvent の中の「名前 → メンバー」を読みます（switch 文と switch 式のどちらでも）。
        /// </summary>
        static SortedDictionary<string, string> ReadCases(string code)
        {
            var cases = new SortedDictionary<string, string>();
            string section = null;
            var pending = new List<string>();
            foreach (var raw in code.Split('\n'))
            {
                var line = raw.Trim();
                if (line.Contains("GetPropertyValue("))
                {
                    section = "get";
                }
                else if (line.Contains("SetPropertyValue("))
                {
                    section = "set";
                }
                else if (line.Contains("ConnectEvent("))
                {
                    section = "event";
                }
                var expression = Regex.Match(line, "^((?:\"\\w+\"(?:\\s+or\\s+)?)+)\\s*=>\\s*(\\w+),$");
                if (expression.Success)
                {
                    foreach (Match name in Regex.Matches(expression.Groups[1].Value, "\"(\\w+)\""))
                    {
                        cases.Add($"{section} {name.Groups[1].Value}", expression.Groups[2].Value);
                    }
                    continue;
                }
                var @case = Regex.Match(line, "^case \"(\\w+)\":$");
                if (@case.Success)
                {
                    pending.Add(@case.Groups[1].Value);
                    continue;
                }
                if (pending.Count > 0)
                {
                    var target = Regex.Match(line, "^(?:return\\s+(\\w+);|(\\w+)\\s*(?:=|\\+=)\\s)");
                    Assert.True(target.Success, $"読めない行: {line}");
                    foreach (var name in pending)
                    {
                        cases.Add($"{section} {name}", target.Groups[1].Success ? target.Groups[1].Value : target.Groups[2].Value);
                    }
                    pending.Clear();
                }
            }
            return cases;
        }

        [Fact]
        public void GeneratedCode_MatchesCheckedInFiles()
        {
            var win = WinDirectory();
            Assert.NotNull(win);
            // リポジトリの .AutoFast.cs を、名前空間とクラスの名前で引けるようにする
            var files = new Dictionary<string, string>();
            foreach (var engine in engines)
            {
                foreach (var path in Directory.EnumerateFiles(Path.Combine(win, engine), "*.AutoFast.cs", SearchOption.AllDirectories))
                {
                    var text = File.ReadAllText(path);
                    var ns = Regex.Match(text, "namespace ([\\w.]+)").Groups[1].Value;
                    var cls = Regex.Match(text, "partial class (\\w+)").Groups[1].Value;
                    files[ns + "." + cls] = path;
                }
            }

            var problems = new List<string>();
            var compared = 0;
            foreach (var type in NodeEventTests.NodeTypes())
            {
                var generated = ReadCases(Generator.GetFastCode((FlowSourceObjectBase)Activator.CreateInstance(type)));
                if (!files.TryGetValue(type.FullName, out var path))
                {
                    if (generated.Count > 0)
                    {
                        problems.Add($"{type.FullName}: .AutoFast.cs がない");
                    }
                    continue;
                }
                var checkedIn = ReadCases(File.ReadAllText(path));
                if (!generated.SequenceEqual(checkedIn))
                {
                    var missing = generated.Except(checkedIn).Select(p => $"{p.Key}->{p.Value}");
                    var extra = checkedIn.Except(generated).Select(p => $"{p.Key}->{p.Value}");
                    problems.Add($"{type.FullName}: ない [{string.Join(", ", missing)}] 余分 [{string.Join(", ", extra)}]");
                }
                compared++;
            }

            Assert.True(compared > 800, $"比べたノード: {compared}");
            Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems.Take(60)) + Environment.NewLine + $"（全部で {problems.Count} 件）");
        }

        [Fact]
        public void GetFastCode_WritesAllThreeMethods()
        {
            var code = Generator.GetFastCode(new FlowScriptEngineBasic.FlowSourceObjects.Repeat.ForeachFlowSourceObject()).ReplaceLineEndings("\n");

            Assert.StartsWith("//--------------------------------------------------------\n// This code is generated by AutoFastGenerator.\n", code);
            Assert.Contains("namespace FlowScriptEngineBasic.FlowSourceObjects.Repeat\n{\n    public partial class ForeachFlowSourceObject\n    {\n", code);
            // 古い名前（Replaced）も同じプロパティにつなぐ
            Assert.Contains("""
                            case "Enumerable":
                            case "ArrayList":
                                Enumerable = (System.Collections.Generic.IEnumerable<System.Object>)value;
                                break;
            """.ReplaceLineEndings("\n"), code);
            Assert.Contains("""
                            case "Loop":
                                Loop += new FlowScriptEngine.FlowEventHandler(eventHandler);
            """.ReplaceLineEndings("\n"), code);
            Assert.Contains("""
                            default:
                                return null;
            """.ReplaceLineEndings("\n"), code);
            Assert.EndsWith("    }\n}\n", code);
        }

        [Fact]
        public void GetFastCode_WritesGenericAndNestedTypeNames()
        {
            var code = Generator.GetFastCode(new FlowScriptEngineBasic.FlowSourceObjects.Hashtable.FromPairFlowSourceObject());

            Assert.Contains("A = (System.Collections.Generic.KeyValuePair<System.Object, System.Object>)value;", code);
        }

        [Fact]
        public void GetFastCode_NodeWithoutMembers_WritesOnlyClass()
        {
            var code = ReadCases(Generator.GetFastCode(new FlowScriptEngineBasic.FlowSourceObjects.Misc.EntryPointFlowSourceObject()));

            Assert.Equal(["event Start"], code.Keys);
        }
    }
}
