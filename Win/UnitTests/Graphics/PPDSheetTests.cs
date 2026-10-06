using PPDEditor;
using PPDEditor.Command.PPDSheet;
using PPDFramework;
using PPDFramework.PPDStructure.PPDData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Xunit;

namespace UnitTests.Graphics
{
    /// <summary>
    /// エディターの譜面（PPDSheet）のマークの編集と、元に戻す・やり直しのテストです。
    /// </summary>
    /// <remarks>
    /// マークは画像を持つので、本物のデバイスを使う（EditorMarkEnvironment）。
    /// 時間軸の行と ID は、ウィンドウの代わりに PPDSheet の差し替え口から渡す。
    /// </remarks>
    [Collection(GraphicsCollection.Name)]
    public sealed class PPDSheetTests : IDisposable
    {
        readonly EditorMarkEnvironment environment;
        readonly GraphicsDevice graphics;
        uint lastID = 100;

        public PPDSheetTests(GraphicsDevice graphics)
        {
            this.graphics = graphics;
            environment = new EditorMarkEnvironment(graphics);
        }

        public void Dispose()
        {
            environment.Dispose();
        }

        PPDSheet Sheet() => new() { GetVisibleRows = () => [.. Enumerable.Range(0, 10)], NextID = () => ++lastID };

        static float[] Times(PPDSheet sheet, ButtonType type) => [.. sheet.Data[(int)type].Keys];

        static void Select(PPDSheet sheet, float start, ButtonType firstRow, float end, ButtonType lastRow)
        {
            sheet.RecStart = new CustomPoint(start, (int)firstRow);
            sheet.RecEnd = new CustomPoint(end, (int)lastRow);
        }

        [Fact]
        public void AddMarks_UndoAndRedo()
        {
            var sheet = Sheet();
            var changed = 0;
            sheet.CommandChanged += (s, e) => changed++;

            var mark = sheet.AddMark(1, 900, -5, 7, (int)ButtonType.Circle, 0, [new("key", "value")]);
            // 位置は画面の中、角度は 0～2π に収める
            Assert.Equal(new Vector2(800, 0), mark.Position);
            Assert.Equal(7 - MathF.PI * 2, mark.Rotation, 4);
            Assert.Equal("value", mark["key"]);
            Assert.Same(mark, sheet.SelectedMark);
            Assert.Equal([(int)ButtonType.Circle, 0], sheet.FocusedMark);
            var hold = sheet.AddExMark(2, 3, (int)ButtonType.Circle, selectMark: false);
            Assert.IsType<ExMark>(hold);
            Assert.Same(mark, sheet.SelectedMark);

            // 同じ時間、長押しの間、範囲の外のボタンには足せない
            Assert.Null(sheet.AddMark(1, (int)ButtonType.Circle));
            Assert.Null(sheet.AddMark(2.5f, (int)ButtonType.Circle));
            Assert.Null(sheet.AddExMark(3, 4, (int)ButtonType.Circle));
            Assert.Null(sheet.AddMark(1, 10));
            Assert.Null(sheet.AddExMark(1, 2, -1));
            Assert.NotNull(sheet.AddMark(1, (int)ButtonType.Cross, selectMark: false));
            Assert.Equal(3, changed);

            var marks = sheet.GetSortedDataAsMarkData();
            Assert.Equal([1f, 1f, 2f], marks.Select(m => m.Time));
            var exData = Assert.IsType<ExMarkData>(marks[2]);
            Assert.Equal(3f, exData.EndTime);
            Assert.Equal(3, sheet.GetSortedData().Length);

            Assert.True(sheet.CanUndo);
            Assert.True(sheet.Undo());
            Assert.Empty(sheet.Data[(int)ButtonType.Cross]);
            Assert.True(sheet.Undo());
            Assert.False(sheet.Undo());
            Assert.False(sheet.CanUndo);
            Assert.False(sheet.Undo());
            Assert.Empty(sheet.GetSortedData());
            Assert.True(sheet.CanRedo);
            Assert.True(sheet.Redo());
            Assert.True(sheet.Redo());
            Assert.False(sheet.Redo());
            Assert.False(sheet.Redo());
            Assert.Equal(3, sheet.GetSortedData().Length);

            sheet.ClearHistory();
            Assert.False(sheet.CanUndo || sheet.CanRedo);
        }

        [Fact]
        public void DisplaySettings_RaiseChanged()
        {
            var sheet = Sheet();
            var changed = 0;
            sheet.DisplayDataChanged += (s, e) => changed++;

            sheet.BPM = 150;
            sheet.BPMStart = 0.5f;
            sheet.DisplayWidth = 300;
            sheet.DisplayMode = DisplayLineMode.Eigth;
            sheet.BeatType = DisplayBeatType.Third;
            sheet.DisplayName = "レイヤー";

            Assert.Equal(5, changed);
            Assert.Equal((150f, 0.5f, 300, DisplayLineMode.Eigth, DisplayBeatType.Third, "レイヤー"),
                (sheet.BPM, sheet.BPMStart, sheet.DisplayWidth, sheet.DisplayMode, sheet.BeatType, sheet.DisplayName));
        }

