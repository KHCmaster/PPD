using FlowScriptEngine;
using PPDCoreModel.Data;
using PPDEditorCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Xml.Linq;
using Xunit;

namespace UnitTests.FlowScript
{
    /// <summary>
    /// エディターのスクリプトでマークとレイヤーを扱うノード（PPDEditor.Mark、PPDEditor.Layer）のテストです。
    /// </summary>
    /// <remarks>
    /// ノードはマークを直接変えず、レイヤーの IChangeMarkPropertyManager に頼む（エディターが元に戻せるようにする）。
    /// </remarks>
    public class EditorMarkNodeTests
    {
        sealed class FakeChanges : IChangeMarkPropertyManager
        {
            public List<string> Calls { get; } = [];
            public void ChangeMarkAngle(IEditorMarkInfo mark, float angle) => Calls.Add($"angle {mark.ID} {angle}");
            public void ChangeMarkPosition(IEditorMarkInfo mark, Vector2 position) => Calls.Add($"position {mark.ID} {position}");
            public void ChangeMarkType(IEditorMarkInfo mark, MarkType markType) => Calls.Add($"type {mark.ID} {markType}");
            public void ChangeMarkTime(IEditorMarkInfo mark, float time) => Calls.Add($"time {mark.ID} {time}");
            public void ChangeParameter(IEditorMarkInfo mark, string key, string value) => Calls.Add($"parameter {mark.ID} {key}={value}");
            public void RemoveParameter(IEditorMarkInfo mark, string key) => Calls.Add($"remove parameter {mark.ID} {key}");
            public void AddMark(Vector2 position, float angle, float time, MarkType markType) => Calls.Add($"add {position} {angle} {time} {markType}");
            public void AddExMark(Vector2 position, float angle, float time, float endTime, MarkType markType) => Calls.Add($"add long {position} {angle} {time}-{endTime} {markType}");
            public void AssignID(IEditorMarkInfo mark) => Calls.Add($"assign {mark.Time}");
            public void UnassignID(IEditorMarkInfo mark) => Calls.Add($"unassign {mark.ID}");
            public void Remove(IEditorMarkInfo mark) => Calls.Add($"remove {mark.ID}");
        }

        sealed class FakeLayer : ILayer
        {
            public FakeLayer()
            {
                Marks = [new FakeMark(this, 1, 1.5f), new FakeMark(this, 2, 2.5f, isLong: true)];
            }

            public IEditorMarkInfo[] SelectedMarks => [Marks[1]];
            public IEditorMarkInfo[] Marks { get; }
            public IEditorMarkInfo SelectedMark => Marks[1];
            public IChangeMarkPropertyManager ChangeMarkPropertyManager { get; } = new FakeChanges();
            public bool IsSelected => true;
            public List<string> Calls => ((FakeChanges)ChangeMarkPropertyManager).Calls;
        }

        sealed class FakeMark(ILayer layer, uint id, float time, bool isLong = false) : IEditorMarkInfo
        {
            public ILayer Layer => layer;
            public Vector2 ColorPosition { get; set; }
            public Vector2 Position => new(10 * id, 20 * id);
            public float Angle => 0.25f * id;
            public float Time => time;
            public uint ID => id;
            public MarkType Type => MarkType.Triangle;
            public bool IsLong => isLong;
            public bool IsAC => false;
            public bool HasSameTimingMark => false;
            public float ReleaseTime => isLong ? time + 1 : 0;
            public bool IsACFT => false;
            public bool IsScratch => false;
            public bool IsRight => false;
            public Dictionary<object, object> Parameters { get; } = new() { ["p"] = "v" };
            public float SlideScale => 1;
            public int SameTimingMarks => 0;
        }

        static NodeHarness Node(string name, params (string Name, object Value)[] inputs)
        {
            var type = typeof(FlowScriptEnginePPDEditor.ToolTipText).Assembly.GetType($"FlowScriptEnginePPDEditor.FlowSourceObjects.{name}FlowSourceObject", true);
            var harness = new NodeHarness(type);
            foreach (var (n, v) in inputs)
            {
                harness.Set(n, v);
            }
            return harness;
        }

        [Fact]
        public void MarkNodes_AskLayerToChangeMark()
        {
            var layer = new FakeLayer();
            var mark = layer.Marks[0];
            void Ok(string name, params (string, object)[] inputs) => Assert.Equal(["Success"], Node("Mark." + name, [("Mark", mark), .. inputs]).Invoke().Fired);

            Ok("SetPosition", ("Position", new Vector2(3, 4)));
            Ok("SetRotation", ("Rotation", 0.5f));
            Ok("SetTime", ("Time", 9f));
            Ok("SetType", ("Type", MarkType.Up));
            Ok("SetParameter", ("Key", "k"), ("Value", "v"));
            Ok("RemoveParameter", ("Key", "k"));
            Ok("AssignId");
            Ok("UnassignId");
            Ok("Remove");

            Assert.Equal([
                $"position 1 {new Vector2(3, 4)}",
                "angle 1 0.5",
                "time 1 9",
                "type 1 Up",
                "parameter 1 k=v",
                "remove parameter 1 k",
                "assign 1.5",
                "unassign 1",
                "remove 1",
            ], layer.Calls);
        }

