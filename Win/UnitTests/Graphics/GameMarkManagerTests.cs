using FlowScriptEngine;
using PPDCore;
using PPDCoreModel;
using PPDCoreModel.Data;
using PPDFramework;
using PPDFramework.PPDStructure;
using PPDFramework.PPDStructure.EVDData;
using PPDFramework.PPDStructure.PPDData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Xunit;

namespace UnitTests.Graphics
{
    /// <summary>
    /// ゲームのマークの判定（MarkManager と、マーク、長押しのマーク）のテストです。
    /// </summary>
    /// <remarks>
    /// マークの画像はテストの赤い画像。エフェクトのファイルはないので、エフェクトは出さない。
    /// スクリプトの処理（FlowScriptManager）は、スクリプトのない空のものを使う。
    /// </remarks>
    [Collection(GraphicsCollection.Name)]
    public sealed class GameMarkManagerTests : IDisposable
    {
        const float Bpm = 130;
        readonly GraphicsDevice graphics;
        readonly TempFolder folder = new();
        readonly PPDFramework.Resource.ResourceManager resources = new();
        readonly TestImagePaths paths;

        public GameMarkManagerTests(GraphicsDevice graphics)
        {
            this.graphics = graphics;
            var image = folder.Combine("mark.png");
            using (var bitmap = new System.Drawing.Bitmap(8, 8))
            {
                using (var g = System.Drawing.Graphics.FromImage(bitmap))
                {
                    g.Clear(System.Drawing.Color.Red);
                }
                bitmap.Save(image, System.Drawing.Imaging.ImageFormat.Png);
            }
            paths = new TestImagePaths(PathObject.Absolute(image), PathObject.Absolute(folder.Combine("none.etd")));
            // 同時押しの線の画像（ゲームでは起動時に読んでおく）
            resources.Add(PPDCore.Utility.Path.Combine("connectpic.png"),
                PPDFramework.Resource.ImageResourceFactoryManager.Factory.Create(graphics.Device, PathObject.Absolute(image), false));
        }

        public void Dispose()
        {
            resources.Dispose();
            folder.Dispose();
        }

        sealed class TestImagePaths(PathObject image, PathObject missing) : MarkImagePathsBase
        {
            public override PathObject GetMarkColorImagePath(ButtonType buttontype) => image;
            public override PathObject GetMarkImagePath(ButtonType buttontype) => image;
            public override PathObject GetTraceImagePath(ButtonType buttontype) => image;
            public override void GetLongNoteCircleInfo(out PathObject imagepath, out float innerradius, out float outerradius)
            {
                imagepath = image;
                innerradius = 20;
                outerradius = 30;
            }
            public override PathObject GetCircleAxisImagePath() => image;
            public override PathObject GetClockAxisImagePath() => image;
            public override PathObject GetEvaluateEffectPath(MarkEvaluateType evatype) => missing;
            public override PathObject GetAppearEffectPath() => missing;
            public override PathObject GetSlideEffectPath() => missing;
            public override void GetHoldInfo(out PathObject imagepath, out float x, out float y)
            {
                imagepath = image;
                x = 0;
                y = 0;
            }
        }

        /// <summary>
        /// スクリプトのない FlowScriptManager（コンストラクターは Mod とスクリプトを読むので使わない）。
        /// </summary>
        static FlowScriptManager EmptyScript()
        {
            var script = (FlowScriptManager)RuntimeHelpers.GetUninitializedObject(typeof(FlowScriptManager));
            var engine = new Engine();
            void Set(string name, object value) => typeof(FlowScriptManager).GetProperty(name).SetValue(script, value);
            Set(nameof(FlowScriptManager.BPMManager), new BPMManager());
            Set(nameof(FlowScriptManager.CalculatePositionManager), new CalculatePositionManager(engine));
            Set(nameof(FlowScriptManager.ProcessEvaluateManager), new ProcessEvaluateManager(engine));
            Set(nameof(FlowScriptManager.CreateMarkManager), new CreateMarkManager(engine));
            Set(nameof(FlowScriptManager.ProcessMarkBPMManager), new ProcessMarkBPMManager(engine));
            Set(nameof(FlowScriptManager.ProcessMissPressManager), new ProcessMissPressManager(engine));
            Set(nameof(FlowScriptManager.ProcessAllowedButtonsManager), new ProcessAllowedButtonsManager(engine));
            return script;
        }

