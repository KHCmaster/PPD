using PPDCoreModel.Data;
using PPDEditor;
using PPDEditor.Command.PPDSheet;
using PPDEditor.FlowScript;
using PPDFramework;
using System;
using System.Linq;
using System.Numerics;
using Xunit;

namespace UnitTests.Graphics
{
    /// <summary>
    /// エディターのスクリプトから譜面を変える層（Layer と ChangeMarkPropertyManager）のテストです。
    /// 変更はすぐにスクリプトに見え、Execute でまとめて譜面に入れ、1 回で元に戻せます。
    /// </summary>
    [Collection(GraphicsCollection.Name)]
    public sealed class EditorLayerTests : IDisposable
    {
        readonly EditorMarkEnvironment environment;
        readonly IDProvider ids = new(10);
        readonly PPDSheet sheet;

        public EditorLayerTests(GraphicsDevice graphics)
        {
            environment = new EditorMarkEnvironment(graphics);
            // スクリプトの層は ID の複製を使い、譜面は元の ID を使う（エディターと同じ）
            var real = ids.Clone();
            sheet = new PPDSheet { GetVisibleRows = () => [.. Enumerable.Range(0, 10)], NextID = real.Next };
        }

        public void Dispose()
        {
            environment.Dispose();
        }

        [Fact]
        public void Layer_ShowsMarksAndSelection()
        {
            var first = sheet.AddMark(1, 100, 200, 0.5f, (int)ButtonType.Circle, 3, [new("k", "v")], false);
            var hold = sheet.AddExMark(2, 3, (int)ButtonType.Cross, false);
            sheet.SelectMark(1, (int)ButtonType.Circle, 0);

            var layer = new Layer(sheet, ids.Clone(), true);

            Assert.True(layer.IsSelected);
            Assert.Equal(2, layer.Marks.Length);
            var info = (EditorMarkInfo)layer.Marks[0];
            Assert.Same(first, info.Mark);
            Assert.Equal((new Vector2(100, 200), 0.5f, 1f, MarkType.Circle, false), (info.Position, info.Angle, info.Time, info.Type, info.IsLong));
            Assert.Equal("v", info.Parameters["k"]);
            // スクリプトにもマークの ID が見える
            Assert.Equal(3u, info.ID);
            Assert.Same(layer, info.Layer);
            var holdInfo = layer.Marks[1];
            Assert.Equal((true, 3f), (holdInfo.IsLong, holdInfo.ReleaseTime));
            Assert.Same(layer.Marks[0], layer.SelectedMark);
            Assert.Empty(layer.SelectedMarks);

            // 範囲で選んでいるときは、選んだマーク
            sheet.RecStart = new CustomPoint(0, 0);
            sheet.RecEnd = new CustomPoint(5, 9);
            var areaLayer = new Layer(sheet, ids.Clone(), false);
            Assert.Equal(2, areaLayer.SelectedMarks.Length);
            Assert.Null(areaLayer.SelectedMark);
            Assert.Same(hold, ((EditorMarkInfo)areaLayer.SelectedMarks[1]).Mark);
        }

        [Fact]
        public void Changes_AreSeenAtOnceAndAppliedTogether()
        {
            var mark = sheet.AddMark(1, (int)ButtonType.Circle, false);
            var other = sheet.AddMark(2, (int)ButtonType.Circle, false);
            sheet.ClearHistory();
            var layer = new Layer(sheet, ids.Clone(), true);
            var info = layer.Marks[0];
            var manager = layer.ChangeMarkPropertyManager;

            manager.ChangeMarkPosition(info, new Vector2(10, 20));
            manager.ChangeMarkAngle(info, 1);
            manager.ChangeParameter(info, "a", "1");
            manager.ChangeParameter(info, "b", "2");
            manager.RemoveParameter(info, "b");
            manager.AddMark(new Vector2(5, 5), 0, 4, MarkType.Square);
            manager.AddExMark(new Vector2(5, 5), 0, 5, 6, MarkType.Triangle);
            manager.Remove(layer.Marks[1]);

            // スクリプトにはすぐ見えるが、譜面はまだ変わらない
            Assert.Equal((new Vector2(10, 20), 1f), (info.Position, info.Angle));
            Assert.Equal(["a"], info.Parameters.Keys);
            Assert.Equal(new Vector2(400, 225), mark.Position);

            layer.Execute();

            Assert.Equal((new Vector2(10, 20), 1f, "1"), (mark.Position, mark.Rotation, mark["a"]));
            Assert.Null(mark["b"]);
            Assert.Single(sheet.Data[(int)ButtonType.Square]);
            Assert.IsType<ExMark>(Assert.Single(sheet.Data[(int)ButtonType.Triangle].Values));
            Assert.DoesNotContain(other, sheet.Data[(int)ButtonType.Circle].Values);

            // まとめて 1 回で戻る
            sheet.Undo();
            Assert.Equal(new Vector2(400, 225), mark.Position);
            Assert.Equal(2, sheet.Data[(int)ButtonType.Circle].Count);
            Assert.Empty(sheet.Data[(int)ButtonType.Square]);
            Assert.False(sheet.CanUndo);

            // 何も変えなければ、何もしない
            new Layer(sheet, ids.Clone(), true).Execute();
            Assert.False(sheet.CanUndo);
        }