        [Fact]
        public void SelectAndShiftSelection()
        {
            var sheet = Sheet();
            var square = sheet.AddMark(1, (int)ButtonType.Square, false);
            var cross = sheet.AddMark(1, (int)ButtonType.Cross, false);
            var circle = sheet.AddMark(2, (int)ButtonType.Circle, false);
            var hold = sheet.AddExMark(3, 5, (int)ButtonType.Circle, false);
            sheet.RecStart = new CustomPoint(1, 0);

            // 少しずれた時間でも選べる。長押しは間のどこでも選べる
            Assert.True(sheet.SelectMark(2.05f, (int)ButtonType.Circle, 0.1f));
            Assert.Same(circle, sheet.SelectedMark);
            Assert.Equal(-1, sheet.RecStart.X);
            Assert.True(sheet.SelectMark(4.5f, (int)ButtonType.Circle, 0.1f));
            Assert.Same(hold, sheet.SelectedMark);
            Assert.False(sheet.SelectMark(2.5f, (int)ButtonType.Circle, 0.1f));

            // 同じ行で前後へ
            Assert.False(sheet.ShiftSelectedMark(1));
            Assert.True(sheet.ShiftSelectedMark(-1));
            Assert.Same(circle, sheet.SelectedMark);
            Assert.False(sheet.ShiftSelectedMark(-1));
            Assert.False(sheet.ShiftSelectedMark(0));

            // すべての行で、ボタンの順に前後へ
            ButtonType[] order = [.. Enum.GetValues<ButtonType>()];
            sheet.SelectMark(1, (int)ButtonType.Square, 0);
            Assert.True(sheet.ShiftSelectedMarkInAll(1, order));
            Assert.Same(cross, sheet.SelectedMark);
            Assert.True(sheet.ShiftSelectedMarkInAll(1, order));
            Assert.Same(circle, sheet.SelectedMark);
            Assert.True(sheet.ShiftSelectedMarkInAll(-1, order));
            Assert.Same(cross, sheet.SelectedMark);
            Assert.True(sheet.ShiftSelectedMarkInAll(-1, order));
            Assert.Same(square, sheet.SelectedMark);
            Assert.False(sheet.ShiftSelectedMarkInAll(-1, order));

            sheet.FocusedMark = [-1, -1];
            Assert.Null(sheet.SelectedMark);
            Assert.False(sheet.ShiftSelectedMark(1));
            Assert.False(sheet.ShiftSelectedMarkInAll(1, order));
            Assert.False(sheet.GetSortedData(false, false, out _));
            Assert.False(sheet.DeleteSelectedMark());
        }

        [Fact]
        public void ShiftSelectedMarkInAll_ReachesMarkAtTimeZero()
        {
            var sheet = Sheet();
            var first = sheet.AddMark(0, (int)ButtonType.Circle, false);
            var last = sheet.AddMark(1, (int)ButtonType.Circle);
            ButtonType[] order = [.. Enum.GetValues<ButtonType>()];

            Assert.True(sheet.ShiftSelectedMarkInAll(-1, order));
            Assert.Same(first, sheet.SelectedMark);
            Assert.True(sheet.ShiftSelectedMarkInAll(1, order));
            Assert.Same(last, sheet.SelectedMark);
        }

