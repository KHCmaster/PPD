using PPDEditor;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Xunit;

namespace UnitTests.PPDEditor
{
    /// <summary>
    /// エディターの設定（PPDEditor.ini）を読む PPDStaticSetting のテストです。
    /// </summary>
    /// <remarks>
    /// 設定は静的コンストラクターで 1 回だけ読むので、PPDEditor.dll を別の読み込みの場所に読み込み直し、
    /// そこでエディターのフォルダーを一時フォルダーにしてから読ませる（テストの全体の設定は変えない）。
    /// </remarks>
    public sealed class EditorStaticSettingTests : IDisposable
    {
        readonly TempFolder folder = new();
        readonly AssemblyLoadContext context = new("EditorStaticSetting", isCollectible: true);
        Type setting;

        // PPDEditor.dll を一時フォルダーに写して読み込む（エディターのフォルダーは dll のあるフォルダーになる）
        void Load(string ini)
        {
            if (ini != null)
            {
                folder.Write("PPDEditor.ini", ini);
            }
            var dll = folder.Combine("PPDEditor.dll");
            File.Copy(typeof(PPDStaticSetting).Assembly.Location, dll);
            setting = context.LoadFromAssemblyPath(dll).GetType("PPDEditor.PPDStaticSetting");
        }

        public void Dispose()
        {
            context.Unload();
            folder.Dispose();
        }

        T Get<T>(string name) => (T)setting.GetField(name, BindingFlags.Static | BindingFlags.Public).GetValue(null);

        [Fact]
        public void ReadsEditorSettings()
        {
            Load("""
                [formattype]
                Maximised=1
                Size=1024,768
                Location=10,20
                AuthorName=作者
                MovieLatency=0.5
                Language=en
                HideToggleRectangle=1
                HideToggleArrow=1
                FixDockPanel=1
                EnableToChangeMarkTypeAndTime=1
                ApplyToIniMovieTrimming=1
                CustomCanvasBackColor=10,20,30
                CanvasSizeIndex=2
                CanvasColorIndex=3
                MemoFontSize=12.5
                Move0=2
                Move5=7
                Angle1=45
                Angle7=90
                """);

            Assert.Equal((System.Windows.WindowState.Maximized, new Size(1024, 768), new Point(10, 20)),
                (Get<System.Windows.WindowState>("LastWindowState"), Get<Size>("LastWindowSize"), Get<Point>("LastWindowLocation")));
            Assert.Equal(("作者", 0.5f, "en"), (Get<string>("AuthorName"), Get<float>("MovieLatency"), Get<string>("langFileISO")));
            Assert.All(new[] { "HideToggleRectangle", "HideToggleArrow", "FixDockPanel", "EnableToChangeMarkTypeAndTime", "ApplyToIniMovieTrimming" },
                name => Assert.True(Get<bool>(name)));
            Assert.Equal(Color.FromArgb(10, 20, 30).ToArgb(), Get<Color>("CustomCanvasBackColor").ToArgb());
            Assert.Equal((2, 3, 12.5f), (Get<int>("CanvasSizeIndex"), Get<int>("CanvasColorIndex"), Get<float>("MemoFontSize")));
            // 書いていない移動量と角度は既定の値
            Assert.Equal([2, 10, 5, 1, 1, 7, 1, 1], Get<int[]>("Moves"));
            Assert.Equal([0, 45, 15, 10, 10, 10, 10, 90], Get<int[]>("Angles"));
        }

        [Fact]
        public void BrokenValues_UseDefaults()
        {
            Load("""
                [formattype]
                Size=wide
                Location=1
                CustomCanvasBackColor=1,2,3,4
                CanvasSizeIndex=x
                MemoFontSize=big
                """);

            Assert.Equal((System.Windows.WindowState.Normal, new Size(800, 600), new Point(0, 0)),
                (Get<System.Windows.WindowState>("LastWindowState"), Get<Size>("LastWindowSize"), Get<Point>("LastWindowLocation")));
            // 4 つの数は不透明度から
            Assert.Equal(Color.FromArgb(1, 2, 3, 4).ToArgb(), Get<Color>("CustomCanvasBackColor").ToArgb());
            Assert.Equal((0, 10f, "jp"), (Get<int>("CanvasSizeIndex"), Get<float>("MemoFontSize"), Get<string>("langFileISO")));
            Assert.Equal(1, Get<int[]>("Moves").Skip(3).Distinct().Single());
        }

        [Fact]
        public void WithoutFile_KeepsInitialValues()
        {
            Load(null);

            Assert.Equal((new Size(800, 600), ""), (Get<Size>("LastWindowSize"), Get<string>("AuthorName")));
        }
    }
}
