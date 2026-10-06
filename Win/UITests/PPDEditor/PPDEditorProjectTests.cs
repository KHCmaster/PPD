using FlaUI.Core.AutomationElements;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace UITests.PPDEditor
{
    /// <summary>
    /// PPDEditor でプロジェクトを作り、タイムライン、リソース、レイヤーを使って保存し、開き直すテストです。
    /// </summary>
    public sealed class PPDEditorProjectTests() : PPDEditorProjectTestBase("PPDEditor.ResourceManager", "PPDEditor.LayerManager")
    {
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

        [Fact]
        public void ResourcesShowImageAndPlayEffect()
        {
            CreateProject();
            // プロジェクトの Resource に、画像とエフェクト（エフェクトが使う画像のフォルダーも）を置いて、開き直す
            var resource = Path.Combine(workDirectory, "Projects", ProjectName, "Resource");
            File.Copy(Path.Combine(workDirectory, "img", "PPD", "home", "up.png"), Path.Combine(resource, "Image", "up.png"));
            File.Copy(Path.Combine(workDirectory, "img", "PPD", "home", "loading.etd"), Path.Combine(resource, "Effect", "loading.etd"));
            CopyDirectory(Path.Combine(workDirectory, "img", "PPD", "home", "loading"), Path.Combine(resource, "Effect", "loading"));
            ReopenProject();

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
            AppSession.InvokeAsync(session.RightClickForPopup(area, 150, 200, "NewLayerMenuItem"));
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

        static void Press(params FlaUI.Core.WindowsAPI.VirtualKeyShort[] keys)
        {
            FlaUI.Core.Input.Keyboard.TypeSimultaneously(keys);
            FlaUI.Core.Input.Wait.UntilInputIsProcessed();
        }

        [Fact]
        public void TimelineShortcutsAndWheel()
        {
            const FlaUI.Core.WindowsAPI.VirtualKeyShort Ctrl = FlaUI.Core.WindowsAPI.VirtualKeyShort.CONTROL;
            const FlaUI.Core.WindowsAPI.VirtualKeyShort Shift = FlaUI.Core.WindowsAPI.VirtualKeyShort.SHIFT;
            CreateProject();
            SetBeatWidth();
            var timeline = session.Find("Seekmain");
            foreach (var (beat, row) in new[] { (1, 2), (2, 3), (3, 2), (4, 1) })
            {
                session.DoubleClickAt(timeline, Beat(beat), Row(row));
            }
            Save();
            var fourMarks = new FileInfo(LayerFile).Length;

            // 選ぶマークを移す（同じ行の前後、すべての行の前後）
            Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.TAB);
            Press(Shift, FlaUI.Core.WindowsAPI.VirtualKeyShort.TAB);
            Press(Ctrl, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_A);
            Press(Ctrl, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_D);
            // 前のマークの角度と位置をコピーする
            Press(Ctrl, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_R);
            Press(Ctrl, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_Q);
            // 動かす量の違う移動
            foreach (var arrow in new[] { FlaUI.Core.WindowsAPI.VirtualKeyShort.LEFT, FlaUI.Core.WindowsAPI.VirtualKeyShort.RIGHT, FlaUI.Core.WindowsAPI.VirtualKeyShort.UP, FlaUI.Core.WindowsAPI.VirtualKeyShort.DOWN })
            {
                Press(Shift, arrow);
                Press(Ctrl, arrow);
            }
            // 範囲を選んで、まとめて動かす
            session.DragAt(timeline, Beat(0.5), Row(0), Beat(4.5), Row(4));
            Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.LEFT);
            Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.UP);
            // つなぐ・はずす
            Press(Ctrl, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_F);
            Press(Ctrl, Shift, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_F);
            // 元に戻して、やり直す
            Press(Ctrl, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_Z);
            Press(Ctrl, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_Y);

            // ホイールでスクロールし、Shift を押しながらなら表示の幅を変える
            FlaUI.Core.Input.Mouse.Scroll(-3);
            FlaUI.Core.Input.Mouse.Scroll(3);
            FlaUI.Core.Input.Keyboard.Press(Shift);
            try
            {
                FlaUI.Core.Input.Mouse.Scroll(1);
            }
            finally
            {
                FlaUI.Core.Input.Keyboard.Release(Shift);
            }
            FlaUI.Core.Input.Wait.UntilInputIsProcessed();

            // マークは消えていない（数は変わらない）
            Assert.Empty(session.ModalWindowsOf());
            Save();
            Assert.True(new FileInfo(LayerFile).Length >= fourMarks, $"{new FileInfo(LayerFile).Length} >= {fourMarks}");
            Assert.NotEqual(BeatWidth.ToString(System.Globalization.CultureInfo.InvariantCulture), ProjectValue("Base.displaywidth"));
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
            ReopenProject();
            Assert.True(AppSession.WaitUntil(() => session.MainWindow.Title.Contains(ProjectName)), session.MainWindow.Title);
        }
    }
}