        /// <summary>
        /// 譜面を流し、ボタンを押して、判定を集めます。
        /// </summary>
        sealed class Play
        {
            public MarkManager Marks { get; init; }
            public List<(EffectType Effect, bool MissPress)> Evaluated { get; } = [];
            public List<bool> Combo { get; } = [];
            public List<(int Index, bool Keep)> Sounds { get; } = [];
            public List<(ButtonType Button, bool Pressing)> Pressing { get; } = [];
            public List<int> Slides { get; } = [];
            public int MaxSlides { get; set; }

            public void Update(float time, params ButtonType[] pressed) => Update(time, pressed, []);

            public void Update(float time, ButtonType[] pressed, ButtonType[] released)
            {
                var b = new bool[10];
                var r = new bool[10];
                foreach (var button in pressed)
                {
                    b[(int)button] = true;
                }
                foreach (var button in released)
                {
                    r[(int)button] = true;
                }
                Marks.Update(time, b, r);
            }

            /// <summary>
            /// 押さずに時間を進めます。
            /// </summary>
            public void Run(float from, float to, float step = 1 / 60f)
            {
                for (var time = from; time <= to; time += step)
                {
                    Update(time);
                }
            }
        }

        Play Start(MarkData[] marks, IEVDData[] events = null, AutoMode auto = AutoMode.None, MainGameConfigBase config = null, bool connect = false, bool random = false)
        {
            // 判定は設定（PPD.ini）の判定の幅とずれを使う。テストの設定が既定であることを確かめておく
            Assert.Equal((0f, 0.04f), (PPDSetting.Setting.AdjustGapTime, PPDSetting.Setting.CoolArea));
            var song = new SongInformation();
            typeof(SongInformation).GetProperty(nameof(SongInformation.BPM)).SetValue(song, Bpm);
            var utility = new PPDGameUtility { SongInformation = song, Profile = new Profile(), SpeedScale = 1, AutoMode = auto, Connect = connect, Random = random };
            var script = EmptyScript();
            var evd = new MemoryStream();
            EVDWriter.Write(evd, events ?? []);
            evd.Position = 0;
            config ??= MainGameConfigBase.Default;
            var eventManager = new EventManager(utility, evd, config, script);
            var ppd = new MemoryStream();
            PPDWriter.Write(ppd, marks);
            ppd.Position = 0;
            var manager = new MarkManager(graphics.Device, resources, utility, new RandomChangeManager(), new PPDEffectManager(graphics.Device, resources),
                paths, config, script, ppd)
            {
                EventManager = eventManager,
            };
            var play = new Play { Marks = manager };
            manager.EvaluateCount += (effect, miss) => play.Evaluated.Add((effect, miss));
            manager.ChangeCombo += (gain, position) => play.Combo.Add(gain);
            manager.PlaySound += (index, keep) => play.Sounds.Add((index, keep));
            manager.PressingButton += (button, pressing) =>
            {
                play.Pressing.Add((button, pressing));
                return false;
            };
            manager.Slide += (sender, position, score, right) => play.Slides.Add(score);
            manager.MaxSlide += (sender, position, score, right) => play.MaxSlides++;
            return play;
        }

        static MarkData Mark(float time, ButtonType button = ButtonType.Circle) => new(400, 225, 0, time, button, 0);

        [Theory]
        [InlineData(0, EffectType.Cool)]
        [InlineData(0.06f, EffectType.Fine)]
        [InlineData(-0.1f, EffectType.Safe)]
        [InlineData(0.14f, EffectType.Sad)]
        public void PressNearTheMark_IsJudgedByTiming(float offset, EffectType expected)
        {
            var play = Start([Mark(2)]);
            Assert.Equal(1, play.Marks.AllMarkCount);

            play.Run(0, 1.5f);
            play.Update(2 + offset, ButtonType.Circle);

            Assert.Equal([(expected, false)], play.Evaluated);
            // クールとファインはコンボがつながる
            Assert.Equal([expected is EffectType.Cool or EffectType.Fine], play.Combo);
        }

        [Fact]
        public void NotPressing_IsWorst()
        {
            var play = Start([Mark(1)]);

            play.Run(0, 1.5f);

            Assert.Equal([(EffectType.Worst, false)], play.Evaluated);
            Assert.Equal([false], play.Combo);
        }

