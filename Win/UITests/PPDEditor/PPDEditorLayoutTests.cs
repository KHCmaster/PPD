using FlaUI.Core.AutomationElements;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace UITests.PPDEditor
{
    /// <summary>
    /// 以前の WinForms 版（DockPanel Suite）の形式の、ドックの配置のファイル（dockforminfo.xml）を読むテストです。
    /// </summary>
    /// <remarks>
    /// PPDEditorFixture と同じファイルはコピーしない。配置のファイルはテストで作る。
    /// </remarks>
    public sealed class PPDEditorLayoutTests : IDisposable
    {
        /// <summary>
        /// ゲームの画面を真ん中、タイムラインを下、プロパティとレイヤーを右（上下に分ける）、歌詞を浮かせ、メモを隠した配置です。
        /// </summary>
        const string dockPanelSuiteLayout = """
            <?xml version="1.0" encoding="utf-16"?>
            <DockPanel FormatVersion="1.0" DockLeftPortion="0.25" DockRightPortion="300" DockTopPortion="0.25" DockBottomPortion="0.3" ActiveDocumentPane="0" ActivePane="-1">
              <Contents Count="6">
                <Content ID="0" PersistString="PPDEditor.DXForm" AutoHidePortion="0.25" IsHidden="False" IsFloat="False" />
                <Content ID="1" PersistString="PPDEditor.TimeLineForm" AutoHidePortion="0.25" IsHidden="False" IsFloat="False" />
                <Content ID="2" PersistString="PPDEditor.InfoForm" AutoHidePortion="0.25" IsHidden="False" IsFloat="False" />
                <Content ID="3" PersistString="PPDEditor.LayerManager" AutoHidePortion="0.25" IsHidden="False" IsFloat="False" />
                <Content ID="4" PersistString="PPDEditor.KasiEditor" AutoHidePortion="0.25" IsHidden="False" IsFloat="True" />
                <Content ID="5" PersistString="PPDEditor.MemoWindow" AutoHidePortion="0.25" IsHidden="True" IsFloat="False" />
              </Contents>
              <Panes Count="5">
                <Pane ID="0" DockState="Document" ActiveContent="0">
                  <Contents Count="1"><Content ID="0" RefID="0" /></Contents>
                </Pane>
                <Pane ID="1" DockState="DockBottom" ActiveContent="1">
                  <Contents Count="1"><Content ID="0" RefID="1" /></Contents>
                </Pane>
                <Pane ID="2" DockState="DockRight" ActiveContent="2">
                  <Contents Count="1"><Content ID="0" RefID="2" /></Contents>
                </Pane>
                <Pane ID="3" DockState="DockRight" ActiveContent="3">
                  <Contents Count="2"><Content ID="0" RefID="3" /><Content ID="1" RefID="5" /></Contents>
                </Pane>
                <Pane ID="4" DockState="Float" ActiveContent="4">
                  <Contents Count="1"><Content ID="0" RefID="4" /></Contents>
                </Pane>
              </Panes>
              <DockWindows>
                <DockWindow ID="0" DockState="Document" ZOrderIndex="0">
                  <NestedPanes Count="1"><Pane ID="0" RefID="0" PrevPane="-1" Alignment="Right" Proportion="0.5" /></NestedPanes>
                </DockWindow>
                <DockWindow ID="1" DockState="DockLeft" ZOrderIndex="1"><NestedPanes Count="0" /></DockWindow>
                <DockWindow ID="2" DockState="DockRight" ZOrderIndex="2">
                  <NestedPanes Count="2">
                    <Pane ID="0" RefID="2" PrevPane="-1" Alignment="Right" Proportion="0.5" />
                    <Pane ID="1" RefID="3" PrevPane="0" Alignment="Bottom" Proportion="0.4" />
                  </NestedPanes>
                </DockWindow>
                <DockWindow ID="3" DockState="DockTop" ZOrderIndex="3"><NestedPanes Count="0" /></DockWindow>
                <DockWindow ID="4" DockState="DockBottom" ZOrderIndex="4">
                  <NestedPanes Count="1"><Pane ID="0" RefID="1" PrevPane="-1" Alignment="Right" Proportion="0.5" /></NestedPanes>
                </DockWindow>
              </DockWindows>
              <FloatWindows Count="1">
                <FloatWindow ID="0" Bounds="100, 100, 300, 200" ZOrderIndex="0">
                  <NestedPanes Count="1"><Pane ID="0" RefID="4" PrevPane="-1" Alignment="Right" Proportion="0.5" /></NestedPanes>
                </FloatWindow>
              </FloatWindows>
            </DockPanel>
            """;

        readonly AppSession session = AppSession.Start(Path.Combine("PPDEditor", "PPDEditor"), "PPDEditor.exe", new AppStartOptions
        {
            ExcludedFiles = ["Projects", "projectcache.dat", "PPDFramework.setting.db", "PPDUpdater.ini", "dockforminfo.xml"],
            MainWindowTitle = "PPDEditor",
            StartupTimeout = TimeSpan.FromMinutes(1),
            Prepare = workDirectory => File.WriteAllText(Path.Combine(workDirectory, "dockforminfo.xml"), dockPanelSuiteLayout.Replace("utf-16", "utf-8")),
        });

        public PPDEditorLayoutTests()
        {
            try
            {
                // 作者名がないときは、起動したときに入力のダイアログが出るので閉じる
                session.ModalWindowsOf().FirstOrDefault()?.Close();
            }
            catch
            {
                session.Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            session.Dispose();
        }

        [Fact]
        public void DockPanelSuiteLayout_IsImported()
        {
            var showMenu = session.FindMenuItem("ShowMenu");
            showMenu.Expand();
            // ドックの 16 個の後に、配置の読み込みと固定の項目がある
            var shown = showMenu.Items.Take(16).Select(item => item.Properties.ItemStatus.ValueOrDefault == "True").ToArray();
            showMenu.Collapse();

            // 表示のメニューの順: ゲーム、タイムライン、プロパティ、位置と角度、歌詞、音、ini、BPM、レイヤー、イベント、図形、メモ、リソース、スクリプト、統計、ヘルプ
            Assert.True(shown[0], "ゲームの画面");
            Assert.True(shown[1], "タイムライン");
            Assert.True(shown[2], "プロパティ");
            Assert.True(shown[4], "浮かせた歌詞");
            Assert.True(shown[8], "レイヤー");
            Assert.False(shown[11], "隠したメモ");
            // 配置のファイルにないウィンドウは出さない
            Assert.False(shown[3], "位置と角度: " + string.Join(",", shown));
        }
    }
}
