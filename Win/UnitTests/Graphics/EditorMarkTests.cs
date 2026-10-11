using PPDEditor;
using PPDFramework;
using PPDFramework.PPDStructure.EVDData;
using PPDFramework.PPDStructure.PPDData;
using System;
using System.Numerics;
using Xunit;
using static UnitTests.Graphics.GraphicsDevice;

namespace UnitTests.Graphics
{
    /// <summary>
    /// エディターのマーク（Mark、長押しの ExMark）の表示と、音を鳴らす時のテストです。
    /// </summary>
    [Collection(GraphicsCollection.Name)]
    public sealed class EditorMarkTests(GraphicsDevice graphics) : IDisposable
    {
        const float Bpm = Mark.defaultbpm;
        readonly GraphicsDevice graphics = graphics;
        readonly EditorMarkEnvironment environment = new(graphics);

        public void Dispose()
        {
            environment.Dispose();
        }

        Mark NewMark(ButtonType type, float time = 10) =>
            new(graphics.Device, environment.Resources, PPDEditorSkin.Skin, type, 400, 225, time, 0, 0);

        ExMark NewExMark(ButtonType type, float time = 1, float endTime = 3) =>
            new(graphics.Device, environment.Resources, PPDEditorSkin.Skin, type, 400, 225, time, endTime, 0, global::PPDEditor.Utility.CirclePoints, 0);

        /// <summary>
        /// 描く処理を、GameComponent として描きます。
        /// </summary>
        sealed class Drawer(PPDDevice device, Action draw) : GameComponent(device)
        {
            public override void Draw() => draw();
        }

        uint[] Render(Action draw) => graphics.Render(new Drawer(graphics.Device, draw));

        static bool IsRed(uint[] pixels, int x = 400, int y = 225) => Rgb(Pixel(pixels, x, y)) == (255, 0, 0);

        [Fact]
        public void Update_ShowsMarkBeforeItsTime()
        {
            var mark = NewMark(ButtonType.Circle);
            int Update(float time, DisplayState state = DisplayState.Normal, float bpm = Bpm) => mark.Update(time, bpm, state, NoteType.Normal, 0);

            // 2 拍ぶん（BPM 130 で 2 秒）前から見える
            Assert.Equal(0, Update(7));
            Assert.True(mark.Hidden);
            Assert.False(IsRed(Render(mark.Draw)));
            Assert.Equal(0, Update(8.5f));
            Assert.False(mark.Hidden);
            Assert.True(IsRed(Render(mark.Draw)));
            // BPM が速いと、見えるのは遅くなる
            Assert.Equal(0, Update(8.5f, bpm: Bpm * 2));
            Assert.True(mark.Hidden);

            // 時間になったら 1 回だけ音を鳴らし、消える
            Update(9.9f);
            Assert.Equal(1, Update(10));
            Assert.True(mark.Hidden);
            Assert.Equal(0, Update(10.1f));
        }

        [Theory]
        [InlineData(DisplayState.Sudden, 9, true)]
        [InlineData(DisplayState.Sudden, 9.6f, false)]
        [InlineData(DisplayState.Hidden, 9, false)]
        [InlineData(DisplayState.Hidden, 9.8f, true)]
        public void Update_SuddenAndHidden(DisplayState state, float time, bool hidden)
        {
            // サドンは直前の 0.5 拍だけ、ヒドゥンは直前の 0.25 拍を隠す
            var mark = NewMark(ButtonType.Circle);

            mark.Update(time, Bpm, state, NoteType.Normal, 0);

            Assert.Equal(hidden, mark.Hidden);
        }

        [Theory]
        [InlineData(ButtonType.R, 0, true, true)]
        [InlineData(ButtonType.L, 0, true, false)]
        [InlineData(ButtonType.Circle, 1 << (int)ButtonType.R, true, true)]
        [InlineData(ButtonType.Triangle, 1 << (int)ButtonType.L, true, false)]
        [InlineData(ButtonType.Circle, 0, false, false)]
        [InlineData(ButtonType.Cross, 1 << (int)ButtonType.R, false, false)]
        public void ACFT_ScratchMarks(ButtonType type, int sameTimingMarks, bool scratch, bool right)
        {
            var mark = NewMark(type);

            mark.Update(5, Bpm, DisplayState.Normal, NoteType.ACFT, sameTimingMarks);

            Assert.Equal(sameTimingMarks, mark.SameTimingMarks);
            Assert.Equal(sameTimingMarks != 0, mark.HasRLSameTiming);
            Assert.Equal((scratch, right), (mark.IsScratch, mark.IsRight));
            // AC-FT でなければスクラッチにはならない
            mark.NoteType = NoteType.AC;
            Assert.False(mark.IsScratch);
        }

