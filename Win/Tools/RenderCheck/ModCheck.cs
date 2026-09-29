using FlowScriptEngine;
using PPDFramework.Mod;
using PPDPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

/// <summary>
/// Mod と FlowScript の互換性を確認します。
/// PPD の実行フォルダ（dlls\ と mods\ があるフォルダ）を作業フォルダにして実行します。
///
/// 使い方: ModCheck.exe <PPDの実行フォルダ> <結果ファイル> [追加の .fsml を探すフォルダ ...]
///
/// 各 Mod について、PPD の ModInfo と同じ手順（Engine.Load → Initialize → MODSTART の呼び出し）で
/// スクリプトを読み込み、失敗したものを報告します。
/// あわせて、各ノードの既定値（.fsml の Value）がどの値に変換されたかを結果ファイルに書き出します。
/// フェーズの前後で結果ファイルを比較し、差分がないことを確認します。
///
/// 終了コード: 0 = 全て読み込めた, 1 = 読み込めないものがあった
/// </summary>
static class ModCheck
{
    static string ppdDir;

    static int Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("usage: ModCheck.exe <ppdDir> <resultFile> [scriptDir ...]");
            return 2;
        }
        ppdDir = Path.GetFullPath(args[0]);
        var resultFile = Path.GetFullPath(args[1]);
        var scriptDirs = args.Skip(2).Select(Path.GetFullPath).ToArray();
        AppDomain.CurrentDomain.AssemblyResolve += ResolveFromPPDDir;
        Environment.CurrentDirectory = ppdDir;
        try
        {
            return Run(resultFile, scriptDirs);
        }
        catch (Exception e)
        {
            Console.WriteLine("error: " + Describe(e));
            Console.WriteLine(e.StackTrace);
            return 2;
        }
    }

    static Assembly ResolveFromPPDDir(object sender, ResolveEventArgs e)
    {
        var name = new AssemblyName(e.Name).Name;
        foreach (var dir in new[] { ppdDir, Path.Combine(ppdDir, "dlls") })
        {
            var path = Path.Combine(dir, name + ".dll");
            if (File.Exists(path))
            {
                return Assembly.LoadFrom(path);
            }
        }
        return null;
    }

    static int Run(string resultFile, string[] scriptDirs)
    {
        ModManager.Instance.EnumerateClasses();
        var output = new StringBuilder();
        var failures = 0;

        var modDir = Path.Combine(ppdDir, "mods");
        var modFiles = Directory.Exists(modDir) ? Directory.GetFiles(modDir, "*.mod", SearchOption.AllDirectories).OrderBy(f => f, StringComparer.OrdinalIgnoreCase).ToArray() : new string[0];
        foreach (var modFile in modFiles)
        {
            var name = modFile.Substring(modDir.Length + 1);
            try
            {
                var scripts = new List<KeyValuePair<string, byte[]>>();
                using (var reader = new PackReader(modFile))
                {
                    foreach (var content in reader.FileList)
                    {
                        if (!content.StartsWith("Scripts"))
                        {
                            continue;
                        }
                        using (var stream = reader.Read(content))
                        using (var memory = new MemoryStream())
                        {
                            stream.CopyTo(memory);
                            scripts.Add(new KeyValuePair<string, byte[]>(content, memory.ToArray()));
                        }
                    }
                }
                LoadLikeModInfo(scripts);
                output.AppendLine(string.Format("[MOD] {0}: OK ({1} scripts)", name, scripts.Count));
                foreach (var script in scripts)
                {
                    DumpDefaultValues(output, name + "/" + script.Key, script.Value);
                }
            }
            catch (Exception e)
            {
                failures++;
                output.AppendLine(string.Format("[MOD] {0}: FAIL {1}", name, Describe(e)));
            }
        }

        foreach (var dir in scriptDirs)
        {
            if (!Directory.Exists(dir))
            {
                output.AppendLine("[DIR] " + dir + ": not found");
                continue;
            }
            foreach (var file in Directory.GetFiles(dir, "*.fsml", SearchOption.AllDirectories).OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
            {
                var bytes = File.ReadAllBytes(file);
                var name = file.Substring(dir.Length).TrimStart('\\');
                try
                {
                    using (var stream = new MemoryStream(bytes))
                    {
                        new Engine().Load(stream, false);
                    }
                    output.AppendLine("[FSML] " + name + ": OK");
                    DumpDefaultValues(output, name, bytes);
                }
                catch (Exception e)
                {
                    failures++;
                    output.AppendLine(string.Format("[FSML] {0}: FAIL {1}", name, Describe(e)));
                }
            }
        }

        File.WriteAllText(resultFile, output.ToString(), new UTF8Encoding(true));
        Console.WriteLine("mods: {0}, failures: {1}", modFiles.Length, failures);
        return failures == 0 ? 0 : 1;
    }

    /// <summary>
    /// PPDFramework.Mod.ModInfo.AnalyzeMod と同じ手順で読み込みます。
    /// </summary>
    static void LoadLikeModInfo(List<KeyValuePair<string, byte[]>> scripts)
    {
        var engine = new Engine();
        var modSettingManager = new ModSettingManager();
        foreach (var script in scripts)
        {
            var manager = engine.Load(new MemoryStream(script.Value), false);
            manager.Items["ModSettingManager"] = modSettingManager;
            manager.Initialize();
        }
        engine.Call(ModSettingManager.MODSTART, null);
    }

    /// <summary>
    /// 各ノードの既定値が、どの型のどの値に変換されるかを書き出します。
    /// </summary>
    static void DumpDefaultValues(StringBuilder output, string scriptName, byte[] bytes)
    {
        XDocument document;
        using (var stream = new MemoryStream(bytes))
        {
            document = XDocument.Load(stream);
        }
        foreach (var source in document.Descendants("Source"))
        {
            var fullName = (string)source.Attribute("FullName");
            var id = (string)source.Attribute("ID");
            var sourceObject = FlowSourceObjectManager.CreateSource(fullName);
            foreach (var value in source.Elements("Value"))
            {
                var propertyName = (string)value.Attribute("Name");
                var text = (string)value.Attribute("Value");
                var property = sourceObject == null ? null : sourceObject.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                string result;
                if (property == null)
                {
                    result = "(no property)";
                }
                else
                {
                    var formatter = TypeFormatterManager.GetFormatter(property.PropertyType);
                    object converted;
                    result = formatter != null && formatter.Format(text, out converted) ? DescribeValue(converted, 0) : "(format failed)";
                }
                output.AppendLine(string.Format("  {0} #{1} {2}.{3} = \"{4}\" -> {5}", scriptName, id, fullName, propertyName, text, result));
            }
        }
    }

    static string DescribeValue(object value, int depth)
    {
        if (value == null)
        {
            return "null";
        }
        var type = value.GetType();
        if (value is float f) return f.ToString("R", CultureInfo.InvariantCulture);
        if (value is double d) return d.ToString("R", CultureInfo.InvariantCulture);
        if (type.IsPrimitive || value is string || value is decimal || type.IsEnum)
        {
            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }
        if (depth >= 2)
        {
            return type.Name;
        }
        if (value is IEnumerable enumerable)
        {
            return "[" + string.Join(", ", enumerable.Cast<object>().Take(32).Select(v => DescribeValue(v, depth + 1))) + "]";
        }
        // 型名は名前空間を除いて出す（SharpDX → System.Numerics などの移行で差分が出ないように）
        var members = type.GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Select(m => new KeyValuePair<string, object>(m.Name, m.GetValue(value)))
            .Concat(type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0 && p.PropertyType != type)
                .Select(p => new KeyValuePair<string, object>(p.Name, SafeGet(p, value))))
            .OrderBy(m => m.Key, StringComparer.Ordinal)
            .Select(m => m.Key + "=" + DescribeValue(m.Value, depth + 1));
        return type.Name + "{" + string.Join(", ", members) + "}";
    }

    static object SafeGet(PropertyInfo property, object target)
    {
        try
        {
            return property.GetValue(target, null);
        }
        catch (Exception e)
        {
            return "(" + e.GetType().Name + ")";
        }
    }

    static string Describe(Exception e)
    {
        var messages = new List<string>();
        while (e != null)
        {
            messages.Add(e.GetType().Name + ": " + e.Message);
            e = e.InnerException;
        }
        return string.Join(" <- ", messages);
    }
}
