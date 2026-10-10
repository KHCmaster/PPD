using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

/// <summary>
/// マルチプレイの通信データ（PPDMultiCommon の [MessagePackObject] 型）を MessagePack でシリアライズし、バイト列を書き出す／比べる。
/// MessagePackDump.ps1（.NET Framework 用）と同じ値を作る（messagepack_v1.txt は ps1 で作った基準）。
///
/// 使い方:
///   MessagePackDump.exe &lt;PPDMultiCommon.dll と MessagePack.dll があるフォルダー&gt; --out &lt;ファイル&gt;
///   MessagePackDump.exe &lt;フォルダー&gt; --compare &lt;基準ファイル&gt;
/// </summary>
static class Program
{
    static string binDir;

    static int Main(string[] args)
    {
        binDir = Path.GetFullPath(args[0]);
        AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
        {
            var path = Path.Combine(binDir, new AssemblyName(e.Name).Name + ".dll");
            return File.Exists(path) ? Assembly.LoadFrom(path) : null;
        };
        var common = Assembly.LoadFrom(Path.Combine(binDir, "PPDMultiCommon.dll"));
        var msgpack = Assembly.LoadFrom(Path.Combine(binDir, "MessagePack.dll"));
        var serializerType = msgpack.GetType("MessagePack.MessagePackSerializer");
        var serialize = serializerType.GetMethods().Where(m => m.Name == "Serialize" && m.IsGenericMethodDefinition && m.ReturnType == typeof(byte[]) && m.GetParameters()[0].ParameterType.IsGenericParameter).OrderBy(m => m.GetParameters().Length).First();
        var deserialize = serializerType.GetMethods().Where(m => m.Name == "Deserialize" && m.IsGenericMethodDefinition && m.GetParameters()[0].ParameterType.Name == "ReadOnlyMemory`1").OrderBy(m => m.GetParameters().Length).FirstOrDefault()
            ?? serializerType.GetMethods().Where(m => m.Name == "Deserialize" && m.IsGenericMethodDefinition && m.GetParameters()[0].ParameterType == typeof(byte[])).OrderBy(m => m.GetParameters().Length).First();

        var types = common.GetTypes().Where(t => !t.IsAbstract && t.GetCustomAttributes(false).Any(a => a.GetType().Name == "MessagePackObjectAttribute")).OrderBy(t => t.FullName, StringComparer.Ordinal).ToArray();
        var lines = new List<string>();
        foreach (var type in types)
        {
            var sample = NewSample(type, 0, 0);
            var bytes = (byte[])Invoke(serialize, type, sample);
            lines.Add(type.FullName + " " + BitConverter.ToString(bytes).Replace("-", ""));
        }
        Console.WriteLine("MessagePack {0}, types: {1}", msgpack.GetName().Version, types.Length);

        if (args[1] == "--out")
        {
            File.WriteAllLines(args[2], lines, new UTF8Encoding(false));
            Console.WriteLine("written: " + args[2]);
            return 0;
        }

        var expected = File.ReadAllLines(args[2]).Select(l => l.Split(' ')).ToDictionary(p => p[0], p => p[1]);
        var failures = 0;
        foreach (var line in lines)
        {
            var parts = line.Split(' ');
            if (!expected.TryGetValue(parts[0], out var hex))
            {
                Console.WriteLine("NEW TYPE " + parts[0]);
                continue;
            }
            if (hex != parts[1])
            {
                Console.WriteLine("DIFF serialize " + parts[0]);
                Console.WriteLine("  expected " + hex);
                Console.WriteLine("  actual   " + parts[1]);
                failures++;
            }
            // 古いバイト列を読み込んで書き直したときに同じバイト列になるか（相手が古い版でも通信できるか。Parser は再シリアライズで長さを求めている）
            var type = types.First(t => t.FullName == parts[0]);
            var old = Enumerable.Range(0, hex.Length / 2).Select(i => Convert.ToByte(hex.Substring(i * 2, 2), 16)).ToArray();
            var roundTrip = (byte[])Invoke(serialize, type, Invoke(deserialize, type, old));
            if (BitConverter.ToString(roundTrip).Replace("-", "") != hex)
            {
                Console.WriteLine("DIFF roundtrip " + parts[0]);
                failures++;
            }
            // 受信データには複数のメッセージが続けて入っていることがある（Parser は先頭の 1 つを読んでから残りを求める）。
            // 後ろに別のデータが続いていても、先頭の 1 つだけを読めること
            try
            {
                var concatenated = old.Concat(old).ToArray();
                var first = (byte[])Invoke(serialize, type, Invoke(deserialize, type, concatenated));
                if (BitConverter.ToString(first).Replace("-", "") != hex)
                {
                    Console.WriteLine("DIFF concatenated " + parts[0]);
                    failures++;
                }
            }
            catch (TargetInvocationException e)
            {
                Console.WriteLine("FAIL concatenated " + parts[0] + ": " + e.InnerException?.Message);
                failures++;
            }
        }
        foreach (var name in expected.Keys.Where(n => !lines.Any(l => l.StartsWith(n + " "))))
        {
            Console.WriteLine("MISSING TYPE " + name);
            failures++;
        }
        Console.WriteLine("failures: " + failures);
        return failures == 0 ? 0 : 1;
    }

