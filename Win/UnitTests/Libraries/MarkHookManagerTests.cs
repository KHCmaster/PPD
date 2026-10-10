using FlowScriptEngine;
using PPDCoreModel;
using PPDCoreModel.Data;
using PPDFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using UnitTests.FlowScript;
using Xunit;

namespace UnitTests.Libraries
{
    /// <summary>
    /// マークの処理をスクリプトに任せる PPDCoreModel のマネージャー（CalculatePositionManager など）と、PauseManager のテストです。
    /// </summary>
    /// <remarks>
    /// マネージャーは優先度の順に処理を呼び、最初に処理したもの（EvaluateHandled）の結果を使う。
    /// そのあとの処理は、IsEvaluateRequired が true のものだけを呼ぶ。
    /// </remarks>
    public class MarkHookManagerTests
    {
        sealed class Hook(string name, int priority, List<string> log, bool handles = false, bool required = false)
            : ICalculatePosition, IProcessAllowedButtons, IProcessEvaluate, IProcessMarkBPM, IProcessMissPress, ICreateMark, PPDCoreModel.IUpdatable
        {
            public int Priority => priority;
            public bool IsEvaluateRequired() => required;
            public bool EvaluateHandled { get; private set; }

            void Run(string what)
            {
                log.Add($"{name} {what}");
                EvaluateHandled = handles;
            }

            public void Calculate(IMarkInfo markInfo, float currentTime, float bpm) => Run($"calculate {currentTime} {bpm}");
            public float BPM => 0;
            public Vector2 CalculatePosition => new(priority, priority);
            public void Process(IMarkInfo markInfo) => Run("process");
            public ButtonType[] ProcessAllowedButtons => [(ButtonType)priority];
            public void ProcessEvaluate(IMarkInfo markInfo, EffectType effectType, bool missPress, bool release, Vector2 position) => Run($"evaluate {effectType}");
            public float ProcessBPM => priority * 100;
            public void Process(IMarkInfo markInfo, MarkType pressedButton) => Run($"miss press {pressedButton}");
            public bool ProcessMissPress => priority % 2 == 0;
            public void Create(IMarkInfo markInfo) => Run("create");
            public GameComponent CreatedMark => null;
            public GameComponent CreatedColorMark => null;
            public GameComponent CreatedAxis => null;
            public GameComponent CreatedSlideMark => null;
            public GameComponent CreatedSlideColorMark => null;
            public PictureObject CreatedTrace => null;
            public void Update(float movieTime) => log.Add($"{name} update {movieTime}");
        }

        sealed class Mark : IMarkInfo
        {
            public Vector2 ColorPosition { get; set; }
            public Vector2 Position => Vector2.Zero;
            public float Angle => 0;
            public float Time => 0;
            public uint ID => 0;
            public MarkType Type => MarkType.Circle;
            public bool IsLong => false;
            public bool IsAC => false;
            public bool HasSameTimingMark => false;
            public float ReleaseTime => 0;
            public bool IsACFT => false;
            public bool IsScratch => false;
            public bool IsRight => false;
            public Dictionary<object, object> Parameters => null;
            public float SlideScale => 1;
            public int SameTimingMarks => 0;
        }

        static readonly Engine engine = new(new StringReader(""));

        // 優先度 3 は処理しない、2 は処理する、1 も処理する（あとで呼ばれる）、0 は IsEvaluateRequired
        static void AddHooks<T>(TemplateManager<T> manager, List<string> log) where T : IPriority
        {
            foreach (var hook in new[] { new Hook("c", 3, log), new Hook("b", 2, log, handles: true), new Hook("d", 4, log, handles: true, required: true), new Hook("a", 1, log, handles: true) })
            {
                manager.Add((T)(object)hook);
            }
            manager.Sort();
        }

        [Fact]
        public void CalculatePosition_FirstHandledWins()
        {
            var log = new List<string>();
            var manager = new CalculatePositionManager(engine);
            AddHooks(manager, log);
            var mark = new Mark();

            Assert.True(manager.Calculate(mark, 1.5f, 120));

            // 優先度の小さい順。a が処理したので、b と c は呼ばず、IsEvaluateRequired の d だけ呼ぶ
            Assert.Equal(["a calculate 1.5 120", "d calculate 1.5 120"], log);
            Assert.Equal(new Vector2(1, 1), mark.ColorPosition);
        }

