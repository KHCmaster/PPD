using PPDCoreModel;
using PPDCoreModel.Data;
using PPDFramework;
using System;
using System.Collections.Generic;
using System.Numerics;
using Xunit;

namespace UnitTests.FlowScript
{
    /// <summary>
    /// ゲームがマークごとに呼ぶノード（PPD.Mark.CalculatePos など）のテストです。
    /// </summary>
    /// <remarks>
    /// ゲームはインターフェース（ICalculatePosition など）でノードを呼び、ノードは Out を出して、スクリプトが決めた Handled と値を返す。
    /// 種類ごとに、すべてのマークに使う Any、ID で選ぶ ByID、時間で選ぶ WithinTime がある。
    /// </remarks>
    public class MarkHookNodeTests
    {
        sealed class FakeMark : IMarkInfo
        {
            public Vector2 ColorPosition { get; set; }
            public Vector2 Position { get; init; } = new(100, 200);
            public float Angle { get; init; } = 0.5f;
            public float Time { get; init; } = 3;
            public uint ID { get; init; } = 7;
            public MarkType Type { get; init; } = MarkType.Triangle;
            public bool IsLong { get; init; } = true;
            public bool IsAC { get; init; } = true;
            public bool HasSameTimingMark { get; init; } = true;
            public float ReleaseTime { get; init; } = 4;
            public bool IsACFT { get; init; } = true;
            public bool IsScratch { get; init; } = true;
            public bool IsRight { get; init; } = true;
            public Dictionary<object, object> Parameters { get; init; } = new() { ["p"] = 1 };
            public float SlideScale { get; init; } = 1.5f;
            public int SameTimingMarks { get; init; } = 2;
        }

        static Type NodeType(string name)
        {
            return typeof(FlowScriptEnginePPD.ToolTipText).Assembly.GetType($"FlowScriptEnginePPD.FlowSourceObjects.Mark.{name}FlowSourceObject", true);
        }

        // 種類ごとの、ノードの呼び方と、Out で決める値と、その結果
        public record Kind(string Name, Action<NodeHarness, IMarkInfo> Call, Action<NodeHarness> Decide, Action<NodeHarness> Check);

        static readonly Dictionary<string, Kind> kinds = new()
        {
            ["CalculatePos"] = new("CalculatePos",
                (h, m) => ((ICalculatePosition)h.Node).Calculate(m, 2.5f, 150),
                h =>
                {
                    Assert.Equal(2.5f, h.Get("CurrentTime"));
                    Assert.Equal(150f, h.Get("BPM"));
                    h.Set("ColorPosition", new Vector2(5, 6));
                },
                h => Assert.Equal(new Vector2(5, 6), ((ICalculatePosition)h.Node).CalculatePosition)),
            ["ProcessAllowedButtons"] = new("ProcessAllowedButtons",
                (h, m) => ((IProcessAllowedButtons)h.Node).Process(m),
                // MarkType 以外は使わない
                h => h.Set("AllowedButtons", new object[] { MarkType.Circle, null, "x", MarkType.L }),
                h => Assert.Equal([ButtonType.Circle, ButtonType.L], ((IProcessAllowedButtons)h.Node).ProcessAllowedButtons)),
            ["ProcessEvaluate"] = new("ProcessEvaluate",
                (h, m) => ((IProcessEvaluate)h.Node).ProcessEvaluate(m, EffectType.Cool, true, false, new Vector2(9, 8)),
                h =>
                {
                    Assert.Equal(EffectType.Cool, h.Get("EffectType"));
                    Assert.Equal(true, h.Get("IsMissPress"));
                    Assert.Equal(false, h.Get("IsRelease"));
                    Assert.Equal(new Vector2(9, 8), h.Get("Position"));
                },
                h => { }),
            ["ProcessMarkBPM"] = new("ProcessMarkBPM",
                (h, m) => ((IProcessMarkBPM)h.Node).Process(m),
                h => h.Set("BPM", 240f),
                h => Assert.Equal(240f, ((IProcessMarkBPM)h.Node).ProcessBPM)),
            ["ProcessMissPress"] = new("ProcessMissPress",
                (h, m) => ((IProcessMissPress)h.Node).Process(m, MarkType.Cross),
                h =>
                {
                    Assert.Equal(MarkType.Cross, h.Get("PressedButton"));
                    h.Set("IsMissPress", true);
                },
                h => Assert.True(((IProcessMissPress)h.Node).ProcessMissPress)),
            ["CreateImage"] = new("CreateImage",
                (h, m) => ((ICreateMark)h.Node).Create(m),
                h => { },
                h => Assert.Null(((ICreateMark)h.Node).CreatedMark)),
        };

        public static TheoryData<string> Kinds() => [.. kinds.Keys];

