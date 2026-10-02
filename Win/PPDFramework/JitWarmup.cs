using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace PPDFramework
{
    /// <summary>
    /// ゲームのアセンブリのメソッドを、低い優先度のスレッドで先にコンパイル（JIT）しておきます。
    /// </summary>
    /// <remarks>
    /// プレイ中に初めて動く処理（最初のノーツの作成や判定など）は、その場でコンパイルされて、そのフレームが重くなる（10ms 以上）。
    /// 起動後に先にコンパイルしておけば、プレイ中のコンパイルはほぼなくなる。
    /// 対象は、アプリのフォルダーにあるアセンブリ（FlowScript の拡張や Mod の dll も、読み込まれた時点で対象にする）。
    /// .NET 自体のアセンブリは、事前にコンパイル済み（ReadyToRun）なので対象にしない。
    /// </remarks>
    public static class JitWarmup
    {
        static readonly Queue<Assembly> queue = new();
        static readonly HashSet<Assembly> queued = [];
        static Thread thread;
        static string baseDirectory;
        static int preparedCount;
        static long elapsedTicks;

        /// <summary>
        /// コンパイルしたメソッドの数を取得します。
        /// </summary>
        public static int PreparedCount
        {
            get
            {
                return preparedCount;
            }
        }

        /// <summary>
        /// コンパイルにかかった時間（ミリ秒）を取得します。
        /// </summary>
        public static double ElapsedMilliseconds
        {
            get
            {
                return Interlocked.Read(ref elapsedTicks) * 1000.0 / Stopwatch.Frequency;
            }
        }

        /// <summary>
        /// 読み込み済みのアセンブリと、これから読み込まれるアセンブリのコンパイルを始めます（2 回目以降の呼び出しは何もしません）。
        /// </summary>
        public static void Start()
        {
            lock (queue)
            {
                if (thread != null)
                {
                    return;
                }
                baseDirectory = Path.GetFullPath(AppContext.BaseDirectory);
                AppDomain.CurrentDomain.AssemblyLoad += (sender, e) => Enqueue(e.LoadedAssembly);
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Enqueue(assembly);
                }
                thread = new Thread(Run)
                {
                    IsBackground = true,
                    Priority = ThreadPriority.Lowest,
                    Name = "JitWarmup",
                };
                thread.Start();
            }
        }

        private static void Enqueue(Assembly assembly)
        {
            if (!IsTarget(assembly))
            {
                return;
            }
            lock (queue)
            {
                if (queued.Add(assembly))
                {
                    queue.Enqueue(assembly);
                    Monitor.Pulse(queue);
                }
            }
        }

        private static bool IsTarget(Assembly assembly)
        {
            if (assembly.IsDynamic || string.IsNullOrEmpty(assembly.Location))
            {
                return false;
            }
            var name = assembly.GetName().Name ?? "";
            if (name.StartsWith("System.") || name.StartsWith("Microsoft.") || name == "System" || name == "mscorlib" || name == "netstandard")
            {
                return false;
            }
            // アプリのフォルダー（dlls や mods の下を含む）にあるものだけ
            return Path.GetFullPath(assembly.Location).StartsWith(baseDirectory, StringComparison.OrdinalIgnoreCase);
        }

        static readonly string[] wpfAssemblyNames = ["PresentationFramework", "PresentationCore", "WindowsBase", "System.Xaml", "UIAutomationProvider", "UIAutomationTypes"];
        static readonly Dictionary<string, bool> wpfReferenceCache = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// アセンブリが WPF を参照しているかを、メタデータだけを読んで調べます（アセンブリは読み込まない）。
        /// アプリのフォルダーにある参照先は、たどって調べます。
        /// </summary>
        private static bool ReferencesWpf(string path, HashSet<string> visiting)
        {
            lock (wpfReferenceCache)
            {
                if (wpfReferenceCache.TryGetValue(path, out var cached))
                {
                    return cached;
                }
            }
            if (!visiting.Add(path))
            {
                return false;
            }
            var result = false;
            try
            {
                using var stream = File.OpenRead(path);
                using var peReader = new System.Reflection.PortableExecutable.PEReader(stream);
                var reader = System.Reflection.Metadata.PEReaderExtensions.GetMetadataReader(peReader);
                foreach (var handle in reader.AssemblyReferences)
                {
                    var name = reader.GetString(reader.GetAssemblyReference(handle).Name);
                    if (Array.IndexOf(wpfAssemblyNames, name) >= 0)
                    {
                        result = true;
                        break;
                    }
                    var referencePath = Path.Combine(Path.GetDirectoryName(path), name + ".dll");
                    if (!File.Exists(referencePath))
                    {
                        referencePath = Path.Combine(baseDirectory, name + ".dll");
                    }
                    if (File.Exists(referencePath) && ReferencesWpf(referencePath, visiting))
                    {
                        result = true;
                        break;
                    }
                }
            }
            catch
            {
                // 調べられないものは、安全のため対象にしない
                result = true;
            }
            lock (wpfReferenceCache)
            {
                wpfReferenceCache[path] = result;
            }
            return result;
        }

        private static void Run()
        {
            while (true)
            {
                Assembly assembly;
                lock (queue)
                {
                    while (queue.Count == 0)
                    {
                        Monitor.Wait(queue);
                    }
                    assembly = queue.Dequeue();
                }
                // WPF を（間接的にも）参照するものは除く。コンパイルするだけで WPF が読み込まれて初期化され、
                // プロセスの DPI の扱いが変わって、ウィンドウの大きさや表示がおかしくなる
                if (ReferencesWpf(assembly.Location, []))
                {
                    continue;
                }
                var start = Stopwatch.GetTimestamp();
                Prepare(assembly);
                Interlocked.Add(ref elapsedTicks, Stopwatch.GetTimestamp() - start);
            }
        }

        private static void Prepare(Assembly assembly)
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                // 読み込めない型（参照先がないものなど）は飛ばす
                types = e.Types;
            }
            catch
            {
                return;
            }
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            foreach (var type in types)
            {
                if (type == null || type.ContainsGenericParameters)
                {
                    continue;
                }
                MethodBase[] methods;
                try
                {
                    methods = [.. type.GetMethods(flags), .. type.GetConstructors(flags)];
                }
                catch
                {
                    continue;
                }
                foreach (var method in methods)
                {
                    if (method.IsAbstract || method.ContainsGenericParameters ||
                        (method.MethodImplementationFlags & (MethodImplAttributes.InternalCall | MethodImplAttributes.Runtime)) != 0 ||
                        (method.Attributes & MethodAttributes.PinvokeImpl) != 0)
                    {
                        continue;
                    }
                    try
                    {
                        RuntimeHelpers.PrepareMethod(method.MethodHandle);
                        Interlocked.Increment(ref preparedCount);
                    }
                    catch
                    {
                        // コンパイルできないメソッドは、その場でコンパイルされる（今までどおり）
                    }
                }
            }
        }
    }
}