        static readonly IEVDData[] AC = [new ChangeNoteTypeEvent(0, NoteType.AC)];
        static readonly IEVDData[] ACFT = [new ChangeNoteTypeEvent(0, NoteType.ACFT)];

        static ExMarkData Hold(float time, float endTime, ButtonType button = ButtonType.Circle) => new(400, 225, 0, time, endTime, button, 0);

        // 押して、離すまで押し続ける
        static void PressAndRelease(Play play, ButtonType button, float press, float release)
        {
            play.Run(0, press - 0.05f);
            play.Update(press, button);
            play.Run(press + 0.01f, release - 0.05f);
            play.Update(release, [], [button]);
            play.Run(release + 0.01f, release + 1);
        }

        [Fact]
        public void NormalHold_JudgesPressAndRelease()
        {
            var play = Start([Hold(1, 2)]);
            Assert.Equal(1, play.Marks.AllLongMarkCount);

            PressAndRelease(play, ButtonType.Circle, 1, 2);

            Assert.Equal([(EffectType.Cool, false), (EffectType.Cool, false)], play.Evaluated);
            Assert.Equal([true, true], play.Combo);
        }

        [Fact]
        public void NormalHold_ReleasedEarlyOrNever_IsWorst()
        {
            var early = Start([Hold(1, 2)]);
            PressAndRelease(early, ButtonType.Circle, 1, 1.5f);
            Assert.Equal([(EffectType.Cool, false), (EffectType.Worst, false)], early.Evaluated);

            var never = Start([Hold(1, 2)]);
            never.Run(0, 0.9f);
            never.Update(1, ButtonType.Circle);
            never.Run(1.01f, 3);
            Assert.Equal([(EffectType.Cool, false), (EffectType.Worst, false)], never.Evaluated);

            var notPressed = Start([Hold(1, 2)]);
            notPressed.Run(0, 3);
            Assert.Equal([(EffectType.Worst, false)], notPressed.Evaluated);
        }

        [Fact]
        public void ACHold_KeepsPressingUntilReleased()
        {
            var play = Start([Hold(1, 2)], AC);

            PressAndRelease(play, ButtonType.Circle, 1, 2);

            // AC の長押しは押したときだけ判定し、押している間は押している状態を知らせる
            Assert.Equal([(EffectType.Cool, false)], play.Evaluated);
            Assert.All(play.Pressing.SkipLast(1), p => Assert.Equal((ButtonType.Circle, true), p));
            Assert.Equal((ButtonType.Circle, false), play.Pressing[^1]);
        }

        [Fact]
        public void Slide_ScoresEachFrameAndMax()
        {
            var play = Start([Hold(1, 1.5f, ButtonType.R)], ACFT);
            play.Run(0, 0.9f);

            for (var time = 1f; time < 1.6f; time += 1 / 60f)
            {
                play.Update(time, ButtonType.R);
            }
            play.Run(1.6f, 2.5f);

            Assert.Equal([(EffectType.Cool, false)], play.Evaluated);
            // 0.5 秒に 12 個のスライド（最後は最大スライドのボーナス）
            Assert.Equal([10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110], play.Slides);
            Assert.Equal(1, play.MaxSlides);
            Assert.All(play.Sounds, s => Assert.Equal(((int)ButtonType.R, false), s));

            var missed = Start([Hold(1, 1.5f, ButtonType.R)], ACFT);
            missed.Run(0, 3);
            Assert.Equal([(EffectType.Worst, false)], missed.Evaluated);
            Assert.Empty(missed.Slides);
        }

        [Fact]
        public void Auto_PlaysHoldsAndSlides()
        {
            var hold = Start([Hold(1, 2)], auto: AutoMode.All);
            hold.Run(0, 3);
            Assert.Equal([(EffectType.Cool, false), (EffectType.Cool, false)], hold.Evaluated);
            Assert.Equal([((int)ButtonType.Circle, true)], hold.Sounds);

            var slide = Start([Hold(1, 1.5f, ButtonType.R)], ACFT, auto: AutoMode.All);
            slide.Run(0, 3);
            Assert.Equal([(EffectType.Cool, false)], slide.Evaluated);
            Assert.Equal(11, slide.Slides.Count);
            Assert.Equal(1, slide.MaxSlides);
            Assert.All(slide.Sounds, s => Assert.Equal(((int)ButtonType.R, true), s));

            // スライド以外だけ自動
            var exceptSlide = Start([Mark(1), Hold(1.5f, 2, ButtonType.R)], ACFT, auto: AutoMode.ExceptSlide);
            exceptSlide.Run(0, 3);
            Assert.Equal([(EffectType.Cool, false), (EffectType.Worst, false)], exceptSlide.Evaluated);
        }