        [Fact]
        public void TypeAndTime_OfAMarkThatIsNotSelected()
        {
            var mark = sheet.AddMark(1, (int)ButtonType.Circle, false);
            sheet.AddMark(1, (int)ButtonType.Cross);
            var layer = new Layer(sheet, ids.Clone(), true);
            var info = layer.Marks.Single(m => ((EditorMarkInfo)m).Mark == mark);

            // 選んでいる行（×）とは別の行（○）のマークを変える
            layer.ChangeMarkPropertyManager.ChangeMarkTime(info, 3);
            layer.ChangeMarkPropertyManager.ChangeMarkType(info, MarkType.Square);
            Assert.Equal((3f, MarkType.Square), (info.Time, info.Type));
            layer.Execute();

            Assert.Equal((3f, ButtonType.Square), (mark.Time, mark.Type));
            Assert.Same(mark, sheet.Data[(int)ButtonType.Square][3]);
            Assert.Equal([1f], sheet.Data[(int)ButtonType.Cross].Keys);
        }

        [Fact]
        public void Executor_RunsScriptWithLayers()
        {
            sheet.AddMark(1, (int)ButtonType.Circle, false);
            var layers = new[] { new Layer(sheet, ids.Clone(), true) };
            var s = new UnitTests.FlowScript.ScriptRunner();
            var probe = s.Probe("layers");
            s.Flow(s.Entry(), "Start", probe, "In").Flow(s.Node(typeof(FlowScriptEnginePPDEditor.FlowSourceObjects.Layer.AllFlowSourceObject)), "All", probe, "Value");
            UnitTests.FlowScript.ScriptRunner.RegisterNodes();
            // エディターのノードも使えるようにする
            _ = FlowScriptEngine.FlowSourceEnumerator.EnumerateFromFile(typeof(FlowScriptEnginePPDEditor.FlowSourceObjects.Layer.AllFlowSourceObject).Assembly.Location,
                [typeof(FlowScriptEngine.FlowSourceObjectBase)]).ToList();
            using var folder = new TempFolder();
            var path = folder.Write("script.fsml", s.ToXml());
            UnitTests.FlowScript.ProbeFlowSourceObject.Records.Clear();

            var executor = new Executor(path, layers);
            using var finished = new System.Threading.ManualResetEventSlim();
            executor.Finished += (sender, e) => finished.Set();
            executor.Execute();

            Assert.True(finished.Wait(10000));
            Assert.False(executor.HasError, executor.ErrorText);
            Assert.False(executor.IsWorking);
            Assert.Same(layers, Assert.Single(UnitTests.FlowScript.ProbeFlowSourceObject.Records).Value);
            Assert.Same(layers, executor.Layers);
            executor.Kill();
        }

        [Fact]
        public void Executor_StopsAtEditorBreakPointAndCanBeKilled()
        {
            Assert.False(PPDCore.TcpDebugControllerBase.IsListening(), "ほかでデバッグの待ち受けをしている");
            var s = new UnitTests.FlowScript.ScriptRunner();
            var seq = s.Node("Logic.Sequential");
            var probe = s.Probe("stopped");
            s.Flow(s.Entry(), "Start", seq, "In").Flow(seq, "Out0", probe, "In").Flow(seq, "Out1", s.Probe("after"), "In");
            UnitTests.FlowScript.ScriptRunner.RegisterNodes();
            using var folder = new TempFolder();
            var path = folder.Write("script.fsml", s.ToXml());
            UnitTests.FlowScript.ProbeFlowSourceObject.Records.Clear();
            // エディターのデバッグ（localhost で待ち受ける）から、ブレークポイントを受け取る
            var events = new UnitTests.Libraries.TestEvents();
            using var host = new PPDCore.TcpDebugControllerHost(new() { [path] = [probe] });
            host.OperationWaited += file => events.Add("waited " + file);
            host.Create();

            var executor = new Executor(path, []);
            var finished = false;
            executor.Finished += (sender, e) => finished = true;
            executor.Execute();
            events.WaitFor("waited " + path);
            Assert.True(executor.IsWorking);

            Assert.Empty(UnitTests.FlowScript.ProbeFlowSourceObject.Records);

            // 止まっている間に中止する（終わりは知らせない）。中止は次のノードで効く
            executor.Kill();

            Assert.False(executor.IsWorking);
            Assert.False(finished);
            Assert.DoesNotContain(UnitTests.FlowScript.ProbeFlowSourceObject.Records, r => r.Label == "after");
        }

        [Fact]
        public void Executor_ReportsMissingScript()
        {
            using var folder = new TempFolder();
            var executor = new Executor(folder.Combine("missing.fsml"), []);
            using var finished = new System.Threading.ManualResetEventSlim();
            executor.Finished += (sender, e) => finished.Set();

            executor.Execute();

            Assert.True(finished.Wait(10000));
            Assert.True(executor.HasError);
            Assert.Contains("missing.fsml", executor.ErrorText);
        }

        [Fact]
        public void IDs_AreSeenAtOnceAndUndone()
        {
            var mark = sheet.AddMark(1, (int)ButtonType.Circle, false);
            var marked = sheet.AddMark(2, 400, 225, 0, (int)ButtonType.Circle, 7, false);
            sheet.ClearHistory();
            var layer = new Layer(sheet, ids.Clone(), true);
            var (info, markedInfo) = (layer.Marks[0], layer.Marks[1]);

            layer.ChangeMarkPropertyManager.AssignID(info);
            layer.ChangeMarkPropertyManager.UnassignID(markedInfo);

            // スクリプトには新しい ID がすぐ見え、譜面に入れると同じ ID になる
            Assert.Equal((11u, 0u), (info.ID, markedInfo.ID));
            Assert.Equal((0u, 7u), (mark.ID, marked.ID));
            layer.Execute();
            Assert.Equal((11u, 0u), (mark.ID, marked.ID));

            sheet.Undo();
            Assert.Equal((0u, 7u), (mark.ID, marked.ID));
        }
    }
}
