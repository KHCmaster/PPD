using PPDEditor;
using PPDFramework;
using System.Drawing;
using System.IO;
using System.Reflection;
using Xunit;

namespace UnitTests.PPDEditor
{
    /// <summary>
    /// エディターのスキン（defaultskin.ini のマークの画像、skin.ini の色）を読むテストです。
    /// </summary>
    /// <remarks>
    /// 全体のスキン（PPDEditorSkin.Skin）は変えず、一時フォルダーで別に作る。
    /// </remarks>
    public class EditorSkinTests
    {
        // 同梱の defaultskin.ini と同じ形
        const string DefaultSkin = """
            [Square]button\sikaku.png
            [Cross]button\batu.png
            [Circle]button\maru.png
            [Triangle]button\sankaku.png
            [Left]button\hidari.png
            [Down]button\sita.png
            [Right]button\migi.png
            [Up]button\ue.png
            [R]button\R.png
            [L]button\L.png
            [ColorSquare]button\sikakuc.png
            [ColorCross]button\batuc.png
            [ColorCircle]button\maruc.png
            [ColorTriangle]button\sankakuc.png
            [ColorLeft]button\newhidaric.png
            [ColorDown]button\newsitac.png
            [ColorRight]button\newmigic.png
            [ColorUp]button\newuec.png
            [ColorR]button\Rc.png
            [ColorL]button\newLc.png
            [TraceSquare]button\sikakut.png
            [TraceCross]button\batut.png
            [TraceCircle]button\marut.png
            [TraceTriangle]button\sankakut.png
            [TraceLeft]button\sikakut.png
            [TraceDown]button\batut.png
            [TraceRight]button\marut.png
            [TraceUp]button\sankakut.png
            [TraceR]button\Rt.png
            [TraceL]button\Lt.png
            [ClockAxis]button\jiku.png
            [LongNoteCirle]button\outcircle.png
            [CircleAxis]button\outjiku.png
            [InnerRadius]37
            [OuterRadius]48
            [EffectCool]button\cooleffect.etd
            [EffectFine]button\fineeffect.etd
            [EffectSafe]button\safeeffect.etd
            [EffectSad]button\sadeffect.etd
            [EffectWorst]
            [EffectAppear]button\uzueffect.etd
            [Hold]button\hold.png
            [HoldX]0
            [HoldY]23
            [SliderRight]button\slider_right.png
            [SliderLeft]button\slider_left.png
            [SliderRightExtra]button\slider_right_ex.png
            [SliderLeftExtra]button\slider_left_ex.png
            [ColorSliderRight]button\slider_right_c.png
            [ColorSliderLeft]button\slider_left_c.png
            [ColorSliderRightExtra]button\slider_right_ex_c.png
            [ColorSliderLeftExtra]button\slider_left_ex_c.png
            [TraceSlider]button\slidert.png
            [EffectSlide]button\slideeffect.etd
            """;

        static PPDEditorSkin NewSkin()
        {
            var constructor = typeof(PPDEditorSkin).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, System.Type.EmptyTypes);
            return (PPDEditorSkin)constructor.Invoke(null);
        }

        static string Image(string name) => Path.Combine("img", "PPDEditor", "button", name);

