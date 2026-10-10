using PPDCoreModel;
using PPDCoreModel.Data;
using PPDFramework;
using PPDFramework.PPDStructure;
using PPDFramework.PPDStructure.PPDData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Xunit;

namespace UnitTests.FlowScript
{
    /// <summary>
    /// 譜面のマークを読む・作るノード（PPD.Mark.Reader、Value、DynamicCreate）のテストです。
    /// </summary>
    public class MarkNodeTests
    {
        static Type NodeType(string name)
        {
            return typeof(FlowScriptEnginePPD.ToolTipText).Assembly.GetType($"FlowScriptEnginePPD.FlowSourceObjects.Mark.{name}FlowSourceObject", true);
        }

        static NodeHarness Node(string name, params (string Name, object Value)[] inputs)
        {
            var harness = new NodeHarness(NodeType(name));
            foreach (var (n, v) in inputs)
            {
                harness.Set(n, v);
            }
            return harness;
        }

        static MemoryStream Score()
        {
            var stream = new MemoryStream();
            PPDWriter.Write(stream, [new MarkData(100, 200, 0.5f, 1.25f, ButtonType.Circle, 3), new ExMarkData(300, 400, 1, 2, 2.5f, ButtonType.L, 4)]);
            stream.Position = 0;
            return stream;
        }

        [Fact]
        public void Reader_ReadsMarksInOrder()
        {
            var reader = Node("Reader", ("Stream", Score())).Invoke();
            Assert.Equal(["Success"], reader.Fired);
            Assert.Equal(false, reader.Get("EndOfStream"));

            reader.Invoke("Next");
            Assert.Equal(new Vector2(100, 200), reader.Get("Position"));
            Assert.Equal(0.5f, reader.Get("Angle"));
            Assert.Equal(1.25f, reader.Get("Time"));
            Assert.Equal(MarkType.Circle, reader.Get("MarkType"));
            Assert.Equal(3, reader.Get("MarkID"));
            Assert.Equal(false, reader.Get("IsLong"));
            Assert.Equal(0f, reader.Get("EndTime"));

            reader.Invoke("Next");
            Assert.Equal(MarkType.L, reader.Get("MarkType"));
            Assert.Equal(true, reader.Get("IsLong"));
            Assert.Equal(2.5f, reader.Get("EndTime"));
            Assert.Equal(true, reader.Get("EndOfStream"));

            // 最後まで読んだあとは変わらない
            reader.Invoke("Next");
            Assert.Equal(4, reader.Get("MarkID"));
        }

        [Fact]
        public void Reader_WithoutStream_Fails()
        {
            Assert.Equal(["Failed"], Node("Reader").Invoke().Fired);
            // 読む前は終わりではない
            Assert.Equal(false, Node("Reader").Get("EndOfStream"));
        }

        [Fact]
        public void Reader_NotScore_HasNoMarks()
        {
            // 始めが "PPD" でなければ、マークのない譜面として読む
            var reader = Node("Reader", ("Stream", new MemoryStream([1, 2, 3]))).Invoke();

            Assert.Equal(["Success"], reader.Fired);
            Assert.Equal(true, reader.Get("EndOfStream"));
        }

        [Fact]
        public void Value_ReadsMarkData()
        {
            var mark = new ExMarkData(10, 20, 0.25f, 1, 3, ButtonType.Triangle, 5);
            mark.AddParameter("key", "value");
            var value = Node("Value", ("Value", mark));

            Assert.Equal(1f, value.Get("Time"));
            Assert.Equal(true, value.Get("IsLong"));
            Assert.Equal(3f, value.Get("ReleaseTime"));
            Assert.Equal(new Vector2(10, 20), value.Get("Position"));
            Assert.Equal(0.25f, value.Get("Rotation"));
            Assert.Equal(MarkType.Triangle, value.Get("Type"));
            Assert.Equal(5, value.Get("ID"));
            Assert.Equal(new KeyValuePair<string, string>("key", "value"), Assert.Single((IEnumerable<object>)value.Get("Parameters")));

            var normal = Node("Value", ("Value", new MarkData(0, 0, 0, 2, ButtonType.Up, 0)));
            Assert.Equal(false, normal.Get("IsLong"));
            Assert.Equal(0f, normal.Get("ReleaseTime"));
        }

        [Fact]
        public void Value_WithoutMark_IsDefault()
        {
            var value = Node("Value");

            Assert.Equal(0f, value.Get("Time"));
            Assert.Equal(false, value.Get("IsLong"));
            Assert.Equal(0f, value.Get("ReleaseTime"));
            Assert.Equal(Vector2.Zero, value.Get("Position"));
            Assert.Equal(0f, value.Get("Rotation"));
            Assert.Equal((MarkType)0, value.Get("Type"));
            Assert.Equal(0, value.Get("ID"));
            Assert.Null(value.Get("Parameters"));
        }

        sealed class FakeMarkManager : IMarkManager
        {
            public List<string> Added { get; } = [];
            public void AddMark(Vector2 position, float angle, float time, MarkType markType, int id) => Added.Add($"mark {position} {angle} {time} {markType} {id}");
            public void AddLongMark(Vector2 position, float angle, float time, float endTime, MarkType markType, int id) => Added.Add($"long {position} {angle} {time}-{endTime} {markType} {id}");
            public int AllMarkCount => throw new NotSupportedException();
            public int AllLongMarkCount => throw new NotSupportedException();
            public ScoreDifficultyMeasureResult ScoreDifficultyMeasureResult => throw new NotSupportedException();
            public MarkDataBase[] Marks => throw new NotSupportedException();
            public int GetMarkCount(int sameNum) => throw new NotSupportedException();
        }

        [Fact]
        public void DynamicCreate_AddsMarksToGame()
        {
            var manager = new FakeMarkManager();
            NodeHarness Create(params (string, object)[] inputs)
            {
                var harness = Node("DynamicCreate", [("Position", new Vector2(1, 2)), ("Angle", 0.5f), ("MarkType", MarkType.Cross), ("Time", 3f), ("MarkID", 9), .. inputs]);
                harness.Items["MarkManager"] = manager;
                return harness.Initialize().Invoke();
            }

            Assert.Equal(["Success"], Create().Fired);
            Assert.Equal(["Success"], Create(("IsLong", true), ("EndTime", 4f)).Fired);
            // 長いマークは終わりが始まりより後でないといけない
            Assert.Equal(["Failed"], Create(("IsLong", true), ("EndTime", 3f)).Fired);

            Assert.Equal([$"mark {new Vector2(1, 2)} 0.5 3 Cross 9", $"long {new Vector2(1, 2)} 0.5 3-4 Cross 9"], manager.Added);
        }

        [Fact]
        public void DynamicCreate_OutsideGame_Fails()
        {
            Assert.Equal(["Failed"], Node("DynamicCreate").Initialize().Invoke().Fired);
        }
    }
}