        [Fact]
        public void EditSelectedMark()
        {
            var sheet = Sheet();
            var first = sheet.AddMark(1, 100, 100, 0.5f, (int)ButtonType.Circle, 0, false);
            var mark = sheet.AddMark(2, (int)ButtonType.Circle);

            // 前のマークの角度と位置を写す（続けて写すと 1 回で戻る）
            Assert.True(sheet.CopyPreviousMarkAngle());
            Assert.True(sheet.CopyPreviousMarkAngle());
            Assert.Equal(0.5f, mark.Rotation);
            Assert.True(sheet.CopyPreviousMarkPosition());
            Assert.True(sheet.CopyPreviousMarkPosition());
            Assert.Equal(new Vector2(100, 100), mark.Position);
            sheet.Undo();
            Assert.Equal(new Vector2(400, 225), mark.Position);
            sheet.Undo();
            Assert.Equal(0, mark.Rotation);
            sheet.FocusedMark = [(int)ButtonType.Circle, 0];
            Assert.False(sheet.CopyPreviousMarkAngle());
            Assert.False(sheet.CopyPreviousMarkPosition());
            sheet.FocusedMark = [(int)ButtonType.Circle, 1];

            Assert.True(sheet.RotateMark(mark, -1));
            Assert.Equal(MathF.PI * 2 - 1, mark.Rotation, 4);
            Assert.False(sheet.RotateMark(null, 1));
            Assert.True(sheet.MoveMark(mark, new Vector2(-10, 1000)));
            Assert.Equal(new Vector2(0, 450), mark.Position);
            Assert.False(sheet.MoveMark(null, Vector2.Zero));

            // 時間を変える、写す
            Assert.True(sheet.MoveMarkInSame(3));
            Assert.Equal([1f, 3f], Times(sheet, ButtonType.Circle));
            Assert.False(sheet.MoveMarkInSame(1));
            Assert.True(sheet.CopyMarkInSame(4));
            Assert.Equal([1f, 3f, 4f], Times(sheet, ButtonType.Circle));
            Assert.False(sheet.CopyMarkInSame(4));
            Assert.Equal(mark.Position, sheet.Data[(int)ButtonType.Circle][4].Position);

            // 別のボタンに写す、変える
            sheet.FocusedMark = [(int)ButtonType.Circle, 0];
            Assert.True(sheet.CopyMark((int)ButtonType.Square));
            Assert.Equal([(int)ButtonType.Square, 0], sheet.FocusedMark);
            Assert.False(sheet.CopyMark((int)ButtonType.Square));
            Assert.NotSame(first, sheet.SelectedMark);
            Assert.Equal(first.Rotation, sheet.SelectedMark.Rotation);
            Assert.True(sheet.ChangeMarkType((int)ButtonType.Triangle));
            Assert.Equal(ButtonType.Triangle, sheet.SelectedMark.Type);
            Assert.Empty(sheet.Data[(int)ButtonType.Square]);
            Assert.False(sheet.ChangeMarkType(first, (int)ButtonType.Triangle));
            // 範囲の外は端のボタンにする
            Assert.True(sheet.ChangeMarkType(first, 99));
            Assert.Equal(ButtonType.L, first.Type);
            Assert.False(sheet.ChangeMarkType(null, 0));

            sheet.Undo();
            Assert.Equal(ButtonType.Circle, first.Type);
            Assert.Same(first, sheet.Data[(int)ButtonType.Circle][1]);
        }

        [Fact]
        public void HoldMarks_CopyFuseAndSplit()
        {
            var sheet = Sheet();
            var hold = sheet.AddExMark(1, 2, 0, 0, 0, (int)ButtonType.Circle, 7);
            hold["p"] = "1";

            Assert.True(sheet.CopyMarkInSame(5));
            var copy = Assert.IsType<ExMark>(sheet.Data[(int)ButtonType.Circle][5]);
            Assert.Equal((6f, "1", 101u), (copy.EndTime, copy["p"], copy.ID));
            // 長押しの間には写せない
            Assert.True(sheet.CopyMarkInSame(1.5f));
            Assert.Equal(2, sheet.Data[(int)ButtonType.Circle].Count);
            Assert.True(sheet.CopyMark((int)ButtonType.Cross));
            Assert.IsType<ExMark>(sheet.SelectedMark);

            // 長押しを 2 つのマークに分ける（後ろは ID なし）
            Assert.True(sheet.DeFusionExMark());
            var split = sheet.Data[(int)ButtonType.Cross];
            Assert.Equal([1f, 2f], split.Keys);
            Assert.All(split.Values, m => Assert.IsNotType<ExMark>(m));
            Assert.Equal((102u, 0u), (split.Values[0].ID, split.Values[1].ID));
            Assert.Equal("1", split.Values[0]["p"]);
            sheet.FocusedMark = [(int)ButtonType.Cross, 0];
            Assert.False(sheet.DeFusionExMark());

            // 範囲で選んだ同じボタンの 2 つのマークを、長押しにまとめる
            Select(sheet, 0, ButtonType.Cross, 3, ButtonType.Cross);
            Assert.True(sheet.FusionMarks());
            var fused = Assert.IsType<ExMark>(Assert.Single(sheet.Data[(int)ButtonType.Cross].Values));
            Assert.Equal((1f, 2f, "1"), (fused.Time, fused.EndTime, fused["p"]));
            Assert.False(sheet.FusionMarks());
            sheet.Undo();
            Assert.Equal(2, sheet.Data[(int)ButtonType.Cross].Count);
            sheet.Undo();
            Assert.IsType<ExMark>(Assert.Single(sheet.Data[(int)ButtonType.Cross].Values));

            sheet.FocusedMark = [-1, -1];
            Assert.False(sheet.DeFusionExMark());
            sheet.RecStart = new CustomPoint(-1, -1);
            Assert.False(sheet.FusionMarks());
        }