        [Fact]
        public void Convert_KeepsDataAndParameters()
        {
            var mark = NewMark(ButtonType.Square, 2);
            mark.Position = new Vector2(10, 20);
            mark.Rotation = 1.5f;
            mark.ID = 9;
            mark["a"] = "1";
            mark["b"] = "2";
            mark.RemoveParameter("b");
            var other = NewMark(ButtonType.Cross);
            other.CopyParameters(mark);
            var data = new MarkData(0, 0, 0, 0, ButtonType.Circle, 0);
            data.AddParameter("c", "3");
            other.CopyParameters(data);
            mark.Type = ButtonType.Triangle;

            var converted = mark.Convert();

            Assert.Equal((10f, 20f, 1.5f, 2f, ButtonType.Triangle, 9u), (converted.X, converted.Y, converted.Angle, converted.Time, converted.ButtonType, converted.ID));
            Assert.Equal([new("a", "1")], converted.Parameters);
            Assert.Null(mark["b"]);
            Assert.Equal(("1", "3"), (other["a"], other["c"]));

            var hold = NewExMark(ButtonType.Circle);
            hold["p"] = "x";
            var holdData = Assert.IsType<ExMarkData>(hold.Convert());
            Assert.Equal((1f, 3f), (holdData.Time, holdData.EndTime));
            Assert.Equal([new("p", "x")], holdData.Parameters);
        }

        [Fact]
        public void ExMark_NormalHold_PlaysAtStartAndRelease()
        {
            var hold = NewExMark(ButtonType.Circle);
            int Update(float time, bool releaseSound = true) =>
                hold.ExUpdate(time, Bpm, DisplayState.Normal, NoteType.Normal, 1, releaseSound, 0);

            Assert.Equal(2, hold.Length);
            Assert.Equal(0, Update(0));
            Assert.False(hold.Hidden);
            Assert.True(IsRed(Render(hold.Draw)));
            Render(hold.GageDraw);
            Render(hold.DrawOnlyMark);
            Assert.Equal(1, Update(1));
            Assert.Equal(0, Update(2));
            Render(hold.GageDraw);
            // 離したときにも鳴らす
            Assert.Equal(1, Update(3));
            Assert.True(hold.Hidden);
            Assert.Equal(0, Update(3.5f));

            // 離したときの音がなければ鳴らさない
            var quiet = NewExMark(ButtonType.Circle);
            quiet.ExUpdate(0, Bpm, DisplayState.Normal, NoteType.Normal, 1, false, 0);
            Assert.Equal(1, quiet.ExUpdate(1, Bpm, DisplayState.Normal, NoteType.Normal, 1, false, 0));
            Assert.Equal(0, quiet.ExUpdate(3, Bpm, DisplayState.Normal, NoteType.Normal, 1, false, 0));
        }

        [Fact]
        public void ExMark_ACHold_EndsAtStart()
        {
            var hold = NewExMark(ButtonType.Circle);

            Assert.Equal(0, hold.ExUpdate(0.5f, Bpm, DisplayState.Normal, NoteType.AC, 1, true, 0));
            Assert.False(hold.Hidden);
            Assert.True(IsRed(Render(hold.Draw)));
            Render(hold.DrawOnlyMark);
            Assert.Equal(1, hold.ExUpdate(1, Bpm, DisplayState.Normal, NoteType.AC, 1, true, 0));
            Assert.True(hold.Hidden);
            Assert.False(IsRed(Render(hold.Draw)));
        }

        [Fact]
        public void ExMark_Scratch_HasSlideMarks()
        {
            var hold = NewExMark(ButtonType.R);
            int Update(float time, float slideScale = 1) =>
                hold.ExUpdate(time, Bpm, DisplayState.Normal, NoteType.ACFT, slideScale, false, 0);

            // 1 秒に 24 個のスライドのマーク。右は右へ並べる
            Assert.Equal(0, Update(0));
            Assert.Equal(48, hold.ExCount);
            Assert.True(hold.IsRight);
            var pixels = Render(hold.Draw);
            Assert.True(IsRed(pixels));
            Assert.True(IsRed(pixels, 400 + 24));
            Assert.False(IsRed(pixels, 400 - 24));
            Render(hold.DrawOnlyMark);
            Assert.Equal(1, Update(1));
            Update(2, slideScale: 0.5f);
            Assert.Equal(24, hold.ExCount);
            // スクラッチは離したときの音の設定がなくても鳴らす
            Assert.Equal(1, Update(3));

            // 長さを変えると、スライドのマークも作り直す
            hold.EndTime = 1.5f;
            Assert.Equal(12, hold.ExCount);
            hold.Position = new Vector2(100, 100);
            Assert.Equal(new Vector2(100, 100), hold.Position);
        }

        [Fact]
        public void ExMark_SuddenAndHidden()
        {
            var hold = NewExMark(ButtonType.Circle);

            hold.ExUpdate(0.5f, Bpm, DisplayState.Hidden, NoteType.Normal, 1, true, 0);
            Assert.False(hold.Hidden);
            hold.ExUpdate(0.9f, Bpm, DisplayState.Hidden, NoteType.Normal, 1, true, 0);
            Assert.True(hold.Hidden);
            // サドンは直前の 0.5 拍だけ見せる
            hold.ExUpdate(0.9f, Bpm, DisplayState.Sudden, NoteType.Normal, 1, true, 0);
            Assert.False(hold.Hidden);
        }
    }
}
