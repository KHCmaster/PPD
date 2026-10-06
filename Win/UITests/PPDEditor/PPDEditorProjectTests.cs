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
            SetBeatWidth();
            var timeline = session.Find("Seekmain");
            session.DoubleClickAt(timeline, Beat(1), Row(2));
            session.DoubleClickAt(timeline, Beat(2), Row(3));
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

        /// <summary>
        /// タイムラインの行の真ん中の高さです（上の時間の欄が 22、行が 26 の高さ）。
        /// </summary>
        static double Row(int row) => 22 + 26 * row + 13;

        /// <summary>
        /// タイムラインの 1 拍の幅（ピクセル）です。マークは拍の区切りに吸着するので、拍ちょうどの位置をクリックする。
        /// </summary>
        const int BeatWidth = 100;

        static double Beat(double beat) => BeatWidth * beat;

        void SetBeatWidth()
        {
            session.Find("DisplayWidthBox").AsTextBox().Text = BeatWidth.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        [Fact]
        public void TimelineAreaSelectionDragAndSeek()
        {
            CreateProject();
            SetBeatWidth();
            var emptySize = new FileInfo(LayerFile).Length;
            var timeline = session.Find("Seekmain");
            foreach (var beat in new[] { 1, 2, 3 })
            {
                session.DoubleClickAt(timeline, Beat(beat), Row(2));
            }
            Save();
            var threeMarks = new FileInfo(LayerFile).Length;

            // マークをドラッグして時間を動かす。Shift を押しながらなら、別の行へ
            session.DragAt(timeline, Beat(1), Row(2), Beat(1.5), Row(2));
            FlaUI.Core.Input.Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.SHIFT);
            try
            {
                session.DragAt(timeline, Beat(2), Row(2), Beat(2), Row(4));
            }
            finally
            {
                FlaUI.Core.Input.Keyboard.Release(FlaUI.Core.WindowsAPI.VirtualKeyShort.SHIFT);
            }
            Save();
            Assert.Equal(threeMarks, new FileInfo(LayerFile).Length);

            // 範囲を選んで、まとめて消す
            session.DragAt(timeline, Beat(0.5), Row(1), Beat(3.5), Row(5));
            FlaUI.Core.Input.Keyboard.Type(FlaUI.Core.WindowsAPI.VirtualKeyShort.DELETE);
            FlaUI.Core.Input.Wait.UntilInputIsProcessed();
            Save();
            Assert.Equal(emptySize, new FileInfo(LayerFile).Length);
            FlaUI.Core.Input.Keyboard.TypeSimultaneously(FlaUI.Core.WindowsAPI.VirtualKeyShort.CONTROL, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_Z);
            FlaUI.Core.Input.Wait.UntilInputIsProcessed();
            Save();
            Assert.Equal(threeMarks, new FileInfo(LayerFile).Length);

            // 上の時間の欄を押すと、その時間へ移る
            session.DragAt(timeline, Beat(3), 10, Beat(3), 10);
            Save();
            var seekTime = double.Parse(ProjectValue("Base.lastseektime"), System.Globalization.CultureInfo.InvariantCulture);
            Assert.True(seekTime > 0.1, seekTime.ToString(System.Globalization.CultureInfo.InvariantCulture));
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

            // タイムラインの行をダブルクリックして、マークを置く
            SetBeatWidth();
            session.DoubleClickAt(session.Find("Seekmain"), Beat(2), Row(2));

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