        [Theory]
        [InlineData(PPDSheet.Direction.Up, 10, 400, 215)]
        [InlineData(PPDSheet.Direction.Up, 500, 400, 0)]
        [InlineData(PPDSheet.Direction.Left, 10, 390, 225)]
        [InlineData(PPDSheet.Direction.Left, 500, 0, 225)]
        [InlineData(PPDSheet.Direction.Down, 10, 400, 235)]
        [InlineData(PPDSheet.Direction.Down, 500, 400, 450)]
        [InlineData(PPDSheet.Direction.Right, 10, 410, 225)]
        [InlineData(PPDSheet.Direction.Right, 500, 800, 225)]
        public void MoveMark_StopsAtTheEdge(PPDSheet.Direction direction, int distance, float x, float y)
        {
            var sheet = Sheet();
            var mark = sheet.AddMark(1, (int)ButtonType.Circle);
            var other = sheet.AddMark(1, (int)ButtonType.Cross, false);

            Assert.True(sheet.MoveMark(distance, direction));
            Assert.Equal(new Vector2(x, y), mark.Position);

            // 範囲で選んだマークも同じ
            Select(sheet, 0, ButtonType.Circle, 2, ButtonType.Cross);
            Assert.True(sheet.MoveMarks(distance, direction));
            Assert.Equal(new Vector2(x, y), other.Position);
        }

        [Fact]
        public void MoveMark_RepeatedMovesUndoTogether()
        {
            var sheet = Sheet();
            var mark = sheet.AddMark(1, (int)ButtonType.Circle);

            sheet.MoveMark(10, PPDSheet.Direction.Right);
            sheet.MoveMark(10, PPDSheet.Direction.Right);
            Assert.Equal(new Vector2(420, 225), mark.Position);
            sheet.Undo();
            Assert.Equal(new Vector2(400, 225), mark.Position);

            sheet.FocusedMark = [-1, -1];
            Assert.False(sheet.MoveMark(10, PPDSheet.Direction.Up));
            sheet.RecStart = new CustomPoint(-1, -1);
            Assert.False(sheet.MoveMarks(10, PPDSheet.Direction.Up));
        }

        [Fact]
        public void AreaSelection_OrdersByTimeAcrossRows()
        {
            var sheet = Sheet();
            var a = sheet.AddMark(1, (int)ButtonType.Cross, false);
            var b = sheet.AddMark(2, (int)ButtonType.Square, false);
            var c = sheet.AddMark(3, (int)ButtonType.Cross, false);
            sheet.AddMark(4, (int)ButtonType.Cross, false);
            sheet.AddMark(2, (int)ButtonType.Triangle, false);

            Assert.Empty(sheet.GetAreaData());
            // 後ろから前へ選んでも同じ
            Select(sheet, 3.5f, ButtonType.Cross, 0.5f, ButtonType.Square);
            Assert.True(sheet.AreaSelectionEnabled);

            Assert.Equal([a, b, c], sheet.GetAreaData());
            Assert.Equal([a], sheet.GetAreaData(true));
            Assert.Equal(3, sheet.AreaSelectionCount);
            Assert.True(sheet.GetSortedData(true, true, out var reversed));
            Assert.Equal([c, b, a], reversed);

            // 表示している行の順で選ぶ
            sheet.GetVisibleRows = () => [(int)ButtonType.Triangle, (int)ButtonType.Square];
            Select(sheet, 0, 0, 5, 0);
            Assert.Equal(2, Assert.Single(sheet.GetAreaData()).Time);

            Select(sheet, 0, ButtonType.Square, 5, ButtonType.Cross);
            sheet.GetVisibleRows = () => [.. Enumerable.Range(0, 10)];
            Assert.True(sheet.DeleteSelectedMarks());
            Assert.Empty(sheet.Data[(int)ButtonType.Cross]);
            Assert.Single(sheet.Data[(int)ButtonType.Triangle]);
            Select(sheet, 10, ButtonType.Square, 11, ButtonType.Cross);
            Assert.False(sheet.DeleteSelectedMarks());
            sheet.RecStart = new CustomPoint(-1, -1);
            Assert.False(sheet.GetSortedData(false, true, out _));
        }

