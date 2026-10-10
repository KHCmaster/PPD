using FlowScriptControl.Classes;
using FlowScriptControlTest.Models;
using FlowScriptDrawControl.Model;
using FlowScriptEngine;
using FlowScriptEngineConsole.FlowSourceObjects.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Xunit;

namespace UnitTests.FlowScriptControlTest
{
    /// <summary>
    /// スクリプトのノード（Basic と Console）を使えるようにします。
    /// </summary>
    static class FlowNodes
    {
        static readonly Lazy<AssemblyAndType[]> types = new(() =>
        [
            .. FlowSourceEnumerator.EnumerateFromFile(typeof(FlowScriptEngineBasic.FlowSourceObjects.Misc.EntryPointFlowSourceObject).Assembly.Location, [typeof(FlowSourceObjectBase)]),
            .. FlowSourceEnumerator.EnumerateFromFile(typeof(WriteLineFlowSourceObject).Assembly.Location, [typeof(FlowSourceObjectBase)]),
        ]);

        public static AssemblyAndType Get<T>()
        {
            return types.Value.First(t => t.Type == typeof(T));
        }

        public static void Load()
        {
            _ = types.Value;
        }

        /// <summary>
        /// 始まったら文字を 1 行書くスクリプトです。
        /// </summary>
        public static MemoryStream WriteLineScript(string text)
        {
            var xml = $"""
                <Root>
                  <Sources>
                    <Source Name="FlowScriptEngineBasic.FlowSourceObjects.Misc.EntryPointFlowSourceObject" FullName="FlowScriptEngineBasic.FlowSourceObjects.Misc.EntryPointFlowSourceObject" ID="1" X="0" Y="0" />
                    <Source Name="FlowScriptEngineConsole.FlowSourceObjects.Console.WriteLineFlowSourceObject" FullName="FlowScriptEngineConsole.FlowSourceObjects.Console.WriteLineFlowSourceObject" ID="2" X="100" Y="0">
                      <Value Name="Text" Value="{text}" />
                    </Source>
                  </Sources>
                  <Flows>
                    <Flow SrcID="1" SrcName="Start" DestID="2" DestName="In" />
                  </Flows>
                  <Comments />
                  <Scopes />
                </Root>
                """;
            return new MemoryStream(Encoding.UTF8.GetBytes(xml));
        }
    }

    public class ExecuteInfoTests
    {
        private static Source CreateSource(int id)
        {
            FlowNodes.Load();
            return new Source(FlowNodes.Get<WriteLineFlowSourceObject>()) { Id = id };
        }

        [Fact]
        public void BreakPointChanges_AreForwarded()
        {
            var source = CreateSource(3);
            var info = new ExecuteInfo(new MemoryStream(), [1], null, [source]);
            var added = new List<int>();
            var removed = new List<int>();
            info.BreakPointAdded += (sender, id) => added.Add(id);
            info.BreakPointRemoved += (sender, id) => removed.Add(id);

            source.IsBreakPointSet = true;
            source.IsBreakPointSet = false;

            Assert.Equal([3], added);
            Assert.Equal([3], removed);
            Assert.Equal([1], info.BreakPoints);
        }

        [Fact]
        public void BreakPointChanges_WithoutHandlers_DoNotThrow()
        {
            var source = CreateSource(3);
            var info = new ExecuteInfo(new MemoryStream(), [], null, [source]);
            var removed = new List<int>();
            info.BreakPointRemoved += (sender, id) => removed.Add(id);

            // 以前は、追加のハンドラーがないと削除のハンドラーを呼んでいた（それもないと例外になっていた）
            source.IsBreakPointSet = true;
            Assert.Empty(removed);

            var other = new ExecuteInfo(new MemoryStream(), [], null, [CreateSource(4)]);
            other.Sources[0].IsBreakPointSet = true;
            other.Sources[0].IsBreakPointSet = false;
        }

        [Fact]
        public void RemoveEventHandler_StopsForwarding()
        {
            var source = CreateSource(3);
            var info = new ExecuteInfo(new MemoryStream(), [], null, [source]);
            var added = 0;
            info.BreakPointAdded += (sender, id) => added++;

            info.RemoveEventHandler();
            source.IsBreakPointSet = true;

            Assert.Equal(0, added);
        }
    }