        [Fact]
        public void SameTimingMarks_AreJudgedTogether()
        {
            var play = Start([Mark(1), Mark(1, ButtonType.Cross)], connect: true);
            play.Run(0, 0.9f);
            // 同時押しの線も描く
            Assert.Contains(graphics.Render(play.Marks), p => GraphicsDevice.Rgb(p) == (255, 0, 0));

            play.Update(1, ButtonType.Circle, ButtonType.Cross);

            Assert.Equal([(EffectType.Cool, false), (EffectType.Cool, false)], play.Evaluated);
        }

        [Fact]
        public void AC_PressingTheWrongButton_IsMissPress()
        {
            var play = Start([Mark(1)], AC);
            play.Run(0, 0.9f);

            play.Update(1, ButtonType.Cross);

            // 押した時の判定に、押し間違いの印を付けて知らせる（コンボは切れる）
            Assert.Equal([(EffectType.Cool, true)], play.Evaluated);
            Assert.Equal([false], play.Combo);

            // AC でなければ、違うボタンは無視する
            var normal = Start([Mark(1)]);
            normal.Run(0, 0.9f);
            normal.Update(1, ButtonType.Cross);
            normal.Update(1.01f, ButtonType.Circle);
            Assert.Equal([(EffectType.Cool, false)], normal.Evaluated);
        }

        [Fact]
        public void SeekRetryAndScriptMarks()
        {
            var play = Start([Mark(1), Mark(2), Mark(3)]);
            Assert.Equal(3, play.Marks.AllMarkCount);
            Assert.Equal(3, play.Marks.Marks.Length);

            // 途中から始めると、前のマークは出ない
            play.Marks.Seek(1.5f);
            play.Run(1.5f, 2.5f);
            Assert.Equal([(EffectType.Worst, false)], play.Evaluated);

            // やり直すと最初から
            play.Marks.Retry();
            play.Evaluated.Clear();
            play.Run(0, 1.5f);
            Assert.Equal([(EffectType.Worst, false)], play.Evaluated);

            // スクリプトから足したマーク
            play.Marks.Retry();
            play.Evaluated.Clear();
            play.Marks.AddMark(new Vector2(100, 100), 0, 0.5f, MarkType.Cross, 1);
            play.Marks.AddLongMark(new Vector2(100, 100), 0, 0.7f, 0.8f, MarkType.Square, 2);
            play.Run(0, 0.9f);
            // 0.9 秒までに判定するのは、足した 2 つ
            Assert.Equal(2, play.Evaluated.Count);
            Assert.All(play.Evaluated, e => Assert.Equal(EffectType.Worst, e.Effect));
            Assert.Equal(0, play.Marks.GetMarkCount(-1));
        }

        [Fact]
        public void ThreeSameTimingMarks_AreConnectedAndCounted()
        {
            // 3 つ以上の同時押しは、外側のマークを線でつなぐ（中のマークは中から）
            var spread = Start([new(100, 100, 0, 1, ButtonType.Circle, 0), new(300, 100, 0, 1, ButtonType.Cross, 0),
                new(200, 300, 0, 1, ButtonType.Square, 0), new(200, 180, 0, 1, ButtonType.Triangle, 0), Mark(2)], connect: true);
            Assert.Equal(1, spread.Marks.GetMarkCount(0));
            Assert.Equal(1, spread.Marks.GetMarkCount(3));
            Assert.Equal(0, spread.Marks.GetMarkCount(10));
            spread.Run(0, 0.9f);
            Assert.Contains(graphics.Render(spread.Marks), p => GraphicsDevice.Rgb(p) == (255, 0, 0));
            spread.Update(1, ButtonType.Circle, ButtonType.Cross, ButtonType.Square, ButtonType.Triangle);
            Assert.Equal(4, spread.Evaluated.Count(e => e.Effect == EffectType.Cool));

            // 全部同じ場所なら、色付きのマークの位置でつなぐ
            var stacked = Start([Mark(1), Mark(1, ButtonType.Cross), Mark(1, ButtonType.Square)], connect: true);
            Assert.Equal(1, stacked.Marks.GetMarkCount(2));
            stacked.Run(0, 2);
            Assert.Equal(3, stacked.Evaluated.Count(e => e.Effect == EffectType.Worst));
        }

