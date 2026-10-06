using PPDEditor;
using PPDEditor.Command.PPDSheet;
using PPDEditor.Models;
using PPDEditor.ViewModels.Docks;
using PPDFramework;
using PPDFramework.PPDStructure.PPDData;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Xunit;

namespace UnitTests.Graphics
{
    /// <summary>
    /// エディターのレイヤーのドックで、マークのあるレイヤーを扱うテストです（複製、全レイヤーのまとめ、ID、統計）。
    /// </summary>
    [Collection(GraphicsCollection.Name)]
    public sealed class EditorLayerManagerTests : IDisposable
    {
        sealed class Host : ILayerHost
        {
            public (float Bpm, float BpmOffset, int DisplayWidth) CurrentSettings { get; set; } = (130, 0, 150);
            public bool Confirm(string text, string caption) => true;
            public string AskNewLayerName(string defaultName) => defaultName;
            public Color? PickColor(Color current) => null;
            public void ApplySheetSettings(PPDSheet sheet) { }
            public void RefreshTimeline() { }
            public void OnSheetChanged() { }
        }

        readonly EditorMarkEnvironment environment;
        readonly LayerManagerViewModel layers = new(new Host());

        public EditorLayerManagerTests(GraphicsDevice graphics)
        {
            environment = new EditorMarkEnvironment(graphics);
            layers.Initialize();
        }

        public void Dispose()
        {
            environment.Dispose();
        }

        [Fact]
        public void Duplicate_CopiesMarksAndSettings()
        {
            var source = layers.Layers[0];
            source.PPDData.AddMark(1, 10, 20, 0.5f, (int)ButtonType.Circle, 3, [new("k", "v")], false);
            source.PPDData.AddExMark(2, 3, 30, 40, 0, (int)ButtonType.Cross, 0, false);
            source.PPDData.BPM = 180;

            source.DuplicateCommand.Execute(null);

            var copy = layers.Layers[1];
            Assert.NotSame(source.PPDData, copy.PPDData);
            var marks = copy.PPDData.GetSortedData();
            Assert.Equal(2, marks.Length);
            Assert.Equal((1f, 3u, "v", 0.5f), (marks[0].Time, marks[0].ID, marks[0]["k"], marks[0].Rotation));
            Assert.Equal(3f, Assert.IsType<ExMark>(marks[1]).EndTime);
            Assert.Equal(180f, copy.PPDData.BPM);
            Assert.Equal(source.MarkColor, copy.MarkColor);
        }

        [Fact]
        public void AllData_MergesLayersInOrder()
        {
            var first = layers.Layers[0].PPDData;
            first.AddMark(1, 10, 10, 0, (int)ButtonType.Circle, 5, false);
            layers.AddLayer("second", 130, 0, 150, true, DisplayLineMode.Fourth, DisplayBeatType.Fourth);
            layers.AddLayer("hidden", 130, 0, 150, false, DisplayLineMode.Fourth, DisplayBeatType.Fourth);
            var second = layers.Layers[1].PPDData;
            second.AddMark(1, 99, 99, 0, (int)ButtonType.Circle, 0, false);
            second.AddMark(2, 0, 0, 0, (int)ButtonType.Square, 0, false);
            layers.Layers[2].PPDData.AddMark(3, 0, 0, 0, (int)ButtonType.Square, 9, false);

            // 同じボタンで同じ時間なら先のレイヤーのもの。隠したレイヤーは含めない
            var visible = layers.GetAllData(false);
            Assert.Equal([1f, 2f], visible.Select(m => m.Time));
            Assert.Equal(10, visible[0].X);
            Assert.Equal(3, layers.GetAllData(true).Length);

            // 最大の ID の次から ID を配る（隠したレイヤーも数える）
            layers.CalculateMaxID();
            Assert.Equal(10u, layers.IDProvider.Next());
        }

        [Fact]
        public void Stats_CountNormalAndLongMarks()
        {
            var sheet = layers.Layers[0].PPDData;
            sheet.AddMark(1, (int)ButtonType.Circle, false);
            sheet.AddMark(2, (int)ButtonType.Circle, false);
            sheet.AddExMark(3, 4, (int)ButtonType.Square, false);

            var stats = LayerStats.FromSheet("layer", sheet);

            Assert.Equal("layer", stats.Name);
            Assert.Equal((2, 0), (stats.NormalCounts[(int)ButtonType.Circle], stats.LongCounts[(int)ButtonType.Circle]));
            Assert.Equal((0, 1), (stats.NormalCounts[(int)ButtonType.Square], stats.LongCounts[(int)ButtonType.Square]));
        }
    }
}
