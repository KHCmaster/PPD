using FlaUI.Core.AutomationElements;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace UITests.PPDEditor
{
    /// <summary>
    /// PPDEditor でプロジェクトを作り、タイムラインにマークを置いて保存し、開き直すテストです。
    /// </summary>
    /// <remarks>
    /// 音は、起動する前に作業のフォルダに無音の WAV を作って使う。プロジェクトは作業のフォルダの Projects に作る。
    /// </remarks>
    public sealed class PPDEditorProjectTests : IDisposable
    {
        const string ProjectName = "uitest";
        string workDirectory;
        readonly AppSession session;

        public PPDEditorProjectTests()
        {
            session = AppSession.Start(Path.Combine("PPDEditor", "PPDEditor"), "PPDEditor.exe", new AppStartOptions
            {
                ExcludedFiles = ["Projects", "projectcache.dat", "PPDFramework.setting.db", "PPDUpdater.ini"],
                MainWindowTitle = "PPDEditor",
                StartupTimeout = TimeSpan.FromMinutes(1),
                Prepare = directory =>
                {
                    workDirectory = directory;
                    WriteSilentWave(Path.Combine(directory, "sound.wav"), seconds: 3);
                },
            });
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

        /// <summary>
        /// 無音の WAV（16 ビット、モノラル、22050 Hz）を書きます。
        /// </summary>
        static void WriteSilentWave(string path, int seconds)
        {
            const int sampleRate = 22050;
            var dataLength = sampleRate * 2 * seconds;
            using var writer = new BinaryWriter(File.Create(path));
            writer.Write("RIFF"u8);
            writer.Write(36 + dataLength);
            writer.Write("WAVEfmt "u8);
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)1);
            writer.Write(sampleRate);
            writer.Write(sampleRate * 2);
            writer.Write((short)2);
            writer.Write((short)16);
            writer.Write("data"u8);
            writer.Write(dataLength);
            writer.Write(new byte[dataLength]);
        }

        string ProjectFile => Path.Combine(workDirectory, "Projects", ProjectName + ".ppdproj");

        string LayerFile => Path.Combine(workDirectory, "Projects", ProjectName, "Base_layers", "layer0.ppd");

        void CreateProject()
        {
            AppSession.InvokeAsync(session.FindMenuItem("fileMenu", "NewProjectMenuItem"));
            var dialog = session.WaitForModal();
            dialog.FindFirstDescendant(cf => cf.ByAutomationId("ProjectNameBox")).AsTextBox().Text = ProjectName;
            dialog.FindFirstDescendant(cf => cf.ByAutomationId("ProjectMovieBox")).AsTextBox().Text = Path.Combine(workDirectory, "sound.wav");
            var ok = dialog.FindFirstDescendant(cf => cf.ByAutomationId("NewProjectOkButton")).AsButton();
            Assert.True(AppSession.WaitUntil(() => ok.IsEnabled), "OK を押せません");
            AppSession.InvokeAsync(ok);
            Assert.True(session.WaitForNoModal(), "ダイアログが閉じません: " + session.DescribeModalWindows());
            Assert.True(AppSession.WaitUntil(() => File.Exists(ProjectFile)), "プロジェクトができません");
        }

        void Save()
        {
            var before = File.GetLastWriteTimeUtc(ProjectFile);
            session.FindMenuItem("fileMenu", "OverwriteProjectMenuItem").Invoke();
            Assert.True(AppSession.WaitUntil(() => File.GetLastWriteTimeUtc(ProjectFile) > before), "保存されません");
            // 保存したことを知らせる小さなウィンドウは、しばらくすると消える
        }

        /// <summary>
        /// 保存したプロジェクトの値です（[キー]値 の行）。
        /// </summary>
        string ProjectValue(string key)
        {
            var line = File.ReadAllLines(ProjectFile).FirstOrDefault(l => l.StartsWith("[" + key + "]", StringComparison.Ordinal));
            return line?[(key.Length + 2)..];
        }

        [Fact]
        public void KeyboardEditsMarksWithUndo()
        {
            CreateProject();
            var timeline = session.Find("Seekmain");
            session.DoubleClickAt(timeline, 200, 22 + 26 * 2 + 13);
            session.DoubleClickAt(timeline, 400, 22 + 26 * 3 + 13);
            Save();
            var twoMarks = new FileInfo(LayerFile).Length;

            // 選んでいるマーク（後に置いたもの）を、矢印で動かしてから消す
            FlaUI.Core.Input.Keyboard.Type(FlaUI.Core.WindowsAPI.VirtualKeyShort.RIGHT);
            FlaUI.Core.Input.Keyboard.Type(FlaUI.Core.WindowsAPI.VirtualKeyShort.UP);
            FlaUI.Core.Input.Keyboard.Type(FlaUI.Core.WindowsAPI.VirtualKeyShort.DELETE);
            FlaUI.Core.Input.Wait.UntilInputIsProcessed();
            Save();
            var oneMark = new FileInfo(LayerFile).Length;
            Assert.True(oneMark < twoMarks, $"{oneMark} < {twoMarks}");

            // 元に戻すと、マークが戻る
            FlaUI.Core.Input.Keyboard.TypeSimultaneously(FlaUI.Core.WindowsAPI.VirtualKeyShort.CONTROL, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_Z);
            FlaUI.Core.Input.Wait.UntilInputIsProcessed();
            Save();
            Assert.Equal(twoMarks, new FileInfo(LayerFile).Length);
        }

        [Fact]
        public void PlayPauseAndStopMoveTheSeekTime()
        {
            CreateProject();

            // 再生して止めると、その時間が保存される
            session.FindMenuItem("movieMenu", "PlayMenuItem").Invoke();
            System.Threading.Thread.Sleep(1500);
            session.FindMenuItem("movieMenu", "PauseMenuItem").Invoke();
            Save();
            var seekTime = double.Parse(ProjectValue("Base.lastseektime"), System.Globalization.CultureInfo.InvariantCulture);
            Assert.True(seekTime > 0.5, seekTime.ToString(System.Globalization.CultureInfo.InvariantCulture));

            // 止めると最初に戻る
            session.FindMenuItem("movieMenu", "StopMenuItem").Invoke();
            Save();
            Assert.Equal("0", ProjectValue("Base.lastseektime"));
        }

        [Fact]
        public void CreatePlaceMarkSaveAndReopen()
        {
            CreateProject();
            // 音はプロジェクトのフォルダに movie としてコピーされる
            Assert.True(File.Exists(Path.Combine(workDirectory, "Projects", ProjectName, "movie.wav")));
            Assert.True(File.Exists(LayerFile));
            var emptySize = new FileInfo(LayerFile).Length;

            // タイムラインの行（上の時間の欄が 22、行が 26 の高さ）をダブルクリックして、マークを置く
            session.DoubleClickAt(session.Find("Seekmain"), 200, 22 + 26 * 2 + 13);

            Save();
            Assert.True(AppSession.WaitUntil(() => new FileInfo(LayerFile).Length > emptySize), "マークが保存されません");

            // 開き直すと、同じプロジェクトになる
            AppSession.InvokeAsync(session.FindMenuItem("fileMenu", "OpenProjectMenuItem"));
            FileDialogs.Accept(session.WaitForModal(), ProjectFile);
            Assert.True(session.WaitForNoModal(), "開けません: " + session.DescribeModalWindows());
            Assert.True(AppSession.WaitUntil(() => session.MainWindow.Title.Contains(ProjectName)), session.MainWindow.Title);
        }
    }
}