        [Theory]
        [InlineData("SetPosition")]
        [InlineData("SetRotation")]
        [InlineData("SetTime")]
        [InlineData("SetType")]
        [InlineData("SetParameter")]
        [InlineData("RemoveParameter")]
        [InlineData("AssignId")]
        [InlineData("UnassignId")]
        [InlineData("Remove")]
        public void MarkNodes_WithoutMark_Fail(string name)
        {
            Assert.Equal(["Failed"], Node("Mark." + name).Invoke().Fired);
        }

        [Fact]
        public void MarkValue_ReadsMark()
        {
            var mark = new FakeLayer().Marks[1];
            var value = Node("Mark.Value", ("Mark", mark));

            Assert.Equal(new Vector2(20, 40), value.Get("Position"));
            Assert.Equal(0.5f, value.Get("Rotation"));
            Assert.Equal(2.5f, value.Get("Time"));
            Assert.Equal(2, value.Get("ID"));
            Assert.Equal(MarkType.Triangle, value.Get("Type"));
            Assert.Equal(true, value.Get("IsLong"));
            Assert.Equal(3.5f, value.Get("ReleaseTime"));
            Assert.Equal("v", ((Dictionary<object, object>)value.Get("Parameters"))["p"]);

            var empty = Node("Mark.Value");
            Assert.Equal(Vector2.Zero, empty.Get("Position"));
            Assert.Equal(0f, empty.Get("Rotation"));
            Assert.Equal(0f, empty.Get("Time"));
            Assert.Equal(0, empty.Get("ID"));
            Assert.Equal((MarkType)0, empty.Get("Type"));
            Assert.Equal(false, empty.Get("IsLong"));
            Assert.Equal(0f, empty.Get("ReleaseTime"));
            Assert.Null(empty.Get("Parameters"));
        }

        [Fact]
        public void LayerNodes_AddMarksAndReadLayer()
        {
            var layer = new FakeLayer();

            Assert.Equal(["Success"], Node("Layer.AddMark", ("Layer", layer), ("Position", new Vector2(1, 2)), ("Rotation", 0.5f), ("Time", 3f), ("Type", MarkType.R)).Invoke().Fired);
            Assert.Equal(["Success"], Node("Layer.AddLongMark", ("Layer", layer), ("Position", new Vector2(1, 2)), ("Rotation", 0.5f), ("Time", 3f), ("ReleaseTime", 4f), ("Type", MarkType.L)).Invoke().Fired);
            Assert.Equal([$"add {new Vector2(1, 2)} 0.5 3 R", $"add long {new Vector2(1, 2)} 0.5 3-4 L"], layer.Calls);

            var value = Node("Layer.Value", ("Layer", layer));
            Assert.Equal(2, ((object[])value.Get("Marks")).Length);
            Assert.Same(layer.Marks[1], Assert.Single((object[])value.Get("SelectedMarks")));
            Assert.Same(layer.Marks[1], value.Get("SelectedMark"));
            Assert.Equal(true, value.Get("IsSelected"));

            var empty = Node("Layer.Value");
            Assert.Empty((object[])empty.Get("Marks"));
            Assert.Empty((object[])empty.Get("SelectedMarks"));
            Assert.Null(empty.Get("SelectedMark"));
            Assert.Equal(false, empty.Get("IsSelected"));
        }

        [Fact]
        public void LayerAll_ReadsEditorLayers()
        {
            var layers = new ILayer[] { new FakeLayer(), new FakeLayer() };
            var all = Node("Layer.All");
            all.Items["AllLayers"] = layers;
            all.Initialize();

            Assert.Equal(layers, (object[])all.Get("All"));
            Assert.Null(Node("Layer.All").Initialize().Get("All"));
        }

        [Fact]
        public void IsTypeNodes_AcceptEditorInterfaces()
        {
            var layer = new FakeLayer();

            Assert.Equal(true, Node("Mark.IsType", ("Target", layer.Marks[0])).Get("Value"));
            Assert.Equal(true, Node("Layer.IsType", ("Target", layer)).Get("Value"));
            Assert.Equal(false, Node("Layer.IsType", ("Target", layer.Marks[0])).Get("Value"));
        }

