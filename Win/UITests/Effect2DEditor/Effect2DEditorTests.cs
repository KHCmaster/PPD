using FlaUI.Core.AutomationElements;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace UITests.Effect2DEditor
{
    /// <summary>
    /// Effect2DEditor（画像を動かすエフェクトを作るエディター）の画面のテストです。
    /// </summary>
    /// <remarks>
    /// カンバス（DirectX）とタイムライン、ヒストリーは描いて表示するので、中身は調べない。
    /// 元に戻すのメニューを使えるかどうかで、編集の履歴に残ったかを確かめる。
    /// </remarks>
    public sealed class Effect2DEditorTests : IDisposable
    {
        // 使う人の設定（背景の色やカンバスの大きさ）と最近使ったファイルはコピーせず、既定の設定で始める
        readonly AppSession session = AppSession.Start("Effect2DEditor", "Effect2DEditor.exe", "Effect2DEditor.ini", "recentused.ini");

        public void Dispose()
        {
            session.Dispose();
        }

        private bool IsMenuItemEnabled(string menuId, string itemId)
        {
            var menu = session.FindMenuItem(menuId);
            var item = session.FindMenuItem(menuId, itemId);
            var enabled = item.IsEnabled;
            menu.Collapse();
            return enabled;
        }

        /// <summary>
        /// テスト用の画像を作業のフォルダにコピーして、タイムラインの追加のボタンから加えます。
        /// </summary>
        private void AddSampleImage()
        {
            var path = Path.Combine(session.WorkDirectory, "sample.png");
            File.Copy(Path.Combine(AppContext.BaseDirectory, "Effect2DEditor", "TestData", "sample.png"), path, true);
            AppSession.InvokeAsync(session.Find("AddButton"));
            FileDialogs.Accept(session.WaitForModal(), path);
            Assert.True(session.WaitForNoModal());
        }

        private Window OpenDialogFromMenu(string menuId, string itemId)
        {
            AppSession.InvokeAsync(session.FindMenuItem(menuId, itemId));
            return session.WaitForModal();
        }

        [Fact]
        public void Startup_HasNothingToUndo()
        {
            Assert.Equal("60", session.Find("FpsBox").AsTextBox().Text);
            Assert.False(IsMenuItemEnabled("EditMenu", "UndoMenuItem"));
            Assert.False(IsMenuItemEnabled("EditMenu", "RedoMenuItem"));
        }

        [Fact]
        public void AddImage_ThenUndoAndRedo()
        {
            AddSampleImage();
            Assert.True(AppSession.WaitUntil(() => IsMenuItemEnabled("EditMenu", "UndoMenuItem")));

            session.FindMenuItem("EditMenu", "UndoMenuItem").Invoke();
            Assert.True(AppSession.WaitUntil(() => !IsMenuItemEnabled("EditMenu", "UndoMenuItem")));
            Assert.True(IsMenuItemEnabled("EditMenu", "RedoMenuItem"));

            session.FindMenuItem("EditMenu", "RedoMenuItem").Invoke();
            Assert.True(AppSession.WaitUntil(() => IsMenuItemEnabled("EditMenu", "UndoMenuItem")));
        }

        [Fact]
        public void SaveAs_WritesEffectAndCopiesImage()
        {
            AddSampleImage();
            var path = Path.Combine(session.WorkDirectory, "saved.etd");
            AppSession.InvokeAsync(session.FindMenuItem("FileMenu", "SaveAsMenuItem"));
            FileDialogs.Accept(session.WaitForModal(), path);

            Assert.True(AppSession.WaitUntil(() => File.Exists(path)));
            Assert.True(session.WaitForNoModal());
            // 使っている画像は、ファイルと同じ名前のフォルダにコピーする
            Assert.True(AppSession.WaitUntil(() => File.Exists(Path.Combine(session.WorkDirectory, "saved", "sample.png"))));
        }

        [Fact]
        public void CanvasSize_ChangedValueIsShownNextTime()
        {
            var dialog = OpenDialogFromMenu("EditMenu", "CanvasSizeMenuItem");
            dialog.FindFirstDescendant(cf => cf.ByAutomationId("WidthBox")).AsTextBox().Text = "300";
            dialog.FindFirstDescendant(cf => cf.ByAutomationId("HeightBox")).AsTextBox().Text = "200";
            dialog.FindFirstDescendant(cf => cf.ByAutomationId("OkButton")).AsButton().Invoke();
            Assert.True(session.WaitForNoModal());

            dialog = OpenDialogFromMenu("EditMenu", "CanvasSizeMenuItem");

            Assert.Equal("300", dialog.FindFirstDescendant(cf => cf.ByAutomationId("WidthBox")).AsTextBox().Text);
            Assert.Equal("200", dialog.FindFirstDescendant(cf => cf.ByAutomationId("HeightBox")).AsTextBox().Text);
            session.CloseWindow(dialog);
            Assert.True(session.WaitForNoModal());
        }

        [Theory]
        [InlineData("BackGroundMenuItem")]
        [InlineData("AnimationMenuItem")]
        public void EditDialog_OpensAndCancels(string itemId)
        {
            var dialog = OpenDialogFromMenu("EditMenu", itemId);

            Assert.NotEqual("", dialog.Title);
            session.CloseWindow(dialog);
            Assert.True(session.WaitForNoModal());
        }

        /// <summary>
        /// タイムラインの、エフェクト（行、0 から）のフレーム（0 から）のステートをクリックして選びます。
        /// </summary>
        /// <remarks>
        /// タイムラインは描いて表示するので、位置でクリックする（上の 20 が時間の目盛り、1 行が 18、1 フレームが 10）。
        /// </remarks>
        private void SelectState(int frame, int row = 0)
        {
            session.ClickAt(session.Find("timeLine1"), frame * 10 + 5, 20 + 18 * row + 9);
        }

        private string ValueOf(string type)
        {
            return session.Find(type + "Box").AsTextBox().Text;
        }

        [Fact]
        public void SelectState_ShowsValuesInPropertyPanel()
        {
            AddSampleImage();
            Assert.Equal("", ValueOf("X"));

            SelectState(0);

            Assert.True(AppSession.WaitUntil(() => ValueOf("X") == "0"));
            Assert.Equal("1", ValueOf("Alpha"));
            Assert.Equal("1", ValueOf("ScaleX"));
        }

        [Fact]
        public void EditStateValue_ThenUndoAndRedo()
        {
            AddSampleImage();
            SelectState(0);
            Assert.True(AppSession.WaitUntil(() => ValueOf("X") == "0"));

            session.Find("XBox").AsTextBox().Text = "25";
            session.FindMenuItem("EditMenu", "UndoMenuItem").Invoke();
            Assert.True(AppSession.WaitUntil(() => ValueOf("X") == "0"));

            session.FindMenuItem("EditMenu", "RedoMenuItem").Invoke();
            Assert.True(AppSession.WaitUntil(() => ValueOf("X") == "25"));
        }

        [Fact]
        public void EditStateValue_OutOfRangeIsClamped()
        {
            AddSampleImage();
            SelectState(0);
            Assert.True(AppSession.WaitUntil(() => ValueOf("Alpha") == "1"));

            // 透明度は 0 から 1、大きさは 0 以上
            session.Find("AlphaBox").AsTextBox().Text = "5";
            Assert.True(AppSession.WaitUntil(() => ValueOf("Alpha") == "1"));
            session.Find("ScaleXBox").AsTextBox().Text = "-2";
            Assert.True(AppSession.WaitUntil(() => ValueOf("ScaleX") == "0"));
        }

        [Fact]
        public void InsertKeyState_FromTimelineMenu()
        {
            AddSampleImage();

            // 5 フレーム目を右クリックして、キーのステートを入れる
            // （画像を足した直後は、メニューが出ないことがあるので、出るまで右クリックし直す）
            session.RightClickForPopup(session.Find("timeLine1"), 5 * 10 + 5, 20 + 9, "InsertKeyStateMenuItem").AsMenuItem().Invoke();

            SelectState(5);
            Assert.True(AppSession.WaitUntil(() => ValueOf("X") == "0"));
            Assert.True(IsMenuItemEnabled("EditMenu", "UndoMenuItem"));
        }

        /// <summary>
        /// 比率を変えるウィンドウ（枠のないウィンドウ）を待って返します。
        /// </summary>
        /// <remarks>
        /// 右クリックのメニューも別のウィンドウなので、OK のボタンがあるウィンドウを探す。
        /// </remarks>
        private Window WaitForRatioWindow()
        {
            var processId = session.MainWindow.Properties.ProcessId.Value;
            var main = session.MainWindow.Properties.NativeWindowHandle.Value;
            Window found = null;
            Assert.True(AppSession.WaitUntil(() => (found = NativeWindows.OfProcess(processId).Where(h => h != main)
                .Select(h => session.Automation.FromHandle(h).AsWindow())
                .FirstOrDefault(w => w.FindFirstDescendant(cf => cf.ByAutomationId("RatioOkButton")) != null)) != null), "比率のウィンドウが出ません");
            return found;
        }

        /// <summary>
        /// 比率を変えるウィンドウを OK で閉じます。
        /// </summary>
        private void AcceptRatioWindow(Window window)
        {
            var handle = window.Properties.NativeWindowHandle.Value;
            AppSession.InvokeAsync(window.FindFirstDescendant(cf => cf.ByAutomationId("RatioOkButton")));
            Assert.True(AppSession.WaitUntil(() => !NativeWindows.OfProcess(session.MainWindow.Properties.ProcessId.Value).Contains(handle)), "比率のウィンドウが閉じません");
        }

        [Fact]
        public void StatesAreMovedRatioEditedAndDeleted()
        {
            AddSampleImage();
            var timeline = session.Find("timeLine1");

            // 10 フレーム目に普通のステート（前のキーとの間をつなぐ）を入れて、値を変える
            session.RightClickForPopup(timeline, 10 * 10 + 5, 20 + 9, "InsertNormalStateMenuItem").AsMenuItem().Invoke();
            SelectState(10);
            Assert.True(AppSession.WaitUntil(() => ValueOf("X") == "0"));
            session.Find("XBox").AsTextBox().Text = "40";
            Assert.True(AppSession.WaitUntil(() => ValueOf("X") == "40"));

            // 普通のステートの終わりを選ぶと、変化の割合（比率）を変えられる
            var ratio = session.Find("XRatioButton").AsButton();
            Assert.True(AppSession.WaitUntil(() => ratio.IsEnabled), "比率のボタンを押せません");
            AppSession.InvokeAsync(ratio);
            var window = WaitForRatioWindow();
            session.DragAt(window, 26 + 40, 12 + 90, 26 + 60, 12 + 30);
            window.FindFirstDescendant(cf => cf.ByAutomationId("RatioCopyButton")).AsButton().Invoke();
            window.FindFirstDescendant(cf => cf.ByAutomationId("RatioLinearButton")).AsButton().Invoke();
            window.FindFirstDescendant(cf => cf.ByAutomationId("RatioPasteButton")).AsButton().Invoke();
            AcceptRatioWindow(window);

            // ステートをドラッグで動かす（Ctrl を押すと、後ろのステートも一緒に動く）
            session.DragAt(timeline, 10 * 10 + 5, 20 + 9, 15 * 10 + 5, 20 + 9);
            SelectState(15);
            Assert.True(AppSession.WaitUntil(() => ValueOf("X") == "40"), "ステートが動きません");
            FlaUI.Core.Input.Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.CONTROL);
            try
            {
                session.DragAt(timeline, 15 * 10 + 5, 20 + 9, 20 * 10 + 5, 20 + 9);
            }
            finally
            {
                FlaUI.Core.Input.Keyboard.Release(FlaUI.Core.WindowsAPI.VirtualKeyShort.CONTROL);
            }
            SelectState(20);
            Assert.True(AppSession.WaitUntil(() => ValueOf("X") == "40"), "ステートが動きません");

            // 上の時間の目盛りを押すと、今のフレームが変わる
            session.ClickAt(timeline, 7 * 10 + 5, 10);

            // ステートを消す
            session.RightClickForPopup(timeline, 20 * 10 + 5, 20 + 9, "DeleteStateMenuItem").AsMenuItem().Invoke();
            Assert.True(IsMenuItemEnabled("EditMenu", "UndoMenuItem"));
        }

        private static bool IsNear(string text, float expected)
        {
            return float.TryParse(text, System.Globalization.CultureInfo.InvariantCulture, out var value) && Math.Abs(value - expected) <= 2;
        }

        /// <summary>
        /// Ctrl を押したまま、マウスの左ボタンでドラッグします。
        /// </summary>
        private void ControlDragAt(AutomationElement element, double fromX, double fromY, double toX, double toY)
        {
            FlaUI.Core.Input.Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.CONTROL);
            try
            {
                session.DragAt(element, fromX, fromY, toX, toY);
            }
            finally
            {
                FlaUI.Core.Input.Keyboard.Release(FlaUI.Core.WindowsAPI.VirtualKeyShort.CONTROL);
            }
        }

        [Fact]
        public void CanvasBezierIsEditedTransformedAndReset()
        {
            // カンバスがドックに収まって、真ん中に置かれる大きさにする
            var dialog = OpenDialogFromMenu("EditMenu", "CanvasSizeMenuItem");
            dialog.FindFirstDescendant(cf => cf.ByAutomationId("WidthBox")).AsTextBox().Text = "300";
            dialog.FindFirstDescendant(cf => cf.ByAutomationId("HeightBox")).AsTextBox().Text = "200";
            dialog.FindFirstDescendant(cf => cf.ByAutomationId("OkButton")).AsButton().Invoke();
            Assert.True(session.WaitForNoModal());

            AddSampleImage();
            var timeline = session.Find("timeLine1");
            session.RightClickForPopup(timeline, 10 * 10 + 5, 20 + 9, "InsertNormalStateMenuItem").AsMenuItem().Invoke();
            SelectState(10);
            Assert.True(AppSession.WaitUntil(() => ValueOf("X") == "0"));
            session.Find("XBox").AsTextBox().Text = "40";
            Assert.True(AppSession.WaitUntil(() => ValueOf("X") == "40"));
            // 選び直すと、カンバスに始めと終わりの位置が点で出る
            SelectState(10);

            // カンバスは描いて表示するので、位置で操作する（エフェクトの原点がカンバスの真ん中）
            var canvas = session.Find("scrollViewer");
            var scale = NativeWindows.GetScale(session.MainWindow.Properties.NativeWindowHandle.Value);
            var cx = canvas.BoundingRectangle.Width / scale / 2;
            var cy = canvas.BoundingRectangle.Height / scale / 2;

            // 終わりの点を Ctrl を押したまま動かすと、位置がベジエで変わるようになる
            ControlDragAt(canvas, cx + 40, cy, cx + 40, cy + 30);
            // プロパティの欄は、ステートを選び直すと変わる（ベジエの位置は、数字では変えられない）
            SelectState(0);
            SelectState(10);
            // マウスの位置は画面の点の単位なので、少しずれることがある
            Assert.True(AppSession.WaitUntil(() => IsNear(ValueOf("Y"), 30)), "点が動きません: " + ValueOf("Y"));
            Assert.True(IsNear(ValueOf("X"), 40), ValueOf("X"));
            Assert.True(AppSession.WaitUntil(() => !session.Find("XBox").IsEnabled), "位置がベジエになりません");
            var bezierButton = session.Find("BezierPositionButton").AsButton();
            Assert.True(AppSession.WaitUntil(() => bezierButton.IsEnabled));

            // 線の上をクリックすると点が増え、メニューから消せる
            session.ClickAt(canvas, cx + 20, cy + 15);
            session.RightClickForPopup(canvas, cx + 20, cy + 15, "deleteAnchorMenuItem").AsMenuItem().Invoke();

            // 変形を始めて、回して、終える
            session.RightClickForPopup(canvas, cx, cy, "startTransformMenuItem").AsMenuItem().Invoke();
            session.DragAt(canvas, cx + 60, cy + 50, cx + 30, cy + 60);
            session.RightClickForPopup(canvas, cx, cy, "endTransformMenuItem").AsMenuItem().Invoke();

            // 位置のベジエの比率を変える
            AppSession.InvokeAsync(bezierButton);
            var window = WaitForRatioWindow();
            AcceptRatioWindow(window);

            // 元に戻すと、ベジエでない位置の変化になる
            session.RightClickForPopup(canvas, cx, cy, "resetMenuItem").AsMenuItem().Invoke();
            Assert.True(AppSession.WaitUntil(() => session.Find("XBox").IsEnabled), "ベジエが元に戻りません");
            Assert.True(IsMenuItemEnabled("EditMenu", "UndoMenuItem"));
        }

        [Fact]
        public void EffectsAreCopiedSwappedDeletedAndCleared()
        {
            AddSampleImage();
            SelectState(0);
            Assert.True(AppSession.WaitUntil(() => ValueOf("X") == "0"));

            // 選んだエフェクトをコピーして、元のほうの値を変える
            session.Find("CopyEffectButton").AsButton().Invoke();
            SelectState(0);
            session.Find("XBox").AsTextBox().Text = "25";
            Assert.True(AppSession.WaitUntil(() => ValueOf("X") == "25"));
            SelectState(0, 1);
            Assert.True(AppSession.WaitUntil(() => ValueOf("X") == "0"), "コピーしたエフェクトがありません");

            // 並びを逆にする
            session.Find("SwapButton").AsButton().Invoke();
            SelectState(0, 1);
            Assert.True(AppSession.WaitUntil(() => ValueOf("X") == "25"), "並びが変わりません");
            SelectState(0);
            Assert.True(AppSession.WaitUntil(() => ValueOf("X") == "0"));

            // 選んだエフェクトを消すと、残りが上に来る
            session.Find("DeleteButton").AsButton().Invoke();
            SelectState(0);
            Assert.True(AppSession.WaitUntil(() => ValueOf("X") == "25"), "エフェクトが消えません");

            // すべて消して、元に戻す
            session.Find("ClearEffectButton").AsButton().Invoke();
            session.FindMenuItem("EditMenu", "UndoMenuItem").Invoke();
            SelectState(0);
            Assert.True(AppSession.WaitUntil(() => ValueOf("X") == "25"), "元に戻りません");
        }

        [Fact]
        public void HistoryCheckBoxes_UndoAndRedo()
        {
            AddSampleImage();
            SelectState(0);
            session.Find("CopyEffectButton").AsButton().Invoke();
            Assert.True(AppSession.WaitUntil(() => IsMenuItemEnabled("EditMenu", "UndoMenuItem")));

            // ヒストリーは描いて表示するので、位置でクリックする（1 行が 20、左のチェックの枠が 3 から 17）
            var history = session.Find("historyPanel1");
            session.ClickAt(history, 10, 10);
            Assert.True(AppSession.WaitUntil(() => !IsMenuItemEnabled("EditMenu", "UndoMenuItem")), "最初まで戻りません");
            Assert.True(IsMenuItemEnabled("EditMenu", "RedoMenuItem"));

            // 消えている行のチェックを押すと、そこまでやり直す（ステートを選んだのも履歴に残るので、まだ後がある）
            session.ClickAt(history, 10, 10);
            Assert.True(AppSession.WaitUntil(() => IsMenuItemEnabled("EditMenu", "UndoMenuItem")), "やり直せません");
            Assert.True(IsMenuItemEnabled("EditMenu", "RedoMenuItem"));
        }

        /// <summary>
        /// 再生のしかたを選びます（選ぶと再生する）。
        /// </summary>
        private void SelectPlayType(string itemId)
        {
            session.Find("playTypeButton").AsButton().Invoke();
            session.FindInPopups(itemId).AsMenuItem().Invoke();
        }

        [Fact]
        public void PlayTypes_PlayPauseAndStop()
        {
            AddSampleImage();
            session.RightClickForPopup(session.Find("timeLine1"), 10 * 10 + 5, 20 + 9, "InsertKeyStateMenuItem").AsMenuItem().Invoke();
            var play = session.Find("PlayButton").AsToggleButton();
            bool IsPlaying() => play.ToggleState == FlaUI.Core.Definitions.ToggleState.On;

            // 繰り返すときは、止めるまで再生している
            SelectPlayType("PlayLoopMenuItem");
            Assert.True(AppSession.WaitUntil(IsPlaying));
            play.Toggle();
            Assert.True(AppSession.WaitUntil(() => !IsPlaying()), "一時停止しません");
            play.Toggle();
            Assert.True(AppSession.WaitUntil(IsPlaying));
            session.Find("StopButton").AsButton().Invoke();
            Assert.True(AppSession.WaitUntil(() => !IsPlaying()), "止まりません");

            SelectPlayType("PlayReverseLoopMenuItem");
            Assert.True(AppSession.WaitUntil(IsPlaying));
            session.Find("StopButton").AsButton().Invoke();
            Assert.True(AppSession.WaitUntil(() => !IsPlaying()), "止まりません");

            // 1 回だけのときは、終わると止まる
            SelectPlayType("PlayOnceMenuItem");
            Assert.True(AppSession.WaitUntil(() => !IsPlaying()), "終わっても止まりません");
            SelectPlayType("PlayOnceReverseMenuItem");
            Assert.True(AppSession.WaitUntil(() => !IsPlaying()), "終わっても止まりません");
        }

        private static AutomationElement In(Window window, string automationId)
        {
            return window.FindFirstDescendant(cf => cf.ByAutomationId(automationId));
        }

        [Fact]
        public void BackGround_ColorAndImageAreKept()
        {
            // 色を選ぶと、色の背景になる
            var dialog = OpenDialogFromMenu("EditMenu", "BackGroundMenuItem");
            AppSession.InvokeAsync(In(dialog, "PickColorButton"));
            var colorWindow = session.WaitForModal(dialog);
            In(colorWindow, "ColorOkButton").AsButton().Invoke();
            Assert.True(AppSession.WaitUntil(() => In(dialog, "BackColorRadio").AsRadioButton().IsChecked));
            In(dialog, "BackOkButton").AsButton().Invoke();
            Assert.True(session.WaitForNoModal());

            // 次に開いたときも色のまま。画像を選ぶと、画像の背景になる
            var image = Path.Combine(session.WorkDirectory, "back.png");
            File.Copy(Path.Combine(AppContext.BaseDirectory, "Effect2DEditor", "TestData", "sample.png"), image, true);
            dialog = OpenDialogFromMenu("EditMenu", "BackGroundMenuItem");
            Assert.True(In(dialog, "BackColorRadio").AsRadioButton().IsChecked);
            AppSession.InvokeAsync(In(dialog, "PickImageButton"));
            FileDialogs.Accept(session.WaitForFileDialog(dialog), image);
            Assert.True(AppSession.WaitUntil(() => In(dialog, "BackImageRadio").AsRadioButton().IsChecked));
            Assert.Equal(image, In(dialog, "BackFileNameBox").AsTextBox().Text);
            In(dialog, "BackOkButton").AsButton().Invoke();
            Assert.True(session.WaitForNoModal());
            AddSampleImage();

            // 市松模様に戻す
            dialog = OpenDialogFromMenu("EditMenu", "BackGroundMenuItem");
            Assert.True(In(dialog, "BackImageRadio").AsRadioButton().IsChecked);
            In(dialog, "BackDefaultRadio").AsRadioButton().IsChecked = true;
            In(dialog, "BackOkButton").AsButton().Invoke();
            Assert.True(session.WaitForNoModal());
            dialog = OpenDialogFromMenu("EditMenu", "BackGroundMenuItem");
            Assert.True(In(dialog, "BackDefaultRadio").AsRadioButton().IsChecked);
            session.CloseWindow(dialog);
            Assert.True(session.WaitForNoModal());
        }

        [Fact]
        public void EffectRows_AreReorderedByDragging()
        {
            AddSampleImage();
            SelectState(0);
            session.Find("CopyEffectButton").AsButton().Invoke();
            SelectState(0);
            session.Find("XBox").AsTextBox().Text = "25";
            Assert.True(AppSession.WaitUntil(() => ValueOf("X") == "25"));

            // 名前の列（タイムラインの左。上の 20 が目盛り、1 行が 18）で、1 行目を 2 行目の下までドラッグする
            // （名前の列は UI Automation で見えないので、タイムラインからの位置で操作する）
            var timeline = session.Find("timeLine1");
            const int names = -100;
            session.DragAt(timeline, names, 20 + 9, names, 20 + 18 * 2 + 4);
            SelectState(0, 1);
            Assert.True(AppSession.WaitUntil(() => ValueOf("X") == "25"), "並びが変わりません");
            SelectState(0);
            Assert.True(AppSession.WaitUntil(() => ValueOf("X") == "0"));

            // 2 行目を上の端までドラッグして戻す
            session.DragAt(timeline, names, 20 + 18 + 9, names, 20 + 2);
            SelectState(0);
            Assert.True(AppSession.WaitUntil(() => ValueOf("X") == "25"), "並びが戻りません");
        }

        /// <summary>
        /// 並んだエフェクトを、順に表示するアニメーションにします（確かめのメッセージに答える）。
        /// </summary>
        private void CreateAnimation(int frameCount, bool withReverse, bool accept)
        {
            var dialog = OpenDialogFromMenu("EditMenu", "AnimationMenuItem");
            In(dialog, "FrameCountBox").AsTextBox().Text = frameCount.ToString();
            In(dialog, "WithReverseCheck").AsCheckBox().IsChecked = withReverse;
            AppSession.InvokeAsync(In(dialog, "FrameCountOkButton"));
            Assert.True(AppSession.WaitUntil(() => session.ModalWindowsOf().FirstOrDefault()?.ClassName == "#32770"), "確かめのメッセージが出ません");
            MessageBoxes.Press(session.ModalWindowsOf().First(), accept ? MessageBoxButton.Yes : MessageBoxButton.No);
            Assert.True(session.WaitForNoModal());
        }

        [Fact]
        public void Animation_ShowsEffectsInTurn()
        {
            AddSampleImage();
            SelectState(0);
            session.Find("CopyEffectButton").AsButton().Invoke();

            // やめたときは、何も変わらない
            CreateAnimation(5, false, false);
            Assert.True(IsMenuItemEnabled("EditMenu", "UndoMenuItem"));

            // 1 行目は 5 フレームで消える（作り直したので、履歴は消える）
            CreateAnimation(5, false, true);
            Assert.False(IsMenuItemEnabled("EditMenu", "UndoMenuItem"));
            SelectState(5);
            Assert.True(AppSession.WaitUntil(() => ValueOf("Alpha") == "0"), "アニメーションになりません");

            // 逆にも再生するときは、1 行目は 10 フレームでまた出る
            CreateAnimation(5, true, true);
            SelectState(10);
            Assert.True(AppSession.WaitUntil(() => ValueOf("Alpha") == "1"), "逆の再生になりません");
        }

        [Fact]
        public void Overwrite_ThenOpenRecentFile()
        {
            AddSampleImage();
            var path = Path.Combine(session.WorkDirectory, "saved.etd");
            AppSession.InvokeAsync(session.FindMenuItem("FileMenu", "SaveAsMenuItem"));
            FileDialogs.Accept(session.WaitForModal(), path);
            Assert.True(AppSession.WaitUntil(() => File.Exists(path)));
            Assert.True(session.WaitForNoModal());
            var saved = File.ReadAllBytes(path);

            // 値を変えて、上書きする
            SelectState(0);
            session.Find("XBox").AsTextBox().Text = "25";
            Assert.True(AppSession.WaitUntil(() => ValueOf("X") == "25"));
            session.FindMenuItem("FileMenu", "OverwriteMenuItem").Invoke();
            Assert.True(AppSession.WaitUntil(() => !File.ReadAllBytes(path).SequenceEqual(saved)), "上書きされません");

            // 開いたファイルは最近使ったファイルに残るので、新しくしてから、そこから開き直す
            AppSession.InvokeAsync(session.FindMenuItem("FileMenu", "OpenMenuItem"));
            FileDialogs.Accept(session.WaitForFileDialog(), path);
            Assert.True(session.WaitForNoModal());
            session.FindMenuItem("FileMenu", "NewMenuItem").Invoke();
            Assert.Empty(session.ModalWindowsOf());
            var recent = session.FindMenuItem("FileMenu", "RecentFilesMenu");
            recent.Expand();
            var item = recent.Items.First();
            Assert.Contains("saved", item.Name);
            item.Invoke();
            SelectState(0);
            Assert.True(AppSession.WaitUntil(() => ValueOf("X") == "25"), "開き直せません");
        }

        [Theory]
        [InlineData("ShowHistoryMenuItem", "historyPanel1")]
        [InlineData("ShowPropertyMenuItem", "BlendCombo")]
        [InlineData("ShowTimeLineMenuItem", "FpsBox")]
        [InlineData("ShowCanvasMenuItem", "scrollViewer")]
        public void ShowMenu_HidesAndShowsDock(string itemId, string contentId)
        {
            bool IsShown() => session.MainWindow.FindFirstDescendant(cf => cf.ByAutomationId(contentId)) != null;
            Assert.True(IsShown());

            // 前に出ていないドックは、1 回目は前に出すだけで、2 回目で隠れる
            for (var i = 0; i < 2 && IsShown(); i++)
            {
                session.FindMenuItem("ShowMenu", itemId).Invoke();
                AppSession.WaitUntil(() => !IsShown());
            }
            Assert.False(IsShown(), "隠れません");
            session.FindMenuItem("ShowMenu", itemId).Invoke();
            Assert.True(AppSession.WaitUntil(IsShown), "出ません");
        }

        [Fact]
        public void BlendMode_IsChangedAndKept()
        {
            AddSampleImage();
            SelectState(0);
            Assert.True(AppSession.WaitUntil(() => ValueOf("X") == "0"));
            var blend = session.Find("BlendCombo").AsComboBox();
            var before = blend.SelectedItem?.Text;

            blend.Expand();
            blend.Select(1);
            blend.Collapse();
            var changed = blend.SelectedItem.Text;
            Assert.NotEqual(before, changed);

            // ほかのステートを選んでから戻っても、変えたまま
            session.RightClickForPopup(session.Find("timeLine1"), 5 * 10 + 5, 20 + 9, "InsertKeyStateMenuItem").AsMenuItem().Invoke();
            SelectState(5);
            SelectState(0);
            Assert.True(AppSession.WaitUntil(() => session.Find("BlendCombo").AsComboBox().SelectedItem?.Text == changed), "ブレンドが残りません");
        }

        [Fact]
        public void Exit_WithoutChanges_SavesSettingsAndCloses()
        {
            session.FindMenuItem("FileMenu", "ExitMenuItem").Invoke();

            Assert.True(session.WaitForExit());
            // 閉じるときに、設定をファイルに残す
            Assert.True(File.Exists(Path.Combine(session.WorkDirectory, "Effect2DEditor.ini")));
        }

        [Fact]
        public void ValueLabel_DraggingChangesValue()
        {
            AddSampleImage();
            SelectState(0);
            Assert.True(AppSession.WaitUntil(() => ValueOf("X") == "0"));

            // 名前のラベル（入力欄の左。右に寄せて、間が 6）を右にドラッグすると増え、左にドラッグすると減る
            var box = session.Find("XBox");
            session.DragAt(box, -10, 10, 30, 10);
            Assert.True(AppSession.WaitUntil(() => float.TryParse(ValueOf("X"), System.Globalization.CultureInfo.InvariantCulture, out var x) && x >= 20), "増えません: " + ValueOf("X"));
            session.DragAt(box, -10, 10, -100, 10);
            Assert.True(AppSession.WaitUntil(() => float.TryParse(ValueOf("X"), System.Globalization.CultureInfo.InvariantCulture, out var x) && x < 0), "減りません: " + ValueOf("X"));
            Assert.True(IsMenuItemEnabled("EditMenu", "UndoMenuItem"));
        }

        [Fact]
        public void SaveAndOpen_RestoresEffect()
        {
            AddSampleImage();
            var path = Path.Combine(session.WorkDirectory, "saved.etd");
            AppSession.InvokeAsync(session.FindMenuItem("FileMenu", "SaveAsMenuItem"));
            FileDialogs.Accept(session.WaitForModal(), path);
            Assert.True(AppSession.WaitUntil(() => File.Exists(path)));
            Assert.True(session.WaitForNoModal());

            // 新しく作ってから（保存してあるので、保存するかは聞かない）、保存したファイルを開く
            session.FindMenuItem("FileMenu", "NewMenuItem").Invoke();
            Assert.True(AppSession.WaitUntil(() => !IsMenuItemEnabled("EditMenu", "UndoMenuItem")));
            AppSession.InvokeAsync(session.FindMenuItem("FileMenu", "OpenMenuItem"));
            FileDialogs.Accept(session.WaitForModal(), path);
            Assert.True(session.WaitForNoModal());

            SelectState(0);
            Assert.True(AppSession.WaitUntil(() => ValueOf("X") == "0"));
        }

        [Fact]
        public void New_WithUnsavedChanges_AsksToSave()
        {
            AddSampleImage();
            AppSession.InvokeAsync(session.FindMenuItem("FileMenu", "NewMenuItem"));
            var ask = session.WaitForModal();

            // キャンセルすると、今のエフェクトのまま
            MessageBoxes.Press(ask, MessageBoxButton.Cancel);
            Assert.True(session.WaitForNoModal());
            Assert.True(IsMenuItemEnabled("EditMenu", "UndoMenuItem"));

            // 保存しないで新しくする
            AppSession.InvokeAsync(session.FindMenuItem("FileMenu", "NewMenuItem"));
            MessageBoxes.Press(session.WaitForModal(), MessageBoxButton.No);
            Assert.True(session.WaitForNoModal());
            Assert.True(AppSession.WaitUntil(() => !IsMenuItemEnabled("EditMenu", "UndoMenuItem")));
        }

        [Fact]
        public void Language_SwitchToEnglish_ChangesTexts()
        {
            var menu = session.FindMenuItem("LanguageMenu");
            menu.Expand();
            menu.Items.First(i => i.Name == "English").Invoke();

            Assert.True(AppSession.WaitUntil(() => session.Find("FileMenu").Name == "File"));
            Assert.Equal("Change canvas size", session.FindMenuItem("EditMenu", "CanvasSizeMenuItem").Name);
        }
    }
}