        [Fact]
        public void Interpolate_PositionAndAngle()
        {
            var sheet = Sheet();
            sheet.AddMark(0, 100, 100, 1, (int)ButtonType.Circle, 0, false);
            var middle = sheet.AddMark(1, 400, 400, 0, (int)ButtonType.Circle, 0, false);
            sheet.AddMark(4, 500, 300, 2, (int)ButtonType.Circle, 0, false);
            Select(sheet, 0, ButtonType.Circle, 4, ButtonType.Circle);

            sheet.InterpolatePosition();
            Assert.Equal(new Vector2(200, 150), middle.Position);
            sheet.InterpolateAngle(false);
            Assert.Equal(1.25f, middle.Rotation, 4);
            // 時計回りは、終わりの角度を 1 周戻して補う
            sheet.InterpolateAngle(true);
            Assert.Equal(1 + (2 - MathF.PI * 2 - 1) / 4 + MathF.PI * 2, middle.Rotation, 4);
            sheet.Undo();
            sheet.Undo();
            Assert.Equal(0, middle.Rotation);

            // 2 つ未満、同じ時間のときは何もしない
            Select(sheet, 0, ButtonType.Circle, 0.5f, ButtonType.Circle);
            sheet.InterpolatePosition();
            sheet.InterpolateAngle(false);
            var other = sheet.AddMark(0, 700, 10, 0, (int)ButtonType.Cross, 0, false);
            Select(sheet, 0, ButtonType.Circle, 0.5f, ButtonType.Cross);
            sheet.InterpolatePosition();
            sheet.InterpolateAngle(false);
            Assert.Equal(new Vector2(700, 10), other.Position);
        }

        [Fact]
        public void SetData_FromSelectionAndInArea()
        {
            var sheet = Sheet();
            var marks = Enumerable.Range(0, 4).Select(i => sheet.AddMark(i, (int)ButtonType.Circle, false)).ToArray();
            Vector2[] positions = [new(10, 10), new(20, 20), new(30, 30)];
            float[] angles = [0.1f, 0.2f, 0.3f];

            Assert.False(sheet.SetData(positions, angles, false, 0, true));
            sheet.FocusedMark = [(int)ButtonType.Circle, 2];
            // 選んだマークから後ろへ（足りなければそこまで）
            Assert.True(sheet.GetSortedData(false, false, out var forward));
            Assert.Equal(marks[2..], forward);
            Assert.True(sheet.SetData(positions, angles, false, 0, true));
            Assert.Equal((new Vector2(10, 10), 0.1f), (marks[2].Position, marks[2].Rotation));
            Assert.Equal(new Vector2(20, 20), marks[3].Position);
            sheet.Undo();
            Assert.Equal(new Vector2(400, 225), marks[3].Position);
            // 前へ
            Assert.True(sheet.GetSortedData(true, false, out var backward));
            Assert.Equal([marks[2], marks[1], marks[0]], backward);
            Assert.True(sheet.SetData(positions, angles, true, 0, false));
            Assert.Equal((new Vector2(30, 30), 0f), (marks[0].Position, marks[0].Rotation));
            sheet.Undo();

            // 範囲の中
            Select(sheet, 0.5f, ButtonType.Circle, 3.5f, ButtonType.Circle);
            Assert.True(sheet.SetData(positions, angles, true, 1, true));
            Assert.Equal((new Vector2(10, 10), 0.1f), (marks[3].Position, marks[3].Rotation));
            Assert.Equal(new Vector2(30, 30), marks[1].Position);
            sheet.Undo();
            Assert.Equal(new Vector2(400, 225), marks[1].Position);
            Select(sheet, 10, ButtonType.Circle, 11, ButtonType.Circle);
            Assert.False(sheet.SetData(positions, angles, false, 1, true));
            Assert.True(sheet.SetData(positions, angles, false, 2, true));
        }

        [Fact]
        public void ApplyTrans_MovesAreaMarks()
        {
            var sheet = Sheet();
            var mark = sheet.AddMark(1, (int)ButtonType.Circle, false);
            Select(sheet, 0, ButtonType.Circle, 2, ButtonType.Circle);

            // 400x225 の座標を 2 倍にして画面に収める。向きから角度を決める
            sheet.ApplyTrans([new Vector2(150, 100)], [new Vector2(-1, 0)], true);
            Assert.Equal(new Vector2(100, 100), mark.Position);
            Assert.Equal(0, mark.Rotation, 4);
            // 同じマークへの続けての変更は 1 回で戻る
            sheet.ApplyTrans([new Vector2(1000, 0)], [new Vector2(0, -1)], true);
            Assert.Equal(new Vector2(800, 0), mark.Position);
            Assert.Equal(MathF.PI * 3 / 2, mark.Rotation, 4);
            sheet.Undo();
            Assert.Equal((new Vector2(400, 225), 0f), (mark.Position, mark.Rotation));

            sheet.ApplyTrans([new Vector2(150, 100)], [new Vector2(0, 1)], false);
            Assert.Equal(0, mark.Rotation);
            Select(sheet, 10, ButtonType.Circle, 11, ButtonType.Circle);
            sheet.ApplyTrans([Vector2.Zero], [Vector2.Zero], true);
        }