    /// <summary>
    /// スクリプトを実際に動かすテストです。コンソールの出力を横取りするので、ほかのテストと同時には動かさない。
    /// </summary>
    public class ExecutorTests
    {
        private static (string Output, bool Aborted) Run(Executor executor)
        {
            var output = new StringBuilder();
            var finished = new ManualResetEventSlim();
            var aborted = false;
            executor.Finished += finished.Set;
            executor.Aborted += () => aborted = true;
            var writer = new ConsoleWriter();
            writer.Output += text =>
            {
                lock (output) output.Append(text);
            };
            try
            {
                executor.Execute();
                Assert.True(finished.Wait(TimeSpan.FromSeconds(30)));
            }
            finally
            {
                writer.Dispose();
            }
            // 転送のスレッドが残りを出力するのを待つ
            SpinWait.SpinUntil(() => { lock (output) return output.Length > 0; }, TimeSpan.FromSeconds(2));
            lock (output) return (output.ToString(), aborted);
        }

        [Fact]
        public void Execute_RunsScriptAndReportsFinished()
        {
            FlowNodes.Load();
            var info = new ExecuteInfo(FlowNodes.WriteLineScript("hello flow"), [], null, []);
            var executor = new Executor([info], "");

            var (output, aborted) = Run(executor);

            Assert.Contains("hello flow", output);
            Assert.False(aborted);
            Assert.False(executor.IsExecuting);
            Assert.NotNull(executor.Controller);
        }

        [Fact]
        public void Execute_WithStdin_RunsScript()
        {
            FlowNodes.Load();
            var executor = new Executor([new ExecuteInfo(FlowNodes.WriteLineScript("with stdin"), [], null, [])], "input line");

            var (output, _) = Run(executor);

            Assert.Contains("with stdin", output);
        }

        [Fact]
        public void Execute_BrokenScript_WritesErrorAndFinishes()
        {
            var executor = new Executor([new ExecuteInfo(new MemoryStream(Encoding.UTF8.GetBytes("<Root><Sources><Source FullName=\"No.Such.Node\" ID=\"1\" /></Sources></Root>")), [], null, [])], "");

            var (output, aborted) = Run(executor);

            Assert.Contains("No.Such.Node", output);
            Assert.False(aborted);
        }

        [Fact]
        public void Abort_BeforeExecute_DoesNothing()
        {
            var executor = new Executor([], "");

            executor.Abort();

            Assert.False(executor.IsExecuting);
        }
    }

    public class ConsoleWriterTests
    {
        [Fact]
        public void ForwardsConsoleOutputUntilStopped()
        {
            var original = Console.Out;
            var output = new StringBuilder();
            var writer = new ConsoleWriter();
            writer.Output += text =>
            {
                lock (output) output.Append(text);
            };

            Console.Write("forwarded");
            Assert.True(SpinWait.SpinUntil(() => { lock (output) return output.ToString().Contains("forwarded"); }, TimeSpan.FromSeconds(5)));
            Assert.Equal(Encoding.UTF8, writer.Encoding);

            writer.Stop();
            Assert.Same(original, Console.Out);
            writer.Dispose();
        }
    }

    public class FlowScriptLanguageProviderTests
    {
        [Fact]
        public void Load_WithoutFiles_ReturnsNull()
        {
            using var folder = new TempFolder(setCurrentDirectory: true);

            Assert.Null(FlowScriptLanguageProvider.Load());
        }

        [Fact]
        public void Load_ProvidesAllTexts()
        {
            using var folder = new TempFolder(setCurrentDirectory: true);
            AppFiles.CopyTo("FlowScriptControlTest", folder);

            ILanguageProvider provider = FlowScriptLanguageProvider.Load();

            Assert.NotNull(provider);
            var texts = typeof(ILanguageProvider).GetProperties().Where(p => p.PropertyType == typeof(string)).ToList();
            Assert.NotEmpty(texts);
            Assert.All(texts, p => Assert.False(string.IsNullOrEmpty((string)p.GetValue(provider)), p.Name));
            Assert.False(string.IsNullOrEmpty(provider.GetFlowSourceToolTipText("Console_WriteLine_In")));
        }
    }
}
