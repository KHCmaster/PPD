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
        readonly AppSession session = AppSession.Start("Effect2DEditor", "Effect2DEditor.exe");

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
        /// タイムラインの、1 つ目のエフェクトのフレーム（0 から）のステートをクリックして選びます。
        /// </summary>
        /// <remarks>
        /// タイムラインは描いて表示するので、位置でクリックする（上の 20 が時間の目盛り、1 行が 18、1 フレームが 10）。
        /// </remarks>
        private void SelectState(int frame)
        {
            session.ClickAt(session.Find("timeLine1"), frame * 10 + 5, 20 + 9);
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
        /// メイン以外の、このアプリのウィンドウ（枠のないウィンドウも含む）を待って返します。
        /// </summary>
        private Window WaitForOtherWindow()
        {
            var processId = session.MainWindow.Properties.ProcessId.Value;
            var main = session.MainWindow.Properties.NativeWindowHandle.Value;
            IntPtr found = IntPtr.Zero;
            Assert.True(AppSession.WaitUntil(() => (found = NativeWindows.OfProcess(processId).FirstOrDefault(h => h != main)) != IntPtr.Zero), "ウィンドウが出ません");
            return session.Automation.FromHandle(found).AsWindow();
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
            var window = WaitForOtherWindow();
            session.DragAt(window, 26 + 40, 12 + 90, 26 + 60, 12 + 30);
            window.FindFirstDescendant(cf => cf.ByAutomationId("RatioCopyButton")).AsButton().Invoke();
            window.FindFirstDescendant(cf => cf.ByAutomationId("RatioLinearButton")).AsButton().Invoke();
            window.FindFirstDescendant(cf => cf.ByAutomationId("RatioPasteButton")).AsButton().Invoke();
            var handle = window.Properties.NativeWindowHandle.Value;
            AppSession.InvokeAsync(window.FindFirstDescendant(cf => cf.ByAutomationId("RatioOkButton")));
            Assert.True(AppSession.WaitUntil(() => !NativeWindows.OfProcess(session.MainWindow.Properties.ProcessId.Value).Contains(handle)), "比率のウィンドウが閉じません");

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