        [Fact]
        public void MouseDrag_MovesAndRotatesSelectedMark()
        {
            var sheet = Sheet();
            var mark = sheet.AddMark(1, (int)ButtonType.Circle);
            sheet.ClearHistory();

            // 位置（画面の外は端に）。続けて動かすと 1 回で戻る
            sheet.MoveMark(new Vortice.Mathematics.Int2(100, 200), 0, false, false, false);
            sheet.MoveMark(new Vortice.Mathematics.Int2(900, -10), 0, false, false, false);
            Assert.Equal(new Vector2(800, 0), mark.Position);
            sheet.Undo();
            Assert.Equal(new Vector2(400, 225), mark.Position);
            Assert.False(sheet.CanUndo);

            // 角度はマークから指した点への向き（上が π/2、下が 3π/2）
            sheet.MoveMark(new Vortice.Mathematics.Int2(400, 125), 1, false, false, false);
            Assert.Equal(MathF.PI / 2, mark.Rotation, 3);
            sheet.MoveMark(new Vortice.Mathematics.Int2(400, 325), 1, false, false, false);
            Assert.Equal(MathF.PI * 3 / 2, mark.Rotation, 3);
            sheet.Undo();
            Assert.Equal(0, mark.Rotation);
        }

        [Fact]
        public void MouseDrag_SnapsAngleWithModifierKeys()
        {
            var sheet = Sheet();
            var mark = sheet.AddMark(1, (int)ButtonType.Circle);
            var saved = (int[])PPDStaticSetting.Angles.Clone();
            try
            {
                // Shift で 45 度ずつ
                PPDStaticSetting.Angles[1] = 45;
                sheet.MoveMark(new Vortice.Mathematics.Int2(500, 215), 1, true, false, false);
                Assert.Equal(0, mark.Rotation, 3);
                sheet.MoveMark(new Vortice.Mathematics.Int2(490, 140), 1, true, false, false);
                Assert.Equal(MathF.PI / 4, mark.Rotation, 3);
                // ほかの組み合わせの設定も使う
                for (var i = 2; i < 8; i++)
                {
                    PPDStaticSetting.Angles[i] = 90;
                }
                foreach (var (shift, ctrl, alt) in new[] { (false, true, false), (false, false, true), (true, true, false), (true, false, true), (false, true, true), (true, true, true) })
                {
                    sheet.MoveMark(new Vortice.Mathematics.Int2(480, 160), 1, shift, ctrl, alt);
                    Assert.Equal(0, mark.Rotation % (MathF.PI / 2), 3);
                }
            }
            finally
            {
                Array.Copy(saved, PPDStaticSetting.Angles, saved.Length);
            }
        }

        [Fact]
        public void MouseDrag_RotatesAllMarksInArea()
        {
            var sheet = Sheet();
            var marks = new[] { sheet.AddMark(1, (int)ButtonType.Circle, false), sheet.AddMark(2, 100, 100, 1, (int)ButtonType.Cross, 0, false) };
            sheet.ClearHistory();
            sheet.FocusedMark = [-1, -1];
            Select(sheet, 0, ButtonType.Circle, 3, ButtonType.Cross);

            // 最初のマークから見た向きを、すべてのマークに
            sheet.MoveMark(new Vortice.Mathematics.Int2(400, 125), 1, false, false, false);
            sheet.MoveMark(new Vortice.Mathematics.Int2(500, 225), 1, false, false, false);

            Assert.All(marks, m => Assert.Equal(0, m.Rotation, 3));
            sheet.Undo();
            Assert.Equal((0f, 1f), (marks[0].Rotation, marks[1].Rotation));
            Assert.False(sheet.CanUndo);
        }

        [Fact]
        public void MoveAllLines_ShiftsEveryRow()
        {
            var sheet = Sheet();
            sheet.AddMark(1, (int)ButtonType.Circle, false);
            sheet.AddExMark(2, 3, (int)ButtonType.Cross, false);
            sheet.ClearHistory();

            // すべての行をまとめて動かす
            sheet.MoveLine(int.MaxValue, 0.5f);

            Assert.Equal([1.5f], Times(sheet, ButtonType.Circle));
            Assert.Equal(3.5f, ((ExMark)sheet.Data[(int)ButtonType.Cross][2.5f]).EndTime);
            sheet.Undo();
            Assert.Equal([1f], Times(sheet, ButtonType.Circle));
            Assert.Equal(3f, ((ExMark)sheet.Data[(int)ButtonType.Cross][2]).EndTime);
        }