    static object Invoke(MethodInfo method, Type type, object first)
    {
        var generic = method.MakeGenericMethod(type);
        var parameters = generic.GetParameters();
        var args = new object[parameters.Length];
        args[0] = parameters[0].ParameterType.Name == "ReadOnlyMemory`1" ? Activator.CreateInstance(parameters[0].ParameterType, first) : first;
        for (var i = 1; i < parameters.Length; i++)
        {
            args[i] = parameters[i].HasDefaultValue ? parameters[i].DefaultValue : null;
        }
        return generic.Invoke(null, args);
    }

    // MessagePackDump.ps1 の New-Sample と同じ値を作る
    static object NewSample(Type type, int depth, int seed)
    {
        if (type == typeof(string)) return "テスト" + seed;
        if (type == typeof(int)) return 1000 + seed;
        if (type == typeof(long)) return 10000000000L + seed;
        if (type == typeof(short)) return (short)(100 + seed);
        if (type == typeof(byte)) return (byte)(seed % 250);
        if (type == typeof(float)) return (float)(1.5 + seed);
        if (type == typeof(double)) return 2.25 + seed;
        if (type == typeof(bool)) return seed % 2 == 0;
        if (type == typeof(DateTime)) return new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc).AddSeconds(seed);
        if (type == typeof(TimeSpan)) return TimeSpan.FromSeconds(90 + seed);
        if (type == typeof(Guid)) return new Guid("01234567-89ab-cdef-0123-456789abcdef");
        if (type.IsEnum)
        {
            var values = Enum.GetValues(type);
            return values.GetValue(seed % values.Length);
        }
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) return NewSample(type.GetGenericArguments()[0], depth, seed);
        if (type.IsArray)
        {
            var element = type.GetElementType();
            var array = Array.CreateInstance(element, 2);
            for (var i = 0; i < 2; i++) array.SetValue(NewSample(element, depth + 1, seed + i + 1), i);
            return array;
        }
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            var element = type.GetGenericArguments()[0];
            var list = (IList)Activator.CreateInstance(type);
            for (var i = 0; i < 2; i++) list.Add(NewSample(element, depth + 1, seed + i + 1));
            return list;
        }
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            var dict = (IDictionary)Activator.CreateInstance(type);
            dict.Add(NewSample(type.GetGenericArguments()[0], depth + 1, seed + 1), NewSample(type.GetGenericArguments()[1], depth + 1, seed + 2));
            return dict;
        }
        if (type.IsGenericType && type.FullName.StartsWith("System.Tuple`"))
        {
            var j = 0;
            var values = type.GetGenericArguments().Select(t => NewSample(t, depth + 1, seed + ++j)).ToArray();
            return type.GetConstructor(type.GetGenericArguments()).Invoke(values);
        }
        if (depth > 3) return null;
        var obj = Activator.CreateInstance(type);
        var index = 0;
        foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!member.GetCustomAttributes(true).Any(a => a.GetType().Name == "KeyAttribute")) continue;
            index++;
            if (member is PropertyInfo property && property.CanWrite && property.GetSetMethod() != null)
            {
                property.SetValue(obj, NewSample(property.PropertyType, depth + 1, seed + index));
            }
            else if (member is FieldInfo field)
            {
                field.SetValue(obj, NewSample(field.FieldType, depth + 1, seed + index));
            }
        }
        return obj;
    }
}
