using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using System;
using System.Drawing;
using System.Threading;
using Xunit;

namespace UITests.BezierDrawControlTest
{
    /// <summary>
    /// BezierDrawControlTest（ベジエの曲線を描くコントロールのテスト用のアプリ）の画面のテストです。
    /// </summary>
    /// <remarks>
    /// 曲線は DirectX で描くので UI Automation では中身を調べられない。画面を撮って、描く前と後を比べる。
    /// マウスとキーボードを実際に動かすので、テストの間は操作しないこと。
    /// </remarks>
    public sealed class BezierDrawControlTestTests : IDisposable
    {
        readonly AppSession session = AppSession.Start("BezierDrawControlTest", "BezierDrawControlTest.exe");

        public void Dispose()
        {
            session.Dispose();
        }

        /// <summary>
        /// 描くところ（ウィンドウの枠とタイトルバーを除いた内側）の、画面の範囲です。
        /// </summary>
        private Rectangle CanvasArea
        {
            get
            {
                var bounds = session.MainWindow.BoundingRectangle;
                const int margin = 80;
                return new Rectangle(bounds.X + margin, bounds.Y + margin, bounds.Width - margin * 2, bounds.Height - margin * 2);
            }
        }

        /// <summary>
        /// ウィンドウを前に出して、描き終わるのを待ってから撮ります。
        /// </summary>
        private Bitmap TakeCanvas()
        {
            session.MainWindow.SetForeground();
            // マウスのカーソルや、カーソルの下の点の強調が写らないように、タイトルバーに動かしておく
            var bounds = session.MainWindow.BoundingRectangle;
            Mouse.Position = new Point(bounds.X + bounds.Width / 2, bounds.Y + 10);
            Thread.Sleep(500);
            return Screenshots.Take(CanvasArea);
        }

        private static bool IsDark(Color color)
        {
            return color.R < 100 && color.G < 100 && color.B < 100;
        }

        [Fact]
        public void Canvas_DrawsGrid()
        {
            using var canvas = TakeCanvas();

            // 白い背景に黒い格子の線が描かれている
            var dark = Screenshots.CountPixels(canvas, IsDark);
            var white = Screenshots.CountPixels(canvas, c => c.R > 240 && c.G > 240 && c.B > 240);
            Assert.True(dark > canvas.Width, "格子の線が描かれていません。");
            Assert.True(white > canvas.Width * canvas.Height / 2, "背景が白くありません。");
        }

        [Fact]
        public void AddAnchors_ThenUndo_RestoresCanvas()
        {
            using var before = TakeCanvas();
            var area = CanvasArea;
            // 2 つのアンカーを置いて、線を引く
            Mouse.Click(new Point(area.X + area.Width / 3, area.Y + area.Height / 2));
            Thread.Sleep(200);
            Mouse.Click(new Point(area.X + area.Width * 2 / 3, area.Y + area.Height / 3));
            Thread.Sleep(300);
            using var drawn = TakeCanvas();
            Assert.True(Screenshots.CountDifferentPixels(before, drawn) > 50, "アンカーを置いても描かれていません。");

            // 元に戻す（アンカーごとに 1 つずつ戻る）
            for (int i = 0; i < 2; i++)
            {
                Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_Z);
                Thread.Sleep(200);
            }
            using var undone = TakeCanvas();
            Assert.True(Screenshots.CountDifferentPixels(before, undone) < 20, "元に戻しても、描く前と同じになりません。");
        }
    }
}