        [Fact]
        public void SetData_InAreaWithoutAngle()
        {
            var sheet = Sheet();
            var marks = Enumerable.Range(0, 3).Select(i => sheet.AddMark(i, 400, 225, 0.5f, (int)ButtonType.Circle, 0, false)).ToArray();
            Select(sheet, -0.5f, ButtonType.Circle, 2.5f, ButtonType.Circle);

            // 前から、位置だけ（並べる数のほうが多ければ、マークの数まで）
            Assert.True(sheet.SetData([new(1, 1), new(2, 2), new(3, 3), new(4, 4)], [9, 9, 9, 9], false, 1, false));

            Assert.Equal([new Vector2(1, 1), new Vector2(2, 2), new Vector2(3, 3)], marks.Select(m => m.Position));
            Assert.All(marks, m => Assert.Equal(0.5f, m.Rotation));
            sheet.Undo();
            Assert.All(marks, m => Assert.Equal(new Vector2(400, 225), m.Position));

            // 選んだマークから前へ、位置だけ
            sheet.FocusedMark = [(int)ButtonType.Circle, 2];
            Assert.True(sheet.SetData([new(5, 5), new(6, 6)], [0, 0], true, 0, false));
            Assert.Equal([new Vector2(400, 225), new Vector2(6, 6), new Vector2(5, 5)], marks.Select(m => m.Position));
            sheet.Undo();
            Assert.Equal(new Vector2(400, 225), marks[1].Position);
        }

        [Fact]
        public void LinesAndData_MoveSwapDelete()
        {
            var sheet = Sheet();
            sheet.AddMark(1, (int)ButtonType.Circle, false);
            sheet.AddExMark(2, 3, (int)ButtonType.Circle, false);
            sheet.AddMark(1, (int)ButtonType.Cross, false);

            // 続けて行を動かすと 1 回で戻る
            sheet.MoveLine((int)ButtonType.Circle, 0.5f);
            sheet.MoveLine((int)ButtonType.Circle, 0.5f);
            Assert.Equal([2f, 3f], Times(sheet, ButtonType.Circle));
            Assert.Equal(4f, ((ExMark)sheet.Data[(int)ButtonType.Circle][3]).EndTime);
            sheet.MoveLine((int)ButtonType.Cross, 1);
            Assert.Equal([2f], Times(sheet, ButtonType.Cross));
            sheet.Undo();
            sheet.Undo();
            Assert.Equal([1f, 2f], Times(sheet, ButtonType.Circle));

            sheet.Swap(ButtonType.Circle, ButtonType.Cross);
            sheet.Swap(ButtonType.Circle, ButtonType.Circle);
            Assert.Equal([1f], Times(sheet, ButtonType.Circle));
            Assert.Equal(ButtonType.Circle, sheet.Data[(int)ButtonType.Circle][1].Type);
            Assert.Equal(ButtonType.Cross, sheet.Data[(int)ButtonType.Cross][2].Type);
            sheet.Undo();
            Assert.Equal([1f, 2f], Times(sheet, ButtonType.Circle));

            // 近い小節線に合わせる
            var hold = (ExMark)sheet.Data[(int)ButtonType.Circle][2];
            sheet.ForceToNearBar(new Dictionary<Mark, float[]> { [hold] = [2.5f, 3.5f] });
            Assert.Equal((2.5f, 3.5f), (hold.Time, hold.EndTime));
            Assert.Same(hold, sheet.Data[(int)ButtonType.Circle][2.5f]);
            sheet.Undo();
            Assert.Equal((2f, 3f), (hold.Time, hold.EndTime));

            sheet.DeleteData((int)ButtonType.Circle);
            Assert.Empty(sheet.Data[(int)ButtonType.Circle]);
            Assert.Single(sheet.Data[(int)ButtonType.Cross]);
            sheet.DeleteData(-1);
            Assert.Empty(sheet.GetSortedData());
            sheet.Undo();
            sheet.Undo();
            Assert.Equal(3, sheet.GetSortedData().Length);
        }

        [Fact]
        public void DeleteMarks_ClearsSelection()
        {
            var sheet = Sheet();
            var first = sheet.AddMark(1, (int)ButtonType.Circle, false);
            var second = sheet.AddMark(2, (int)ButtonType.Circle);

            sheet.DeleteMark(first);
            Assert.Equal([2f], Times(sheet, ButtonType.Circle));
            sheet.FocusedMark = [(int)ButtonType.Circle, 0];
            sheet.DeleteMark(second);
            Assert.Equal([-1, -1], sheet.FocusedMark);
            sheet.Undo();
            sheet.Undo();
            sheet.RemoveMark(first);
            sheet.RemoveMark(first);
            Assert.Equal([2f], Times(sheet, ButtonType.Circle));

            sheet.FocusedMark = [(int)ButtonType.Circle, 0];
            Assert.True(sheet.DeleteSelectedMark());
            Assert.Empty(sheet.Data[(int)ButtonType.Circle]);
        }