        [Fact]
        public void OtherManagers_UseFirstHandledResult()
        {
            var log = new List<string>();
            var mark = new Mark();

            var buttons = new ProcessAllowedButtonsManager(engine);
            AddHooks(buttons, log);
            Assert.True(buttons.Process(mark, out var allowed));
            Assert.Equal([(ButtonType)1], allowed);

            var bpm = new ProcessMarkBPMManager(engine);
            AddHooks(bpm, log);
            Assert.True(bpm.Process(mark, out var processedBpm));
            Assert.Equal(100f, processedBpm);

            var missPress = new ProcessMissPressManager(engine);
            AddHooks(missPress, log);
            Assert.True(missPress.Process(mark, MarkType.Up, out var isMissPress));
            Assert.False(isMissPress);

            var evaluate = new ProcessEvaluateManager(engine);
            AddHooks(evaluate, log);
            Assert.True(evaluate.ProcessEvaluate(mark, EffectType.Cool, false, false, Vector2.Zero));

            var create = new CreateMarkManager(engine);
            AddHooks(create, log);
            Assert.True(create.CreateMark(mark, out var created, out _, out _, out _, out _, out var trace));
            Assert.Null(created);
            Assert.Null(trace);

            Assert.Equal(["a process", "d process", "a process", "d process", "a miss press Up", "d miss press Up", "a evaluate Cool", "d evaluate Cool", "a create", "d create"], log);
        }

        [Fact]
        public void NothingHandled_ReturnsFalseAndDefaults()
        {
            var log = new List<string>();
            var mark = new Mark { ColorPosition = new Vector2(9, 9) };
            var none = new Hook("x", 0, log);

            var calc = new CalculatePositionManager(engine);
            calc.Add(none);
            Assert.False(calc.Calculate(mark, 0, 0));
            Assert.Equal(new Vector2(9, 9), mark.ColorPosition);

            var buttons = new ProcessAllowedButtonsManager(engine);
            buttons.Add(none);
            Assert.False(buttons.Process(mark, out var allowed));
            Assert.Null(allowed);

            var bpm = new ProcessMarkBPMManager(engine);
            Assert.False(bpm.Process(mark, out var processedBpm));
            Assert.Equal(0f, processedBpm);

            var missPress = new ProcessMissPressManager(engine);
            missPress.Add(none);
            missPress.Clear();
            Assert.False(missPress.Process(mark, MarkType.Up, out var isMissPress));
            Assert.False(isMissPress);
            Assert.Equal(["x calculate 0 0", "x process"], log);
        }

        [Fact]
        public void UpdateManager_UpdatesAllInOrder()
        {
            var log = new List<string>();
            var manager = new UpdateManager(engine) { MovieTime = 2.5f };
            AddHooks(manager, log);

            manager.Update();

            Assert.Equal(["a update 2.5", "b update 2.5", "c update 2.5", "d update 2.5"], log);
        }

        [Fact]
        public void ScriptNodes_RegisterThemselvesWithManagers()
        {
            // スクリプトの PPD.Mark.CalculatePos ノードは、読み込むと Items のマネージャーに自分を足す
            var script = new ScriptRunner();
            script.Node(typeof(FlowScriptEnginePPD.FlowSourceObjects.Mark.CalculatePosAnyFlowSourceObject), ("Handled", "True"));
            script.Node(typeof(FlowScriptEnginePPD.FlowSourceObjects.Mark.CalculatePosByIDFlowSourceObject), ("Handled", "True"), ("TargetID", "5"));
            ScriptRunner.RegisterNodes();
            _ = FlowSourceEnumerator.EnumerateFromFile(typeof(FlowScriptEnginePPD.ToolTipText).Assembly.Location, [typeof(FlowSourceObjectBase)]).ToList();
            var scriptEngine = new Engine(new StringReader(""));
            var calc = new CalculatePositionManager(scriptEngine);
            var flowManager = scriptEngine.Load(new MemoryStream(Encoding.UTF8.GetBytes(script.ToXml())), false);
            flowManager.Items["CalculatePositionManager"] = calc;
            flowManager.Initialize();
            calc.Sort();

            Assert.True(calc.Calculate(new Mark(), 0, 0));
        }

        [Fact]
        public void PauseManager_CallsScriptOnChange()
        {
            var script = new ScriptRunner();
            ScriptRunner.RegisterNodes();
            var scriptEngine = new Engine(new StringReader(""));
            var flowManager = scriptEngine.Load(new MemoryStream(Encoding.UTF8.GetBytes(script.ToXml())), false);
            flowManager.Initialize();
            var calls = new List<string>();
            flowManager.RegisterCallBack(PauseManager.PausedCallbackName, _ => calls.Add("paused"));
            flowManager.RegisterCallBack(PauseManager.ResumedCallbackName, _ => calls.Add("resumed"));
            var pause = new PauseManager(scriptEngine);

            pause.Pause();
            pause.Pause();
            Assert.True(pause.IsPaused);
            pause.Toggle();
            Assert.False(pause.IsPaused);
            pause.Resume();
            pause.Toggle();
            pause.Initialize();

            // 同じ状態にするときは呼ばない
            Assert.Equal(["paused", "resumed", "paused"], calls);
            Assert.False(pause.IsPaused);
        }
    }
}