        sealed class PosAndAngle(Vector2? position, float? rotation) : IPosAndAngle
        {
            public Vector2? Position => position;
            public float? Rotation => rotation;
        }

        [Fact]
        public void PosAndRotation_Value()
        {
            var full = Node("Mark.PosAndRotation.Value", ("Value", new PosAndAngle(new Vector2(1, 2), 0.5f)));
            Assert.Equal(new Vector2(1, 2), full.Get("Position"));
            Assert.Equal(0.5f, full.Get("Rotation"));
            Assert.Equal(true, full.Get("HasPosition"));
            Assert.Equal(true, full.Get("HasRotation"));

            // 位置か角度のないものは 0 で、Has〜 が false
            var partial = Node("Mark.PosAndRotation.Value", ("Value", new PosAndAngle(null, null)));
            Assert.Equal(Vector2.Zero, partial.Get("Position"));
            Assert.Equal(0f, partial.Get("Rotation"));
            Assert.Equal(false, partial.Get("HasPosition"));
            Assert.Equal(false, partial.Get("HasRotation"));

            var empty = Node("Mark.PosAndRotation.Value");
            Assert.Equal(Vector2.Zero, empty.Get("Position"));
            Assert.Equal(false, empty.Get("HasRotation"));
        }

        sealed class FakeLoader : IPosAndAngleLoader
        {
            public IPosAndAngle[] Load(string fileName) => fileName == "ok" ? [new PosAndAngle(Vector2.One, 1)] : null;
        }

        [Fact]
        public void PosAndRotation_Load()
        {
            NodeHarness Load(string fileName, bool withLoader = true)
            {
                var load = Node("Mark.PosAndRotation.Load", ("FileName", fileName));
                if (withLoader)
                {
                    load.Items["PosAndAngleLoader"] = new FakeLoader();
                }
                return load.Initialize().Invoke();
            }

            var ok = Load("ok");
            Assert.Equal(["Success"], ok.Fired);
            Assert.Single((object[])ok.Get("Values"));
            Assert.Equal(["Failed"], Load("missing").Fired);
            Assert.Equal(["Failed"], Load("ok", withLoader: false).Fired);
        }

        [Fact]
        public void ParameterPresets()
        {
            var path = Path.GetTempFileName();
            PPDEditorCommon.ParameterPreset preset;
            try
            {
                File.WriteAllText(path, "<Root><Parameter Key=\"a\" Value=\"1\" /><Parameter Key=\"b\" Value=\"2\" /></Root>");
                preset = new PPDEditorCommon.ParameterPreset(path, "preset");
            }
            finally
            {
                File.Delete(path);
            }

            var value = Node("Mark.ParameterPreset.Value", ("Value", preset));
            Assert.Equal("preset", value.Get("PresetName"));
            Assert.Equal(new object[] { new KeyValuePair<object, object>("a", "1"), new KeyValuePair<object, object>("b", "2") }, (object[])value.Get("Parameters"));
            Assert.Equal("", Node("Mark.ParameterPreset.Value").Get("PresetName"));
            Assert.Null(Node("Mark.ParameterPreset.Value").Get("Parameters"));

            var all = Node("Mark.ParameterPreset.All");
            all.Items["ParameterPresets"] = new[] { preset };
            Assert.Same(preset, Assert.Single((object[])all.Initialize().Get("All")));
            Assert.Null(Node("Mark.ParameterPreset.All").Initialize().Get("All"));
        }

        static XElement Serialize(Type serializerType, object value)
        {
            var element = new XElement("Value");
            ((TypeSerializerBase)Activator.CreateInstance(serializerType)).Serialize(new Serializer(), element, value);
            return element;
        }

        static string[] Names(XElement element) => [.. element.Elements("Property").Select(e => e.Attribute("Name").Value)];

        [Fact]
        public void DebuggerSerializers_WriteEditorObjects()
        {
            var layer = new FakeLayer();

            Assert.Equal(["SelectedMarks", "Marks", "SelectedMark", "IsSelected"], Names(Serialize(typeof(FlowScriptEnginePPDEditor.TypeSerializers.LayerTypeSerializer), layer)));
            Assert.Equal(["Position", "Rotation", "Time", "ID", "Type", "IsLong", "ReleaseTime", "Parameters"],
                Names(Serialize(typeof(FlowScriptEnginePPDEditor.TypeSerializers.MarkTypeSerializer), layer.Marks[0])));
            // 位置のないものも書ける
            Assert.Equal(["Position", "Rotation", "HasPosition", "HasRotation"],
                Names(Serialize(typeof(FlowScriptEnginePPDEditor.TypeSerializers.PosAndRotationTypeSerializer), new PosAndAngle(null, null))));
        }
    }
}