        [Fact]
        public void TriangleWithR_IsSlideInACFT()
        {
            // R か L と同時の△は、ACFT ではスライドになる
            var play = Start([Hold(1, 1.5f, ButtonType.Triangle), Hold(1, 1.5f, ButtonType.R)], ACFT, auto: AutoMode.All);

            play.Run(0, 3);

            Assert.Equal(2, play.MaxSlides);
            // R か L がなければ AC の長押し
            var hold = Start([Hold(1, 1.5f, ButtonType.Triangle)], ACFT, auto: AutoMode.All);
            hold.Run(0, 3);
            Assert.Equal(0, hold.MaxSlides);
            Assert.Equal([(EffectType.Cool, false)], hold.Evaluated);
        }

        sealed class AutoConfig : MainGameConfigBase
        {
            public override bool Auto => true;
        }

        [Fact]
        public void ConfigAuto_PlaysEveryMark()
        {
            var play = Start([Mark(1)], config: new AutoConfig());

            play.Run(0, 2);

            Assert.Equal([(EffectType.Cool, false)], play.Evaluated);
            Assert.NotNull(play.Marks.ScoreDifficultyMeasureResult);
        }

        [Fact]
        public void ChartFromFolderOrBrokenStream()
        {
            var song = new SongInformation();
            typeof(SongInformation).GetProperty(nameof(SongInformation.DirectoryPath)).SetValue(song, folder.Path);
            typeof(SongInformation).GetProperty(nameof(SongInformation.BPM)).SetValue(song, Bpm);
            PPDWriter.Write(folder.Combine("Normal.ppd"), [Mark(1), Mark(2)]);
            MarkManager Create(PPDFrameworkCore.Difficulty difficulty, Stream stream = null)
            {
                var utility = new PPDGameUtility { SongInformation = song, Profile = new Profile(), SpeedScale = 1, Difficulty = difficulty };
                var script = EmptyScript();
                var effects = new PPDEffectManager(graphics.Device, resources);
                var manager = stream == null
                    ? new MarkManager(graphics.Device, resources, utility, new RandomChangeManager(), effects, paths, MainGameConfigBase.Default, script)
                    : new MarkManager(graphics.Device, resources, utility, new RandomChangeManager(), effects, paths, MainGameConfigBase.Default, script, stream);
                manager.EventManager = new EventManager(utility, new MemoryStream(), MainGameConfigBase.Default, script);
                return manager;
            }

            // 古い形式の譜面は、難易度の名前の .ppd を読む
            var normal = Create(PPDFrameworkCore.Difficulty.Normal);
            Assert.Equal(2, normal.AllMarkCount);
            var count = 0;
            normal.EvaluateCount += (effect, miss) => count++;
            for (var time = 0f; time < 3; time += 1 / 60f)
            {
                normal.Update(time, new bool[10], new bool[10]);
            }
            Assert.Equal(2, count);

            // ファイルがない、読めない譜面は何もしない
            foreach (var manager in new[] { Create(PPDFrameworkCore.Difficulty.Extreme), Create(PPDFrameworkCore.Difficulty.Normal, new MemoryStream([1, 2, 3])) })
            {
                manager.Update(1, new bool[10], new bool[10]);
                manager.Seek(1);
                Assert.Equal(0, manager.MarkLayer.ChildrenCount);
                Assert.DoesNotContain(graphics.Render(manager), p => GraphicsDevice.Rgb(p) == (255, 0, 0));
            }
        }

        [Fact]
        public void Random_ChangesButtonsButStillJudges()
        {
            var play = Start([Mark(1)], auto: AutoMode.All, random: true);

            play.Run(0, 2);

            Assert.Equal([(EffectType.Cool, false)], play.Evaluated);
        }

        [Fact]
        public void Auto_PlaysEveryMark()
        {
            var play = Start([Mark(1), Mark(1.5f, ButtonType.Cross)], auto: AutoMode.All);

            play.Run(0, 2);

            Assert.Equal([(EffectType.Cool, false), (EffectType.Cool, false)], play.Evaluated);
            Assert.Equal([((int)ButtonType.Circle, false), ((int)ButtonType.Cross, false)], play.Sounds);
        }
    }
}
