using PPDFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using Xunit;

namespace UnitTests.Libraries
{
    /// <summary>
    /// 起動時に、アプリのアセンブリを先にコンパイルしておく JitWarmup のテストです。
    /// </summary>
    /// <remarks>
    /// Start はずっと動くスレッドを作るので使わず、どれを対象にするかの判定と、小さなアセンブリのコンパイルだけを確かめる。
    /// </remarks>
    public class JitWarmupTests
    {
        static object Call(string name, params object[] args) =>
            typeof(JitWarmup).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, args);

        static void SetBaseDirectory()
        {
            typeof(JitWarmup).GetField("baseDirectory", BindingFlags.Static | BindingFlags.NonPublic)
                .SetValue(null, Path.GetFullPath(AppContext.BaseDirectory));
        }

        [Fact]
        public void IsTarget_OnlyAppAssemblies()
        {
            SetBaseDirectory();

            Assert.True((bool)Call("IsTarget", typeof(PPDMath.MathCompat).Assembly));
            // .NET のアセンブリ、動的なアセンブリは対象にしない
            Assert.False((bool)Call("IsTarget", typeof(object).Assembly));
            var dynamic = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Dynamic"), AssemblyBuilderAccess.Run);
            Assert.False((bool)Call("IsTarget", dynamic));
        }

        [Fact]
        public void ReferencesWpf_FollowsReferences()
        {
            SetBaseDirectory();
            var dir = AppContext.BaseDirectory;

            // WPF を直接、または参照先を通して使うものは除く
            Assert.True((bool)Call("ReferencesWpf", Path.Combine(dir, "PPDEditor.dll"), new HashSet<string>()));
            Assert.False((bool)Call("ReferencesWpf", Path.Combine(dir, "PPDMath.dll"), new HashSet<string>()));
            // 2 回目は覚えた結果
            Assert.False((bool)Call("ReferencesWpf", Path.Combine(dir, "PPDMath.dll"), new HashSet<string>()));

            // 読めないものは、安全のため除く
            using var folder = new TempFolder();
            var broken = folder.Write("broken.dll", "not a dll");
            Assert.True((bool)Call("ReferencesWpf", broken, new HashSet<string>()));
        }

        [Fact]
        public void Prepare_CompilesMethods()
        {
            var before = JitWarmup.PreparedCount;

            Call("Prepare", typeof(PPDMath.MathCompat).Assembly);

            Assert.True(JitWarmup.PreparedCount > before);
            Assert.True(JitWarmup.ElapsedMilliseconds >= 0);
        }
    }
}
