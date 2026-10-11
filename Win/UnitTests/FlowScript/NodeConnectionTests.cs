using FlowScriptEngine;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using static UnitTests.FlowScript.ScriptRunner;

namespace UnitTests.FlowScript
{
    /// <summary>
    /// 名前を変えたメンバー（Replaced）を古い名前でつなぐテスト用のノードです。
    /// </summary>
    public class RenamedFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Test.Renamed"; }
        }

        [Replaced("OldFired")]
        public event FlowEventHandler Fired;

        [Replaced("OldText")]
        public string Text { private get; set; }

        public string Received { get; private set; }

        [Replaced("OldGo")]
        public void Go(FlowEventArgs e)
        {
            SetValue(nameof(Text));
            FireEvent(Fired);
        }

        public override object GetPropertyValue(string propertyName)
        {
            return propertyName == nameof(Received) ? Received : null;
        }

        protected override void SetPropertyValue(string propertyName, object value)
        {
            if (propertyName == nameof(Text))
            {
                Text = (string)value;
                Received = Text;
            }
        }
    }

    /// <summary>
    /// ノードのつなぎ方（古い名前、二重のつなぎ）と、エディターが使うノードの情報のテストです。
    /// </summary>
    public class NodeConnectionTests
    {
        [Fact]
        public void OldNames_StillConnect()
        {
            var s = new ScriptRunner();
            var entry = s.Entry();
            var renamed = s.Node(typeof(RenamedFlowSourceObject));
            var probe = s.Probe("fired");
            s.Flow(entry, "Start", renamed, "OldGo")
                .Flow(renamed, "OldFired", probe, "In")
                .Flow(s.Node("String.Value", ("Value", "text")), "Value", renamed, "OldText")
                .Flow(renamed, "Received", probe, "Value");

            var records = s.Run();

            Assert.Equal(["text"], Values(records, "fired"));
        }

        [Fact]
        public void TwoFlowsIntoOneInput_IsConnectError()
        {
            var s = new ScriptRunner();
            var probe = s.Probe("x");
            s.Flow(s.Node("String.Value", ("Value", "a")), "Value", probe, "Value")
                .Flow(s.Node("String.Value", ("Value", "b")), "Value", probe, "Value");
            RegisterNodes();

            var engine = new Engine(new StringReader(""));
            var error = Assert.Throws<ConnectErrorException>(() => engine.Load(new MemoryStream(Encoding.UTF8.GetBytes(s.ToXml())), false));

            Assert.Equal(("String.Value", "Test.Probe", "Value"), (error.SrcName, error.DestName, error.DestMemberName));
        }

        [Theory]
        [InlineData("NoSuchEvent", "In")]
        [InlineData("Start", "NoSuchMethod")]
        [InlineData("Value", "NoSuchProperty")]
        public void UnknownMembers_AreConnectErrors(string from, string to)
        {
            // ない名前へのつなぎは、読み込むときのエラーになる（黙って無視はしない）
            var s = new ScriptRunner();
            var source = from == "Value" ? s.Node("String.Value", ("Value", "a")) : s.Entry();
            s.Flow(source, from, s.Probe("p"), to);
            RegisterNodes();

            var engine = new Engine(new StringReader(""));
            var error = Assert.Throws<ConnectErrorException>(() => engine.Load(new MemoryStream(Encoding.UTF8.GetBytes(s.ToXml())), false));

            Assert.Equal((from, to), (error.SrcMemberName, error.DestMemberName));
        }

        [Fact]
        public void NodeInfo_ForEditor()
        {
            var node = new FlowScriptEngineBasic.FlowSourceObjects.Repeat.ForeachFlowSourceObject();

            Assert.Contains("Enumerable", node.InPropertyNames);
            Assert.Contains(node.InProperties, p => p.MemberInfo.Name == "Enumerable" && p.ReplacedNames.Contains("ArrayList"));
            Assert.Equal(["Iter", "Value"], node.OutPropertyNames.Order());
            Assert.Contains("In", node.InMethodNames);
            Assert.Contains(node.InMethods, m => m.MemberInfo.Name == "In");
            Assert.Equal(["Loop"], node.OutMethodNames);
            Assert.Single(node.OutMethods);
            Assert.False(string.IsNullOrEmpty(node.ToolTipText));
            Assert.False(string.IsNullOrEmpty(node.ToolTipTextKey));
            Assert.False(string.IsNullOrEmpty(node.Summary));
            Assert.Empty(node.WarningKey);
            Assert.Empty(node.Warning);

            // データを変えるノードには警告がある
            var dynamic = new FlowScriptEnginePPD.FlowSourceObjects.Mark.DynamicCreateFlowSourceObject();
            Assert.False(string.IsNullOrEmpty(dynamic.WarningKey));
            Assert.False(string.IsNullOrEmpty(dynamic.Warning));
            Assert.True(dynamic.ContainsAttribute(typeof(PPDFramework.ModifyDataAttribute)));
        }

#if DEBUG
        [Fact]
        public void AutoTest_RunsEveryMemberWithoutThrowing()
        {
            // デバッグ用の、ノードのメンバーをすべて動かす機能（結果はコンソールに出す）
            new FlowScriptEngineBasic.FlowSourceObjects.Logic.IfFlowSourceObject().AutoTest();
            new FlowScriptEngineBasic.FlowSourceObjects.Int32.AddFlowSourceObject().AutoTest();
            new FlowScriptEngineBasic.FlowSourceObjects.Repeat.BreakFlowSourceObject().AutoTest();
            new FlowScriptEngineBasic.FlowSourceObjects.ArrayList.RemoveAtFlowSourceObject().AutoTest();
        }
#endif
    }
}
