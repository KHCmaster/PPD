using FlowScriptEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Xml.Linq;
using Xunit;

namespace UnitTests.FlowScript
{
    /// <summary>
    /// スクリプトのデバッグ（ブレークポイントで止めて値を見る）と、値の表示（Serializer）のテストです。
    /// </summary>
    public class DebuggerTests
    {
        static void RegisterSerializers()
        {
            ScriptRunner.RegisterNodes();
            _ = FlowSourceEnumerator.EnumerateFromFile(typeof(FlowScriptEngineSlimDX.ToolTipText).Assembly.Location, []).ToList();
        }

        [Fact]
        public void Serialize_NullArraysAndRegisteredTypes()
        {
            RegisterSerializers();
            var serializer = new Serializer();
            var root = new XElement("Test");

            serializer.Serialize(root, null);
            serializer.Serialize(root, new object[] { 1, null });
            serializer.Serialize(root, new Vector2(1.5f, 2));
            serializer.Serialize(root, "text");

            var values = root.Elements("Value").ToList();
            // 以前は null で例外になり、値が出なかった
            Assert.Equal(("(null)", "(null)"), (values[0].Attribute("Type").Value, values[0].Value));
            Assert.Equal("System.Object[]", values[1].Attribute("Type").Value);
            Assert.Equal(["[0]", "[1]"], values[1].Elements("Property").Select(e => e.Attribute("Name").Value));
            Assert.Equal("(null)", values[1].Elements("Property").Last().Element("Value").Value);
            Assert.Equal(["X", "Y"], values[2].Elements("Property").Select(e => e.Attribute("Name").Value));
            Assert.Equal("1.5", values[2].Elements("Property").First().Element("Value").Value);
            Assert.Equal("text", values[3].Value);
            Assert.Equal("<Root />", serializer.ToString());
        }

        // Object.Null の値を記録するスクリプト。Object.Null で止める
        static (string Xml, int NullNode) NullScript()
        {
            var s = new ScriptRunner();
            var entry = s.Entry();
            var seq = s.Node("Logic.Sequential");
            var set = s.Node("Scope.SetValue", ("ValueName", "empty"));
            var nil = s.Node("Object.Null");
            var probe = s.Probe("value");
            s.Flow(entry, "Start", seq, "In")
                .Flow(seq, "Out0", set, "In").Flow(nil, "Value", set, "Value")
                .Flow(seq, "Out1", probe, "In").Flow(nil, "Value", probe, "Value");
            return (s.ToXml(), nil);
        }

        [Fact]
        public void BreakPoint_StopsAndShowsValues()
        {
            ScriptRunner.RegisterNodes();
            var (xml, nullNode) = NullScript();
            using var controller = new DebugController();
            var shown = new List<string>();
            var waited = 0;
            var accepted = 0;
            controller.SourceChanged += (text, manager) => shown.Add(text);
            controller.OperationWaited += manager =>
            {
                waited++;
                Assert.True(controller.Waiting);
                // 止まっている間に、ほかのノードの値も見られる
                controller.ChangeSource(nullNode);
                controller.Continue();
            };
            controller.OperationAccepted += manager => accepted++;
            var engine = new Engine(new StringReader(""));
            var manager = engine.Load(new MemoryStream(Encoding.UTF8.GetBytes(xml)), true, controller);
            manager.Initialize();
            manager.AddBreakPoint(nullNode);

            engine.Start();

            Assert.True(waited >= 1);
            Assert.Equal(waited, accepted);
            var source = XDocument.Parse(shown[0]);
            var value = source.Root.Element("Source").Elements("Property").Single(p => p.Attribute("Name").Value == "Value");
            Assert.Equal("(null)", value.Element("Value").Value);
            Assert.True(shown.Count >= 2);
        }

        [Fact]
        public void RemovedBreakPoint_DoesNotStop()
        {
            ScriptRunner.RegisterNodes();
            var (xml, nullNode) = NullScript();
            using var controller = new DebugController();
            var waited = 0;
            controller.OperationWaited += manager =>
            {
                waited++;
                controller.Continue();
            };
            var engine = new Engine(new StringReader(""));
            var manager = engine.Load(new MemoryStream(Encoding.UTF8.GetBytes(xml)), true, controller);
            manager.Initialize();
            manager.AddBreakPoint(nullNode);
            manager.RemoveBreakPoint(nullNode);

            engine.Start();

            Assert.Equal(0, waited);
        }

        [Fact]
        public void StepIn_StopsAtNextNodeAndAbortStops()
        {
            ScriptRunner.RegisterNodes();
            var (xml, nullNode) = NullScript();
            using var controller = new DebugController();
            var waited = 0;
            controller.OperationWaited += manager =>
            {
                waited++;
                if (waited == 1)
                {
                    controller.StepIn();
                }
                else
                {
                    controller.Abort();
                }
            };
            var engine = new Engine(new StringReader(""));
            var manager = engine.Load(new MemoryStream(Encoding.UTF8.GetBytes(xml)), true, controller);
            manager.Initialize();
            manager.AddBreakPoint(nullNode);
            ProbeFlowSourceObject.Records.Clear();

            engine.Start();

            Assert.Equal(2, waited);
            Assert.True(controller.IsAbortRequested);
            Assert.True(manager.IsAborted);
        }
    }
}
