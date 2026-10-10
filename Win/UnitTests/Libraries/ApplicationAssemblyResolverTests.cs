using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Xunit;

namespace UnitTests.Libraries
{
    /// <summary>
    /// 見つからない dll を、アプリのフォルダーから探すテストです（全ての exe に入る PPDShared.ProcessInitializer）。
    /// </summary>
    /// <remarks>
    /// PPD が skins から読み込む PPDSingle などは、アプリのフォルダーの SharpZipLib などを使う（.NET では探さないので、探すようにした）。
    /// 既定の読み込み先を変えないように、テストだけの読み込み先で確かめる。
    /// ProcessInitializer はこのテストにも、参照している exe にも入っているので、名前がぶつからないように、このテストの中のものをリフレクションで呼ぶ。
    /// </remarks>
    public class ApplicationAssemblyResolverTests
    {
        private static Assembly Resolve(AssemblyLoadContext context, AssemblyName name, string directory)
        {
            var method = typeof(ApplicationAssemblyResolverTests).Assembly.GetType("PPDShared.ProcessInitializer")
                .GetMethod("ResolveFromDirectory", BindingFlags.Static | BindingFlags.NonPublic);
            return (Assembly)method.Invoke(null, [context, name, directory]);
        }

        [Fact]
        public void Resolve_LoadsTheDllInTheFolder()
        {
            using var folder = new TempFolder();
            var source = typeof(PPDMath.MathCompat).Assembly.Location;
            File.Copy(source, folder.Combine(Path.GetFileName(source)));
            var context = new AssemblyLoadContext("ApplicationAssemblyResolverTests", isCollectible: true);
            try
            {
                var assembly = Resolve(context, new AssemblyName("PPDMath"), folder.Path);

                Assert.Equal(folder.Combine("PPDMath.dll"), assembly.Location);
                Assert.Same(context, AssemblyLoadContext.GetLoadContext(assembly));
            }
            finally
            {
                context.Unload();
            }
        }

        [Fact]
        public void Resolve_MissingDll_ReturnsNull()
        {
            using var folder = new TempFolder();
            var context = new AssemblyLoadContext("ApplicationAssemblyResolverTests", isCollectible: true);
            try
            {
                Assert.Null(Resolve(context, new AssemblyName("NotExisting"), folder.Path));
                Assert.Null(Resolve(context, new AssemblyName(), folder.Path));
            }
            finally
            {
                context.Unload();
            }
        }

        [Fact]
        public void Resolve_OlderOrNotManagedDll_ReturnsNull()
        {
            // 古い版が残っていても使わない（skins にある正しい版を、.NET の探し方で見つけられるようにする）
            using var folder = new TempFolder();
            var source = typeof(PPDMath.MathCompat).Assembly.Location;
            File.Copy(source, folder.Combine("PPDMath.dll"));
            folder.Write("Native.dll", "not a .NET dll");
            var version = AssemblyName.GetAssemblyName(source).Version;
            var context = new AssemblyLoadContext("ApplicationAssemblyResolverTests", isCollectible: true);
            try
            {
                Assert.Null(Resolve(context, new AssemblyName("PPDMath") { Version = new System.Version(version.Major + 1, 0, 0, 0) }, folder.Path));
                Assert.NotNull(Resolve(context, new AssemblyName("PPDMath") { Version = version }, folder.Path));
                Assert.Null(Resolve(context, new AssemblyName("Native"), folder.Path));
            }
            finally
            {
                context.Unload();
            }
        }
    }
}