        [Fact]
        public void IDsAndParameters()
        {
            var sheet = Sheet();
            var mark = sheet.AddMark(1, (int)ButtonType.Circle);
            var other = sheet.AddMark(2, (int)ButtonType.Circle, false);

            // パラメーターを付けると ID も付く
            sheet.ChangeParameter("key", "value");
            Assert.Equal((101u, "value"), (mark.ID, mark["key"]));
            sheet.ChangeParameter(mark, null, null);
            Assert.Equal("", mark[""]);
            Assert.Equal(101u, mark.ID);
            Assert.Equal(0u, sheet.GetCorrectID(other));
            Assert.Equal(102u, sheet.GetCorrectID(mark));
            Assert.Equal(0u, sheet.GetCorrectID(new MarkData(0, 0, 0, 0, ButtonType.Circle, 0)));
            Assert.Equal(103u, sheet.GetCorrectID(new MarkData(0, 0, 0, 0, ButtonType.Circle, 5)));

            sheet.RemoveParameter("");
            Assert.DoesNotContain(mark.Parameters, p => p.Key == "");
            // ID を外すときにパラメーターがあれば聞く（断れば何もしない）
            sheet.UnassignID(() => false);
            Assert.Equal(101u, mark.ID);
            sheet.UnassignID(() => true);
            Assert.Equal(0u, mark.ID);
            Assert.Empty(mark.Parameters);
            sheet.Undo();
            Assert.Equal((101u, "value"), (mark.ID, mark["key"]));

            sheet.AssignID();
            Assert.Equal(104u, mark.ID);
            sheet.UnassignID(mark);
            Assert.Equal((0u, 0), (mark.ID, mark.Parameters.Length));

            // 範囲で選んだマークにまとめて
            sheet.FocusedMark = [-1, -1];
            Select(sheet, 0, ButtonType.Circle, 3, ButtonType.Circle);
            sheet.ChangeParameter("all", "1");
            Assert.All(new[] { mark, other }, m => Assert.Equal("1", m["all"]));
            sheet.RemoveParameter("all");
            Assert.All(new[] { mark, other }, m => Assert.Null(m["all"]));
            sheet.UnassignID(() => true);
            Assert.All(new[] { mark, other }, m => Assert.Equal(0u, m.ID));
            sheet.AssignID();
            Assert.All(new[] { mark, other }, m => Assert.NotEqual(0u, m.ID));
            sheet.AssignID(mark);
            Assert.Equal(lastID, mark.ID);
        }

        sealed class EventHost : global::PPDEditor.ViewModels.Docks.IEventManagerHost
        {
            public float CurrentTime => 0;
            public float DefaultBpm => 120;
            public PPDEditorCommon.EventData EditEvent(PPDEditorCommon.EventData eventData) => eventData;
            public void ShowMessage(string text) { }
            public bool Confirm(string text, string caption, bool yesNo) => false;
            public void ShowSelectedRow() { }
            public void RefreshTimeline() { }
            public void ChangeMovieVolume(int volume) { }
        }

        [Fact]
        public void UpdateAndDrawMarks_FollowTime()
        {
            var events = new global::PPDEditor.ViewModels.Docks.EventManagerViewModel(new EventHost());
            var sheet = Sheet();
            sheet.GetEvents = () => events;
            sheet.AddMark(1, 100, 100, 0, (int)ButtonType.Circle, 0, null, false);
            sheet.AddExMark(2, 3, (int)ButtonType.Cross, selectMark: false);
            using var dummy = new DummyDrawComponent(graphics.Device);
            dummy.Drawing += sheet.DrawMark;
            static bool Red(uint[] pixels, int x, int y) => GraphicsDevice.Rgb(GraphicsDevice.Pixel(pixels, x, y)) == (255, 0, 0);

            // マークの時間の少し前から出る
            Assert.Equal(new bool[10], sheet.UpdateMark(0.5f, 1));
            Assert.True(Red(graphics.Render(dummy), 100, 100));

            // マークの時間になったら、そのボタンの音を鳴らす（そのあとは消える）
            var played = sheet.UpdateMark(1, 1);
            Assert.True(played[(int)ButtonType.Circle]);
            Assert.False(played[(int)ButtonType.Cross]);
            Assert.False(Red(graphics.Render(dummy), 100, 100));
            Assert.True(sheet.UpdateMark(1.9f, 1).All(p => !p));
            Assert.True(sheet.UpdateMark(2, 1)[(int)ButtonType.Cross]);
            Assert.True(sheet.UpdateMark(3.5f, 1).All(p => !p));

            // 消えたマークも、範囲選択の中なら印だけ描く
            Select(sheet, 0.5f, ButtonType.Circle, 1.5f, ButtonType.Circle);
            Assert.True(Red(graphics.Render(dummy), 100, 100));
        }
    }
}
