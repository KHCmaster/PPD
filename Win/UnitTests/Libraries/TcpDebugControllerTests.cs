using FlowScriptEngine;
using PPDCore;
using PPDFramework.Logger;
using System;
using System.IO;
using System.Text;
using System.Threading;
using UnitTests.FlowScript;
using Xunit;

namespace UnitTests.Libraries
{
    /// <summary>
    /// スクリプトのデバッグの通信（エディターの TcpDebugControllerHost と、スクリプトを動かす側の TcpDebugControllerClient）のテストです。
    /// </summary>
    /// <remarks>
    /// 待ち受けは localhost（IPAddress.Loopback）だけなので、ファイアウォールの確認は出ない。
    /// ポートは決まっている（54201）ので、エディターでデバッグしているときは動かせない。
    /// </remarks>
    public class TcpDebugControllerTests
    {
        [Fact]
        public void HostAndClient_DebugAScriptOverLocalhost()
        {
            Assert.False(TcpDebugControllerBase.IsListening(), "ほかでデバッグの待ち受けをしている");
            ScriptRunner.RegisterNodes();
            var s = new ScriptRunner();
            var entry = s.Entry();
            var seq = s.Node("Logic.Sequential");
            var first = s.Probe("first");
            var second = s.Probe("second");
            s.Flow(entry, "Start", seq, "In").Flow(seq, "Out0", first, "In").Flow(seq, "Out1", second, "In");

            var events = new TestEvents();
            using var host = new TcpDebugControllerHost(new() { ["a.fsml"] = [first], ["b.fsml"] = [] });
            host.OperationWaited += file => events.Add("waited " + file);
            host.OperationAccepted += file => events.Add("accepted " + file);
            host.SourceChanged += (text, file) => events.Add("source " + file);
            host.LogReceived += (type, text) => events.Add("log " + type + " " + text);
            host.Create();
            Assert.True(TcpDebugControllerBase.IsListening());

            using var controller = new DebugController();
            using var client = new TcpDebugControllerClient(controller);
            client.Create();
            // ブレークポイントはエディターから受け取る（ないファイルは入らない）
            Assert.True(client.Connected);
            Assert.Equal([first], client.BreakPoints["a.fsml"]);
            Assert.False(client.BreakPoints.ContainsKey("b.fsml"));

            // スクリプトのログはエディターに送る
            LogManager.Instance.AddLog(new LogInfo(LogLevel.Info, "hello"));
            events.WaitFor("log Info hello");

            var engine = new Engine(new StringReader(""));
            var manager = engine.Load(new MemoryStream(Encoding.UTF8.GetBytes(s.ToXml())), true, controller);
            manager.Initialize();
            foreach (var id in client.BreakPoints["a.fsml"])
            {
                manager.AddBreakPoint(id);
            }
            client.AddFlowSourceManager("a.fsml", manager);
            ProbeFlowSourceObject.Records.Clear();
            var thread = new Thread(engine.Start) { IsBackground = true };
            thread.Start();

            // ブレークポイントで止まる
            events.WaitFor("waited a.fsml");
            Assert.Empty(ProbeFlowSourceObject.Records);
            // 止まっている間に、エディターで選んだノードの値を見る
            host.ChangeSource("a.fsml", second);
            events.WaitFor("source a.fsml");
            // 次のノードまで進める
            host.StepIn();
            events.WaitFor("accepted a.fsml");
            events.WaitFor("waited a.fsml");
            Assert.Single(ProbeFlowSourceObject.Records);
            host.Continue();
            events.WaitFor("accepted a.fsml");

            Assert.True(thread.Join(TestEvents.Timeout));
            Assert.Equal(["first", "second"], ProbeFlowSourceObject.Records.ConvertAll(r => r.Label));

            client.Close();
            Assert.False(client.Connected);
            host.Close();
            Assert.False(TcpDebugControllerBase.IsListening());
        }

        [Fact]
        public void Client_WithoutHost_IsNotConnected()
        {
            Assert.False(TcpDebugControllerBase.IsListening(), "ほかでデバッグの待ち受けをしている");
            using var controller = new DebugController();
            using var client = new TcpDebugControllerClient(controller);

            client.Create();

            Assert.False(client.Connected);
            Assert.Empty(client.BreakPoints);
        }
    }
}