        [Fact]
        public void DefaultSkin_GivesMarkImages()
        {
            using var folder = new TempFolder(setCurrentDirectory: true);
            folder.Write("defaultskin.ini", DefaultSkin);

            var skin = NewSkin();

            Assert.Equal(Image("maru.png"), skin.GetMarkImagePath(ButtonType.Circle).ToString());
            Assert.Equal(Image("maruc.png"), skin.GetMarkColorImagePath(ButtonType.Circle).ToString());
            Assert.Equal(Image("Lt.png"), skin.GetTraceImagePath(ButtonType.L).ToString());
            // スライドの画像は Start から後ろ
            Assert.Equal(Image("slider_left_ex.png"), skin.GetMarkImagePath(ButtonType.Start + 3).ToString());
            Assert.Equal(Image("slider_right_c.png"), skin.GetMarkColorImagePath(ButtonType.Start).ToString());
            Assert.Equal(Image("slidert.png"), skin.GetTraceImagePath(ButtonType.Start + 2).ToString());
            skin.GetLongNoteCircleInfo(out var circle, out var inner, out var outer);
            Assert.Equal((Image("outcircle.png"), 37f, 48f), (circle.ToString(), inner, outer));
            Assert.Equal(Image("jiku.png"), skin.GetClockAxisImagePath().ToString());
            Assert.Equal(Image("outjiku.png"), skin.GetCircleAxisImagePath().ToString());
            Assert.Equal(Image("cooleffect.etd"), skin.GetEvaluateEffectPath(MarkEvaluateType.Cool).ToString());
            Assert.Equal(Image("uzueffect.etd"), skin.GetAppearEffectPath().ToString());
            skin.GetHoldInfo(out var hold, out var x, out var y);
            Assert.Equal((Image("hold.png"), 0f, 23f), (hold.ToString(), x, y));
        }

        [Fact]
        public void Theme_SetsColorsAndTimeLine()
        {
            using var folder = new TempFolder(setCurrentDirectory: true);
            var skin = NewSkin();
            var holdColor = skin.TimeLineHoldExtentColor;
            folder.Write("skin.ini", """
                [BackGround]none.png
                [DisplayMode]2
                [FillColor]1,2,3
                [OverLayColor]10, 20, 30, 40
                [BorderColor]bad
                [TimeLineBackSquare]5,6,7
                [TimeLineCircleImagePath]small_circle.png
                [TimeLineHeight]30
                [TimeLineRowHeight]x
                """);

            skin.Initialize("skin.ini");

            Assert.Null(skin.BackGround);
            Assert.Equal(PPDEditorSkin.BackGroundDisplayMode.center, skin.DisplayMode);
            Assert.Equal(Color.FromArgb(1, 2, 3).ToArgb(), skin.FillColor.ToArgb());
            // 4 つの数は不透明度から
            Assert.Equal(Color.FromArgb(10, 20, 30, 40).ToArgb(), skin.OverLayColor.ToArgb());
            // 読めない色はコントロールの色。時間軸の長押しの色だけは元のまま
            Assert.Equal(SystemColors.Control.ToArgb(), skin.BorderColor.ToArgb());
            Assert.Equal(holdColor, skin.TimeLineHoldExtentColor);
            Assert.Equal(Color.FromArgb(5, 6, 7).ToArgb(), ((SolidBrush)skin.TimeLineBackGroundColors[0]).Color.ToArgb());
            Assert.Equal(Color.White.ToArgb(), ((SolidBrush)skin.TimeLineBackGroundColors[1]).Color.ToArgb());
            Assert.Equal("small_circle.png", skin.GetSmallMarkColorImagePath(ButtonType.Circle));
            Assert.Equal((30, 26), (skin.TimeLineHeight, skin.TimeLineRowHeight));

            // ファイルがなければ何もしない
            skin.Initialize("missing.ini");
            Assert.Equal(30, skin.TimeLineHeight);
        }

        [Fact]
        public void Theme_ReadsBackgroundImageAndRowHeight()
        {
            using var folder = new TempFolder(setCurrentDirectory: true);
            using (var bitmap = new Bitmap(4, 3))
            {
                bitmap.Save(folder.Combine("back.png"), System.Drawing.Imaging.ImageFormat.Png);
            }
            folder.Write("skin.ini", """
                [BackGround]back.png
                [DisplayMode]9
                [TimeLineRowHeight]40
                """);
            var skin = NewSkin();

            skin.Initialize("skin.ini");

            Assert.Equal((4, 3), (skin.BackGround.Width, skin.BackGround.Height));
            // 範囲の外の表示のしかたは変えない
            Assert.Equal(PPDEditorSkin.BackGroundDisplayMode.right, skin.DisplayMode);
            Assert.Equal(40, skin.TimeLineRowHeight);
            skin.BackGround.Dispose();
        }
    }
}