        // マークの値は Out の中で読める
        static void CheckMarkValues(NodeHarness h, Kind kind)
        {
            var mark = new FakeMark();
            if (kind.Name != "ProcessEvaluate")
            {
                Assert.Equal(mark.Position, h.Get("Position"));
            }
            Assert.Equal(mark.Angle, h.Get("Rotation"));
            Assert.Equal(mark.Time, h.Get("Time"));
            Assert.Equal(mark.ReleaseTime, h.Get("ReleaseTime"));
            Assert.Equal((int)mark.ID, h.Get("MarkID"));
            Assert.Equal(mark.Type, h.Get("Type"));
            foreach (var flag in new[] { "IsLong", "IsAC", "HasSameTimingMark", "IsACFT", "IsScratch", "IsRight" })
            {
                Assert.Equal(true, h.Get(flag));
            }
            Assert.Equal(1, ((Dictionary<object, object>)h.Get("Parameters"))["p"]);
            Assert.Equal(mark.SlideScale, h.Get("SlideScale"));
            Assert.Equal(mark.SameTimingMarks, h.Get("SameTimingMarks"));
        }

        static NodeHarness Run(Kind kind, string variant, bool handled, IMarkInfo mark, params (string Name, object Value)[] inputs)
        {
            var harness = new NodeHarness(NodeType(kind.Name + variant));
            foreach (var (name, value) in inputs)
            {
                harness.Set(name, value);
            }
            harness.On("Out", h =>
            {
                CheckMarkValues(h, kind);
                kind.Decide(h);
                h.Set("Handled", handled);
            });
            harness.Run(h => kind.Call(h, mark));
            return harness;
        }

        [Theory]
        [MemberData(nameof(Kinds))]
        public void Any_HandlesEveryMark(string name)
        {
            var kind = kinds[name];

            var harness = Run(kind, "Any", true, new FakeMark());

            Assert.Equal(["Out"], harness.Fired);
            Assert.True(((IEvaluate)harness.Node).EvaluateHandled);
            kind.Check(harness);
        }

        [Theory]
        [MemberData(nameof(Kinds))]
        public void NotHandled_IsReported(string name)
        {
            var harness = Run(kinds[name], "Any", false, new FakeMark());

            Assert.Equal(["Out"], harness.Fired);
            Assert.False(((IEvaluate)harness.Node).EvaluateHandled);
        }

        [Theory]
        [MemberData(nameof(Kinds))]
        public void ByID_HandlesOnlyTargetMark(string name)
        {
            var kind = kinds[name];

            var target = Run(kind, "ByID", true, new FakeMark(), ("TargetID", 7));
            Assert.Equal(["Out"], target.Fired);
            Assert.True(((IEvaluate)target.Node).EvaluateHandled);

            var other = Run(kind, "ByID", true, new FakeMark { ID = 8 }, ("TargetID", 7));
            Assert.Empty(other.Fired);
            Assert.False(((IEvaluate)other.Node).EvaluateHandled);
        }

        [Theory]
        [MemberData(nameof(Kinds))]
        public void WithinTime_HandlesMarksInRange(string name)
        {
            var kind = kinds[name];

            // 両端も含む
            foreach (var time in new[] { 1f, 3f, 5f })
            {
                var inside = Run(kind, "WithinTime", true, new FakeMark(), ("StartTime", time == 1 ? 3f : 1f), ("EndTime", time == 5 ? 3f : 5f));
                Assert.Equal(["Out"], inside.Fired);
                Assert.True(((IEvaluate)inside.Node).EvaluateHandled);
            }

            var outside = Run(kind, "WithinTime", true, new FakeMark { Time = 6 }, ("StartTime", 1f), ("EndTime", 5f));
            Assert.Empty(outside.Fired);
            Assert.False(((IEvaluate)outside.Node).EvaluateHandled);
        }

        [Theory]
        [MemberData(nameof(Kinds))]
        public void Priorities_OrderIdBeforeTimeBeforeAny(string name)
        {
            int Priority(string variant) => ((IPriority)Activator.CreateInstance(NodeType(name + variant))).Priority;

            // 優先度の値は種類ごとの列挙型のもの。3 つは違う値になる
            Assert.Equal(3, new HashSet<int> { Priority("Any"), Priority("ByID"), Priority("WithinTime") }.Count);
        }

        [Theory]
        [MemberData(nameof(Kinds))]
        public void IsEvaluateRequired_ReturnsInput(string name)
        {
            var node = (IEvaluate)new NodeHarness(NodeType(name + "Any")).Set("EvaluateRequired", true).Node;

            Assert.True(node.IsEvaluateRequired());
        }

        [Theory]
        [MemberData(nameof(Kinds))]
        public void BeforeAnyMark_ValuesAreDefaults(string name)
        {
            var harness = new NodeHarness(NodeType(name + "Any"));

            Assert.Equal(0f, harness.Get("Time"));
            Assert.Equal(0, harness.Get("MarkID"));
            Assert.Equal(false, harness.Get("IsLong"));
            Assert.Null(harness.Get("Parameters"));
        }
    }
}
