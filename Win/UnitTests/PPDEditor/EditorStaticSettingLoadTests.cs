using PPDEditor;
using PPDEditor.ViewModels.Docks;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using Xunit;
using Color = System.Drawing.Color;
using Utility = PPDEditor.Utility;
using WindowState = System.Windows.WindowState;

namespace UnitTests.PPDEditor
{
    /// <summary>
    /// エディターの設定ファイル（PPDEditor.ini）を、起動時の設定（PPDStaticSetting）に読み込むテストです。
    /// </summary>
    /// <remarks>
    /// 設定はプロセス全体で 1 つなので、テストの後で元に戻す。
    /// </remarks>
    public sealed class EditorStaticSettingLoadTests : IDisposable
    {
        static readonly FieldInfo[] fields = typeof(PPDStaticSetting).GetFields(BindingFlags.Static | BindingFlags.Public);
        readonly object[] saved = [.. fields.Select(f => f.GetValue(null) is Array a ? a.Clone() : f.GetValue(null))];

        public void Dispose()
        {
            for (var i = 0; i < fields.Length; i++)
            {
                if (saved[i] is Array a)
                {
                    Array.Copy(a, (Array)fields[i].GetValue(null), a.Length);
                }
                else
                {
                    fields[i].SetValue(null, saved[i]);
                }
            }
        }

        [Fact]
        public void Load_ReadsEveryValue()
        {
            PPDStaticSetting.Load(string.Join("\n",
                "[Maximised]1", "[Size]1024,768", "[Location]10,20", "[AuthorName]作者", "[MovieLatency]0.25", "[Language]en",
                "[HideToggleRectangle]1", "[HideToggleArrow]1", "[FixDockPanel]1", "[EnableToChangeMarkTypeAndTime]1", "[ApplyToIniMovieTrimming]1",
                "[CustomCanvasBackColor]128, 64, 32, 16", "[CanvasSizeIndex]2", "[CanvasColorIndex]3", "[MemoFontSize]12.5",
                "[Move0]2", "[Move5]7", "[Angle1]45", "[Angle7]90"));

            Assert.Equal(WindowState.Maximized, PPDStaticSetting.LastWindowState);
            Assert.Equal((1024, 768), (PPDStaticSetting.LastWindowSize.Width, PPDStaticSetting.LastWindowSize.Height));
            Assert.Equal((10, 20), (PPDStaticSetting.LastWindowLocation.X, PPDStaticSetting.LastWindowLocation.Y));
            Assert.Equal(("作者", 0.25f, "en"), (PPDStaticSetting.AuthorName, PPDStaticSetting.MovieLatency, PPDStaticSetting.langFileISO));
            Assert.True(PPDStaticSetting.HideToggleRectangle && PPDStaticSetting.HideToggleArrow && PPDStaticSetting.FixDockPanel
                && PPDStaticSetting.EnableToChangeMarkTypeAndTime && PPDStaticSetting.ApplyToIniMovieTrimming);
            // 4 つの数は、透明度と RGB
            Assert.Equal(Color.FromArgb(128, 64, 32, 16), PPDStaticSetting.CustomCanvasBackColor);
            Assert.Equal((2, 3, 12.5f), (PPDStaticSetting.CanvasSizeIndex, PPDStaticSetting.CanvasColorIndex, PPDStaticSetting.MemoFontSize));
            Assert.Equal([2, 10, 5, 1, 1, 7, 1, 1], PPDStaticSetting.Moves);
            Assert.Equal([0, 45, 15, 10, 10, 10, 10, 90], PPDStaticSetting.Angles);
        }

        [Fact]
        public void Load_UsesDefaultsForMissingOrBrokenValues()
        {
            PPDStaticSetting.Load("[Size]wide\n[Location]1\n[CustomCanvasBackColor]1, 2, 3\n[MemoFontSize]big\n[Move1]x");

            Assert.Equal((800, 600), (PPDStaticSetting.LastWindowSize.Width, PPDStaticSetting.LastWindowSize.Height));
            Assert.Equal((0, 0), (PPDStaticSetting.LastWindowLocation.X, PPDStaticSetting.LastWindowLocation.Y));
            // 言語がなければ日本語
            Assert.Equal("jp", PPDStaticSetting.langFileISO);
            Assert.False(PPDStaticSetting.FixDockPanel);
            Assert.Equal(Color.FromArgb(1, 2, 3), PPDStaticSetting.CustomCanvasBackColor);
            Assert.Equal(10f, PPDStaticSetting.MemoFontSize);
            Assert.Equal(10, PPDStaticSetting.Moves[1]);
            Assert.Equal(0f, PPDStaticSetting.MovieLatency);

            PPDStaticSetting.Load("[CustomCanvasBackColor]none");
            Assert.Equal(Color.White, PPDStaticSetting.CustomCanvasBackColor);
        }

        [Fact]
        public void MovieLatency_IsReadInAnyCulture()
        {
            // 保存するときは「.」を使う。「,」を小数点にする言語の設定でも、同じ値に読む
            var culture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            try
            {
                PPDStaticSetting.Load("[MovieLatency]0.25");
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = culture;
            }

            Assert.Equal(0.25f, PPDStaticSetting.MovieLatency);
        }

        [Theory]
        [InlineData("Functions", true, false)]
        [InlineData("Commands", false, true)]
        [InlineData("Projects", false, false)]
        public void ScriptPaths_KnowEditorFolders(string folder, bool isFunctions, bool isCommands)
        {
            var path = System.IO.Path.Combine(Utility.AppDir, folder, "a.fsml");

            Assert.Equal(isFunctions, ScriptPaths.IsUnderFunctions(path));
            Assert.Equal(isCommands, ScriptPaths.IsUnderCommands(path.ToUpperInvariant()));
        }

        [Fact]
        public void FlowLog_AddsAndClears()
        {
            var log = new FlowLogViewModel();

            log.AddLog("Info", "text");
            Assert.Equal([new("Info", "text")], log.Logs);
            Assert.NotNull(log.Texts);

            log.ClearCommand.Execute(null);
            Assert.Empty(log.Logs);
        }
    }
}
