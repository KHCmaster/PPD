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

        /// <summary>
        /// ドックの配置です（DockPanel Suite の形式）。ゲームの画面を真ん中、タイムラインを下（高さ 300）、プロパティを右に置き、
        /// リソースとレイヤーは真ん中のゲームの画面の右に（上下に分けて）大きく置く（小さくドックしていると、中を探すのに時間がかかり、不安定になるため。
        /// 浮かせると、AvalonDock は中身を別のウィンドウに入れるので、UI Automation から中が見えない）。
        /// </summary>
        const string Layout = """
            <?xml version="1.0" encoding="utf-8"?>
            <DockPanel FormatVersion="1.0" DockLeftPortion="0.25" DockRightPortion="300" DockTopPortion="0.25" DockBottomPortion="300" ActiveDocumentPane="0" ActivePane="-1">
              <Contents Count="5">
                <Content ID="0" PersistString="PPDEditor.DXForm" AutoHidePortion="0.25" IsHidden="False" IsFloat="False" />
                <Content ID="1" PersistString="PPDEditor.TimeLineForm" AutoHidePortion="0.25" IsHidden="False" IsFloat="False" />
                <Content ID="2" PersistString="PPDEditor.InfoForm" AutoHidePortion="0.25" IsHidden="False" IsFloat="False" />
                <Content ID="3" PersistString="PPDEditor.LayerManager" AutoHidePortion="0.25" IsHidden="False" IsFloat="False" />
                <Content ID="4" PersistString="PPDEditor.ResourceManager" AutoHidePortion="0.25" IsHidden="False" IsFloat="False" />
              </Contents>
              <Panes Count="5">
                <Pane ID="0" DockState="Document" ActiveContent="0"><Contents Count="1"><Content ID="0" RefID="0" /></Contents></Pane>
                <Pane ID="1" DockState="DockBottom" ActiveContent="1"><Contents Count="1"><Content ID="0" RefID="1" /></Contents></Pane>
                <Pane ID="2" DockState="DockRight" ActiveContent="2"><Contents Count="1"><Content ID="0" RefID="2" /></Contents></Pane>
                <Pane ID="3" DockState="Document" ActiveContent="4"><Contents Count="1"><Content ID="0" RefID="4" /></Contents></Pane>
                <Pane ID="4" DockState="Document" ActiveContent="3"><Contents Count="1"><Content ID="0" RefID="3" /></Contents></Pane>
              </Panes>
              <DockWindows>
                <DockWindow ID="0" DockState="Document" ZOrderIndex="0"><NestedPanes Count="3"><Pane ID="0" RefID="0" PrevPane="-1" Alignment="Right" Proportion="0.5" /><Pane ID="1" RefID="3" PrevPane="0" Alignment="Right" Proportion="0.5" /><Pane ID="2" RefID="4" PrevPane="1" Alignment="Bottom" Proportion="0.5" /></NestedPanes></DockWindow>
                <DockWindow ID="1" DockState="DockBottom" ZOrderIndex="1"><NestedPanes Count="1"><Pane ID="0" RefID="1" PrevPane="-1" Alignment="Right" Proportion="0.5" /></NestedPanes></DockWindow>
                <DockWindow ID="2" DockState="DockRight" ZOrderIndex="2"><NestedPanes Count="1"><Pane ID="0" RefID="2" PrevPane="-1" Alignment="Right" Proportion="0.5" /></NestedPanes></DockWindow>
              </DockWindows>
            </DockPanel>
            """;

        public PPDEditorProjectTests()
        {
            session = AppSession.Start(Path.Combine("PPDEditor", "PPDEditor"), "PPDEditor.exe", new AppStartOptions
            {
                ExcludedFiles = ["Projects", "projectcache.dat", "PPDFramework.setting.db", "PPDUpdater.ini", "dockforminfo.xml"],
                MainWindowTitle = "PPDEditor",
                StartupTimeout = TimeSpan.FromMinutes(1),
                Prepare = directory =>
                {
                    workDirectory = directory;
                    File.WriteAllText(Path.Combine(directory, "dockforminfo.xml"), Layout);
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

        static void CopyDirectory(string source, string destination)
        {
            Directory.CreateDirectory(destination);
            foreach (var file in Directory.GetFiles(source))
            {
                File.Copy(file, Path.Combine(destination, Path.GetFileName(file)));
            }
            foreach (var directory in Directory.GetDirectories(source))
            {
                CopyDirectory(directory, Path.Combine(destination, Path.GetFileName(directory)));
            }
        }

        /// <summary>
        /// 表示のメニューの、ドックの項目（並びの番号）を押して、出ていなければ出します。
        /// </summary>
        void ShowDock(int index)
        {
            var showMenu = session.FindMenuItem("ShowMenu");
            showMenu.Expand();
            var item = showMenu.Items[index];
            if (item.Properties.ItemStatus.ValueOrDefault != "True")
            {
                item.Invoke();
            }
            else
            {
                showMenu.Collapse();
            }
        }

        [Fact]
        public void ResourcesShowImageAndPlayEffect()
        {
            CreateProject();
            // プロジェクトの Resource に、画像とエフェクト（エフェクトが使う画像のフォルダーも）を置いて、開き直す
            var resource = Path.Combine(workDirectory, "Projects", ProjectName, "Resource");
            File.Copy(Path.Combine(workDirectory, "img", "PPD", "home", "up.png"), Path.Combine(resource, "Image", "up.png"));
            File.Copy(Path.Combine(workDirectory, "img", "PPD", "home", "loading.etd"), Path.Combine(resource, "Effect", "loading.etd"));
            CopyDirectory(Path.Combine(workDirectory, "img", "PPD", "home", "loading"), Path.Combine(resource, "Effect", "loading"));
            AppSession.InvokeAsync(session.FindMenuItem("fileMenu", "OpenProjectMenuItem"));
            FileDialogs.Accept(session.WaitForFileDialog(), ProjectFile);
            Assert.True(session.WaitForNoModal(), "開けません: " + session.DescribeModalWindows());

            // リソースのドックは、真ん中の右にある
            var tree = session.Find("resourceTree").AsTree();
            // 種類のフォルダーを開く（中が空のものは開けない）
            foreach (var root in tree.Items.Where(i => i.ExpandCollapseState != FlaUI.Core.Definitions.ExpandCollapseState.LeafNode))
            {
                root.Expand();
            }

            // 画像を選ぶと画像を、エフェクトを選ぶとエフェクトを出す
            TreeItem ItemNamed(string name)
            {
                // 名前は項目の中の文字にあるので、親の項目をたどる
                var element = tree.FindFirstDescendant(cf => cf.ByName(name));
                while (element.ControlType != FlaUI.Core.Definitions.ControlType.TreeItem)
                {
                    element = element.Parent;
                }
                return element.AsTreeItem();
            }
            // 探すのはドックの中だけ（PPDEditor のウィンドウ全体を探すと時間がかかる）
            var dock = tree.Parent;
            ItemNamed("up.png").Select();
            Assert.True(AppSession.WaitUntil(() => dock.FindFirstDescendant(cf => cf.ByAutomationId("pictureBox1")) != null), "画像が出ません");
            ItemNamed("loading.etd").Select();
            // エフェクトを描くところ（EffectCanvas）は UI Automation に出ないので、再生のボタンで確かめる
            Assert.True(AppSession.WaitUntil(() => dock.FindFirstDescendant(cf => cf.ByAutomationId("toolStripSplitButton1")) != null), "エフェクトが出ません");

            // 再生して止める。キャンバスの大きさを変える
            var play = dock.FindFirstDescendant(cf => cf.ByAutomationId("toolStripSplitButton1")).AsToggleButton();
            play.Toggle();
            System.Threading.Thread.Sleep(500);
            dock.FindFirstDescendant(cf => cf.ByAutomationId("toolStripButton1")).AsButton().Invoke();
            var size = dock.FindFirstDescendant(cf => cf.ByAutomationId("toolStripComboBox1")).AsComboBox();
            size.Select(2);
            Assert.Equal(2, size.SelectedItem == null ? -1 : Array.IndexOf(size.Items, size.SelectedItem));
            Assert.Empty(session.ModalWindowsOf());
        }

        /// <summary>
        /// レイヤーの並びの n 番目（上から）の真ん中の高さです（1 つのレイヤーの高さは 45）。
        /// </summary>
        static double LayerRow(int index) => 45 * index + 22;

        [Fact]
        public void LayersAddRenameHideDuplicateAndDelete()
        {
            CreateProject();
            var area = session.Find("LayerArea");

            // 余白を右クリックして、名前を付けてレイヤーを足す
            AppSession.InvokeAsync(session.RightClickForPopup(area, 150, 400, "NewLayerMenuItem"));
            var dialog = session.WaitForModal();
            dialog.FindFirstDescendant(cf => cf.ByAutomationId("nameTextBox")).AsTextBox().Text = "added";
            FlaUI.Core.Input.Keyboard.Type(FlaUI.Core.WindowsAPI.VirtualKeyShort.ENTER);
            Assert.True(session.WaitForNoModal(), session.DescribeModalWindows());
            Save();
            Assert.Equal("added", ProjectValue("Base.layer1"));

            // 名前をダブルクリックして変える（後ろのレイヤーほど上に並ぶので、足したレイヤーは上）
            session.DoubleClickAt(area, 80, LayerRow(0));
            FlaUI.Core.Input.Keyboard.TypeSimultaneously(FlaUI.Core.WindowsAPI.VirtualKeyShort.CONTROL, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_A);
            FlaUI.Core.Input.Keyboard.Type("renamed");
            FlaUI.Core.Input.Keyboard.Type(FlaUI.Core.WindowsAPI.VirtualKeyShort.ENTER);
            FlaUI.Core.Input.Wait.UntilInputIsProcessed();

            // 目のところを押すと、表示を切り替える
            session.ClickAt(area, 19, LayerRow(0));
            Save();
            Assert.Equal("renamed", ProjectValue("Base.layer1"));
            Assert.Equal("0", ProjectValue("Base.layer1Visible"));

            // 複製して、消す（確かめるメッセージで OK）
            AppSession.InvokeAsync(session.RightClickForPopup(area, 150, LayerRow(0), "DuplicateLayerMenuItem"));
            Save();
            Assert.NotNull(ProjectValue("Base.layer2"));
            Assert.Equal(3, Directory.GetFiles(Path.GetDirectoryName(LayerFile), "*.ppd").Length);
            AppSession.InvokeAsync(session.RightClickForPopup(area, 150, LayerRow(0), "DeleteLayerMenuItem"));
            MessageBoxes.Press(session.WaitForModal(), MessageBoxButton.Ok);
            Assert.True(session.WaitForNoModal(), session.DescribeModalWindows());
            Save();
            // 消したレイヤーの名前はプロジェクトのファイルに残るが、レイヤーのファイルはなくなる（読むときはファイルのあるところまで）
            Assert.Equal(2, Directory.GetFiles(Path.GetDirectoryName(LayerFile), "*.ppd").Length);
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
            FileDialogs.Accept(session.WaitForFileDialog(), ProjectFile);
            Assert.True(session.WaitForNoModal(), "開けません: " + session.DescribeModalWindows());
            Assert.True(AppSession.WaitUntil(() => session.MainWindow.Title.Contains(ProjectName)), session.MainWindow.Title);
        }
    }
}
