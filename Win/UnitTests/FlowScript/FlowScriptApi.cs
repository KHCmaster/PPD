using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace UnitTests.FlowScript
{
    /// <summary>
    /// FlowScript のノード（FlowSourceObjectBase を継承したクラス）の、スクリプト（.fsml）から使う形を書き出します。
    /// </summary>
    /// <remarks>
    /// .fsml にはクラスの完全な名前と、つなぐプロパティやイベントの名前が書かれるので、これらが変わると以前のスクリプトや Mod が動かなくなる。
    /// 以前に配った版の DLL（.NET Framework）も調べるので、読み込まずに MetadataLoadContext で調べる。
    /// 書き出す内容は、FlowSourceObjectBase がノードを調べるのと同じ（Ignore を付けたものは除く）。
    /// </remarks>
    public static class FlowScriptApi
    {
        public static readonly string[] NodeAssemblies =
        [
            "FlowScriptEngineBasic", "FlowScriptEngineBasicExtension", "FlowScriptEngineConsole", "FlowScriptEngineData",
            "FlowScriptEnginePPD", "FlowScriptEnginePPDEditor", "FlowScriptEngineSlimDX",
        ];

        /// <summary>
        /// フォルダーの DLL のノードを調べます。
        /// </summary>
        /// <param name="directories">ノードの DLL と、それが使う DLL のあるフォルダー（前にあるものを優先する）。</param>
        public static List<string> Extract(params string[] directories)
        {
            var paths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var directory in directories.Concat(RuntimeDirectories()))
            {
                foreach (var path in Directory.GetFiles(directory, "*.dll"))
                {
                    paths.TryAdd(Path.GetFileName(path), path);
                }
            }
            using var context = new MetadataLoadContext(new PathAssemblyResolver(paths.Values), "System.Private.CoreLib");
            var lines = new List<string>();
            foreach (var name in NodeAssemblies)
            {
                var assembly = context.LoadFromAssemblyPath(paths[name + ".dll"]);
                foreach (var type in GetTypes(assembly).Where(IsNode).OrderBy(t => t.FullName, StringComparer.Ordinal))
                {
                    lines.Add(type.FullName);
                    lines.AddRange(DescribeMembers(type).Select(m => "  " + m));
                }
            }
            return lines;
        }

        private static IEnumerable<string> RuntimeDirectories()
        {
            yield return RuntimeEnvironment.GetRuntimeDirectory();
            yield return Path.GetDirectoryName(typeof(System.Windows.Window).Assembly.Location);
            yield return Path.GetDirectoryName(typeof(System.Windows.Forms.Form).Assembly.Location);
        }

        private static IEnumerable<Type> GetTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }

        private static bool IsNode(Type type)
        {
            if (!type.IsClass || !type.IsPublic || type.IsAbstract)
            {
                return false;
            }
            for (var current = type.BaseType; current != null; current = current.BaseType)
            {
                if (current.FullName == "FlowScriptEngine.FlowSourceObjectBase")
                {
                    return true;
                }
            }
            return false;
        }

        private static IEnumerable<string> DescribeMembers(Type type)
        {
            var members = new List<string>();
            // MetadataLoadContext は get が公開でないプロパティを公開でないものとして扱うので、すべて取って set と get をそれぞれ調べる
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .GroupBy(p => p.Name).Select(g => g.First());
            foreach (var property in properties)
            {
                if (property.Name == "Name" || IsIgnored(property))
                {
                    continue;
                }
                var replaced = ReplacedNames(property);
                if (property.GetGetMethod() is { IsPublic: true })
                {
                    members.Add($"out {property.Name}: {FormatType(property.PropertyType)}{replaced}");
                }
                if (property.GetSetMethod() is { IsPublic: true })
                {
                    members.Add($"in {property.Name}: {FormatType(property.PropertyType)}{replaced}");
                }
            }
            foreach (var method in type.GetMethods())
            {
                if (method.IsPublic && !method.IsAbstract && method.ReturnType.FullName == "System.Void"
                    && method.GetParameters() is [{ ParameterType.FullName: "FlowScriptEngine.FlowEventArgs" }] && !IsIgnored(method))
                {
                    members.Add($"method {method.Name}{ReplacedNames(method)}");
                }
            }
            foreach (var @event in type.GetEvents())
            {
                if (@event.EventHandlerType.FullName == "FlowScriptEngine.FlowEventHandler"
                    && @event.GetAddMethod() is { IsPublic: true } && @event.GetRemoveMethod() is { IsPublic: true } && !IsIgnored(@event))
                {
                    members.Add($"event {@event.Name}{ReplacedNames(@event)}");
                }
            }
            return members.Distinct().OrderBy(m => m, StringComparer.Ordinal);
        }

        // 属性は基のクラスのものも見る（FlowSourceObjectBase と同じく、継承した属性も使う）
        private static IEnumerable<CustomAttributeData> Attributes(MemberInfo member)
        {
            for (var type = member.DeclaringType; type != null; type = type.BaseType)
            {
                MemberInfo declared = member switch
                {
                    PropertyInfo => type.GetProperty(member.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                    EventInfo => type.GetEvent(member.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                    _ => member.DeclaringType == type ? member : null,
                };
                if (declared == null)
                {
                    continue;
                }
                foreach (var attribute in declared.GetCustomAttributesData())
                {
                    yield return attribute;
                }
            }
        }

        private static bool IsIgnored(MemberInfo member)
        {
            return Attributes(member).Any(a => a.AttributeType.FullName == "FlowScriptEngine.IgnoreAttribute");
        }

        private static string ReplacedNames(MemberInfo member)
        {
            var names = Attributes(member).Where(a => a.AttributeType.FullName == "FlowScriptEngine.ReplacedAttribute")
                .Select(a => (string)a.ConstructorArguments[0].Value).Distinct().Order(StringComparer.Ordinal).ToArray();
            return names.Length == 0 ? "" : " (replaced: " + string.Join(", ", names) + ")";
        }

        public static string FormatType(Type type)
        {
            if (type.IsArray)
            {
                return FormatType(type.GetElementType()) + "[" + new string(',', type.GetArrayRank() - 1) + "]";
            }
            if (type.IsGenericParameter)
            {
                return type.Name;
            }
            var name = type.IsNested ? FormatType(type.DeclaringType) + "+" + type.Name : type.Namespace + "." + type.Name;
            if (type.IsGenericType)
            {
                var tick = name.LastIndexOf('`');
                if (tick >= 0)
                {
                    name = name[..tick];
                }
                name += "<" + string.Join(", ", type.GetGenericArguments().Select(FormatType)) + ">";
            }
            return name;
        }
    }
}
