using System;
using System.IO;
using System.Linq;
using Xunit;

namespace UITests.PPDEditor
{
    /// <summary>
    /// PPDEditor を終わらせるテストです（ほかのテストと同じアプリを使わないように、テストごとに起動する）。
    /// </summary>
    public sealed class PPDEditorExitTests : IDisposable
    {
        readonly AppSession session = AppSession.Start(Path.Combine("PPDEditor", "PPDEditor"), "PPDEditor.exe", new AppStartOptions
        {
            ExcludedFiles = ["Projects", "projectcache.dat", "PPDFramework.setting.db", "PPDUpdater.ini"],
            MainWindowTitle = "PPDEditor",
            StartupTimeout = TimeSpan.FromMinutes(1),
        });

        public PPDEditorExitTests()
        {
            // 作者名がないときは、起動したときに入力のダイアログが出るので閉じる
            session.ModalWindowsOf().FirstOrDefault()?.Close();
        }

        public void Dispose()
        {
            session.Dispose();
        }

        [Fact]
        public void ExitMenu_EndsTheProcess()
        {
            AppSession.InvokeAsync(session.FindMenuItem("fileMenu", "ExitMenuItem"));

            Assert.True(session.WaitForExit(), "プロセスが終わりません");
        }

        [Fact]
        public void CloseButton_EndsTheProcess()
        {
            session.CloseWindow(session.MainWindow);

            Assert.True(session.WaitForExit(), "プロセスが終わりません");
        }
    }
}
