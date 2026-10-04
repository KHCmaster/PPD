using BezierCaliculator;
using Effect2D;
using Effect2DEditor;
using Effect2DEditor.Models;
using Effect2DEditor.ViewModels;
using Effect2DEditor.ViewModels.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;
using Color = System.Drawing.Color;
using PointF = System.Drawing.PointF;

namespace UnitTests.Effect2DEditor
{
    /// <summary>
    /// テストで使う画像とフォルダーを作ります。
    /// </summary>
    static class EditorFiles
    {
        /// <summary>
        /// 言語ファイルを一時フォルダーにコピーして、言語を日本語に戻します（言語の文字は全部の画面で共有している）。
        /// </summary>
        public static void Prepare(TempFolder folder)
        {
            AppFiles.CopyTo("Effect2DEditor", folder);
            LanguageTexts.Instance.Load(Path.Combine("Lang", "lang_Effect2DEditor_jp.ini"));
        }

        public static string WritePng(TempFolder folder, string name)
        {
            var bitmap = BitmapSource.Create(2, 2, 96, 96, PixelFormats.Bgra32, null, new byte[16], 8);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            var path = folder.Combine(name);
            using var stream = File.Create(path);
            encoder.Save(stream);
            return path;
        }
    }

    public class EditorSettingsTests
    {
        [Fact]
        public void Load_MissingFiles_UsesDefaults()
        {
            using var folder = new TempFolder(setCurrentDirectory: true);

            var settings = EditorSettings.Load();

            Assert.Equal("jp", settings.Language);
            Assert.Equal((512, 512), (settings.CanvasWidth, settings.CanvasHeight));
            Assert.Null(settings.WindowWidth);
            Assert.Equal(BackGroundMode.Default, settings.BackGroundMode);
            Assert.Equal(Color.White.ToArgb(), settings.BackGroundColor.ToArgb());
        }

        [Fact]
        public void Load_ReadsValuesAndExistingRecentFiles()
        {
            using var folder = new TempFolder(setCurrentDirectory: true);
            folder.Write("a.etd", "");
            File.WriteAllText("Effect2DEditor.ini", "[Language]en\n[WindowWidth]800\n[WindowHeight]600\n[WindowState]2\n[CanvasWidth]0\n[CanvasHeight]300\n"
                + "[CanvasBackGround]1\n[CanvasBackGroundColor]#102030\n[CanvasBackGroundImage]bg.png\n");
            File.WriteAllLines("recentused.ini", [folder.Combine("a.etd"), folder.Combine("deleted.etd")]);

            var settings = EditorSettings.Load();

            Assert.Equal("en", settings.Language);
            Assert.Equal((800.0, 600.0), (settings.WindowWidth, settings.WindowHeight));
            Assert.True(settings.Maximized);
            // 0 以下の大きさは 1 にする
            Assert.Equal((1, 300), (settings.CanvasWidth, settings.CanvasHeight));
            Assert.Equal(BackGroundMode.Color, settings.BackGroundMode);
            Assert.Equal(Color.FromArgb(0x10, 0x20, 0x30).ToArgb(), settings.BackGroundColor.ToArgb());
            Assert.Equal("bg.png", settings.BackGroundImage);
            Assert.Equal([folder.Combine("a.etd")], settings.RecentFiles);
        }

        [Fact]
        public void Load_MinimizedAndBadColor_AreIgnored()
        {
            using var folder = new TempFolder(setCurrentDirectory: true);
            File.WriteAllText("Effect2DEditor.ini", "[WindowState]1\n[CanvasBackGround]2\n[CanvasBackGroundColor]not a color\n");

            var settings = EditorSettings.Load();

            Assert.False(settings.Maximized);
            Assert.Equal(BackGroundMode.Image, settings.BackGroundMode);
            Assert.Equal(Color.White.ToArgb(), settings.BackGroundColor.ToArgb());
        }

        [Fact]
        public void Save_ThenLoad_KeepsValuesAndOtherSettings()
        {
            using var folder = new TempFolder(setCurrentDirectory: true);
            folder.Write("a.etd", "");
            File.WriteAllText("Effect2DEditor.ini", "[Other]keep\n");
            var settings = new EditorSettings
            {
                Language = "kr",
                WindowWidth = 1000.6,
                WindowHeight = 700,
                CanvasWidth = 640,
                CanvasHeight = 480,
                BackGroundMode = BackGroundMode.Color,
                BackGroundColor = Color.FromArgb(1, 2, 3),
                BackGroundImage = "x.png",
            };
            settings.RecentFiles.Add(folder.Combine("a.etd"));

            settings.Save();
            var loaded = EditorSettings.Load();

            Assert.Equal("kr", loaded.Language);
            Assert.Equal((1000.0, 700.0), (loaded.WindowWidth, loaded.WindowHeight));
            Assert.False(loaded.Maximized);
            Assert.Equal((640, 480), (loaded.CanvasWidth, loaded.CanvasHeight));
            Assert.Equal(Color.FromArgb(1, 2, 3).ToArgb(), loaded.BackGroundColor.ToArgb());
            Assert.Equal([folder.Combine("a.etd")], loaded.RecentFiles);
            Assert.Contains("[Other]keep", File.ReadAllText("Effect2DEditor.ini"));
        }
    }

    public class EffectModelTests
    {
        private static EffectManager CreateEffects(int count)
        {
            var manager = new EffectManager();
            for (int i = 0; i < count; i++)
            {
                var effect = new BaseEffect { Filename = $"{i}.png" };
                effect.SetDefault();
                manager.Effects.Add(effect);
            }
            return manager;
        }

        [Theory]
        [InlineData("a.png", true)]
        [InlineData("a.JPG", true)]
        [InlineData("a.jpeg", true)]
        [InlineData("a.gif", true)]
        [InlineData("a.etd", true)]
        [InlineData("a.bmp", false)]
        [InlineData("a", false)]
        public void CanAdd(string path, bool expected)
        {
            Assert.Equal(expected, EffectFile.CanAdd(path));
        }

        [Fact]
        public void AnimationBuilder_ShowsImagesInTurn()
        {
            var manager = CreateEffects(3);

            AnimationBuilder.Build(manager, 10, false);

            // 最後の画像から順に、10 フレームずつ表示する
            Assert.Equal([0, 10], manager.Effects[2].Sets.Keys);
            Assert.Equal([0, 10, 20], manager.Effects[1].Sets.Keys);
            Assert.Equal([0, 20], manager.Effects[0].Sets.Keys);
            Assert.Equal(0, manager.Effects[1].Sets[0].StartState.Alpha);
            Assert.Equal(1, manager.Effects[1].Sets[10].StartState.Alpha);
            Assert.Equal(30, manager.Effects[1].Sets[20].EndFrame);
        }

        [Fact]
        public void AnimationBuilder_WithReverse_GoesBackAndForth()
        {
            var manager = CreateEffects(3);

            AnimationBuilder.Build(manager, 10, true);

            Assert.Equal([0, 10, 40], manager.Effects[2].Sets.Keys);
            Assert.Equal([0, 10, 20, 30, 40], manager.Effects[1].Sets.Keys);
            Assert.Equal([0, 20, 30], manager.Effects[0].Sets.Keys);
            Assert.All(manager.Effects, e => Assert.Equal(50, e.Sets.Values.Last().EndFrame));
        }

        [Fact]
        public void SaveThenLoad_CopiesImagesNextToFile()
        {
            using var folder = new TempFolder();
            var png = EditorFiles.WritePng(folder, "image.png");
            var manager = new EffectManager();
            var effect = new BaseEffect { Filename = png };
            effect.SetDefault();
            manager.Effects.Add(effect);
            var path = folder.Combine("out", "effect.etd");
            Directory.CreateDirectory(folder.Combine("out"));

            EffectFile.Save(manager, path);
            var errors = 0;
            var loaded = EffectFile.Load(path, () => errors++);

            Assert.True(File.Exists(folder.Combine("out", "effect", "image.png")));
            Assert.Single(loaded.Effects);
            Assert.Equal("image.png", Path.GetFileName(((BaseEffect)loaded.Effects[0]).Filename));
            Assert.Equal(0, errors);
        }

        [Fact]
        public void Load_BrokenImage_CallsErrorHandler()
        {
            using var folder = new TempFolder();
            var png = EditorFiles.WritePng(folder, "image.png");
            var manager = new EffectManager();
            var effect = new BaseEffect { Filename = png };
            effect.SetDefault();
            manager.Effects.Add(effect);
            var path = folder.Combine("effect.etd");
            EffectFile.Save(manager, path);
            File.WriteAllText(folder.Combine("effect", "image.png"), "not an image");

            var errors = 0;
            EffectFile.Load(path, () => errors++);

            Assert.Equal(1, errors);
        }
    }

    public class Effect2DEditorDialogTests
    {
        sealed class FakePicker : IPickerView
        {
            public Color? Color { get; set; }
            public string File { get; set; }

            public Color? PickColor(Color current) => Color;

            public string OpenFile() => File;
        }

        sealed class FakeRatioEditor : IRatioEditor
        {
            public BezierControlPoint[] Bcps { get; set; } = [new BezierControlPoint { Second = new PointF(1, 2) }, new BezierControlPoint { Second = new PointF(3, 4) }];
            public bool IsLinear { get; private set; }

            public void SetLinear() => IsLinear = true;
        }

        [Theory]
        [InlineData("5", 5)]
        [InlineData("0", 1)]
        [InlineData("abc", 1)]
        public void AskFrameCount_ParsesCount(string text, int expected)
        {
            Assert.Equal(expected, new AskFrameCountViewModel { FrameCountText = text }.FrameCount);
        }

        [Fact]
        public void CanvasSize_ParsesOrUsesDefaults()
        {
            var viewModel = new CanvasSizeViewModel(100, 200);
            Assert.Equal(("100", "200"), (viewModel.WidthText, viewModel.HeightText));

            viewModel.WidthText = "x";
            viewModel.HeightText = "-5";

            Assert.Equal((256, 1), (viewModel.CanvasWidth, viewModel.CanvasHeight));
        }

        [Fact]
        public void Ok_RaisesCloseRequested()
        {
            var viewModel = new AskFrameCountViewModel();
            var closed = false;
            viewModel.CloseRequested += () => closed = true;

            viewModel.OkCommand.Execute(null);

            Assert.True(closed);
        }

        [Fact]
        public void ChangeBackGround_PickSetsMode()
        {
            var picker = new FakePicker();
            var viewModel = new ChangeBackGroundViewModel(BackGroundMode.Default, Color.White, "") { View = picker };

            viewModel.PickColorCommand.Execute(null);
            viewModel.PickImageCommand.Execute(null);
            Assert.Equal(BackGroundMode.Default, viewModel.Mode);

            picker.Color = Color.Red;
            viewModel.PickColorCommand.Execute(null);
            Assert.Equal((BackGroundMode.Color, Color.Red), (viewModel.Mode, viewModel.Color));

            picker.File = "bg.png";
            viewModel.PickImageCommand.Execute(null);
            Assert.Equal((BackGroundMode.Image, "bg.png"), (viewModel.Mode, viewModel.FileName));
        }

        [Fact]
        public void RatioChange_CopyPasteAndOk()
        {
            var first = new FakeRatioEditor();
            var viewModel = new RatioChangeViewModel(null) { Editor = first };
            viewModel.CopyCommand.Execute(null);
            var second = new FakeRatioEditor { Bcps = [new BezierControlPoint(), new BezierControlPoint()] };
            var other = new RatioChangeViewModel(null) { Editor = second };

            other.PasteCommand.Execute(null);
            other.SetLinearCommand.Execute(null);
            other.OkCommand.Execute(null);

            Assert.Equal(new PointF(3, 4), other.Bcps[1].Second);
            Assert.NotSame(first.Bcps[1], other.Bcps[1]);
            Assert.True(other.IsLinear);
        }
    }

    /// <summary>
    /// Effect2DEditor のメインの ViewModel のテストです。カンバスとタイムラインは偽物にする。
    /// </summary>
    public class Effect2DEditorViewModelTests : IDisposable
    {
        sealed class FakeView : IMainWindowView
        {
            public List<string> Messages { get; } = [];
            public bool? AskSaveAnswer { get; set; } = false;
            public int AskSaveCount { get; private set; }
            public bool ConfirmAnswer { get; set; } = true;
            public Queue<string[]> OpenResults { get; } = new();
            public string SavePath { get; set; }
            public Func<DialogViewModel, bool> Dialog { get; set; } = _ => false;
            public List<DockKind> ToggledDocks { get; } = [];
            public (int Width, int Height)? CanvasSize { get; private set; }
            public int CanvasRefreshes { get; private set; }
            public int AttachCount { get; private set; }
            public BezierControlPoint[] CanvasBezier { get; set; } = [];
            public bool CanvasBezierEditable { get; private set; }
            public bool Closed { get; private set; }

            public int CurrentFrame => 0;

            public void ShowMessage(string text) => Messages.Add(text);

            public bool Confirm(string text, string caption) => ConfirmAnswer;

            public bool? AskSave(string text, string caption)
            {
                AskSaveCount++;
                return AskSaveAnswer;
            }

            public string[] OpenFiles(string filter, bool multiselect, string fileName = null) => OpenResults.Count > 0 ? OpenResults.Dequeue() : [];

            public string SaveFile(string filter, string defaultExt, string fileName) => SavePath;

            public bool ShowDialog(DialogViewModel viewModel) => Dialog(viewModel);

            public void ToggleDock(DockKind kind) => ToggledDocks.Add(kind);

            public void InitializeCanvas(int width, int height) => CanvasSize = (width, height);

            public void RefreshCanvas() => CanvasRefreshes++;

            public void RefreshTimeLine()
            {
            }

            public void AdjustTimeLineHScroll()
            {
            }

            public void MoveTimeLineVScroll(int line)
            {
            }

            public void AttachEffect() => AttachCount++;

            public void SetCanvasBezier(BezierControlPoint[] bcps, bool allowEdit)
            {
                CanvasBezier = bcps;
                CanvasBezierEditable = allowEdit;
            }

            public void Close() => Closed = true;
        }

        readonly TempFolder folder = new(setCurrentDirectory: true);
        readonly FakeView view = new();
        readonly MainWindowViewModel viewModel;

        public Effect2DEditorViewModelTests()
        {
            EditorFiles.Prepare(folder);
            viewModel = new MainWindowViewModel(view);
            viewModel.Start();
        }

        public void Dispose()
        {
            viewModel.OnClosed();
            folder.Dispose();
        }

        private IList<IEffect> Effects => viewModel.EffectManager.Effects;

        private string AddImage(string name = "image.png")
        {
            var png = EditorFiles.WritePng(folder, name);
            viewModel.AddDroppedFiles([png]);
            return png;
        }

        // タイムラインで、エフェクトの最初のステートの組の終わりのステートを選ぶ
        private EffectStateRatioSet SelectEndState(int index = 0)
        {
            var effect = Effects[index];
            var set = effect.Sets.Values[0];
            viewModel.SelectedManager.Effect = effect;
            viewModel.SelectedManager.Set = set;
            viewModel.SelectedManager.State = set.EndState;
            return set;
        }

        [Fact]
        public void Start_LoadsLanguagesAndNewEffect()
        {
            Assert.Equal(6, viewModel.Languages.Count);
            Assert.Equal("日本語", viewModel.Languages.Single(l => l.IsChecked).Header);
            Assert.Empty(Effects);
            Assert.Equal("60", viewModel.FpsText);
            Assert.Equal(1, view.AttachCount);
            Assert.False(viewModel.UndoCommand.CanExecute(null));
            Assert.Equal("１回再生", viewModel.PlayToolTip);
        }

        [Fact]
        public void SelectLanguage_ChangesTextsAndSetting()
        {
            var english = viewModel.Languages.Single(l => l.Header == "English");

            viewModel.SelectLanguageCommand.Execute(english);

            Assert.Equal("en", viewModel.Settings.Language);
            Assert.Equal("Play once", viewModel.PlayToolTip);
            Assert.Single(viewModel.Languages, l => l.IsChecked);
            Assert.Equal("missing key", viewModel.Texts["missing key"]);
        }

        [Fact]
        public void AddFiles_AddsImagesAndUndoRedo()
        {
            var png = EditorFiles.WritePng(folder, "a.png");
            view.OpenResults.Enqueue([png, EditorFiles.WritePng(folder, "b.png")]);

            viewModel.AddFilesCommand.Execute(null);

            Assert.Equal(2, Effects.Count);
            Assert.Equal("イメージ追加:b", viewModel.CommandManager.LastCommand.Explaination);
            viewModel.UndoCommand.Execute(null);
            Assert.Single(Effects);
            Assert.True(viewModel.RedoCommand.CanExecute(null));
            viewModel.RedoCommand.Execute(null);
            Assert.Equal(2, Effects.Count);
        }

        [Fact]
        public void AddDroppedFiles_SkipsOtherFilesAndReportsBrokenImages()
        {
            folder.Write("note.txt", "");
            folder.Write("broken.png", "not an image");

            viewModel.AddDroppedFiles([folder.Combine("note.txt"), folder.Combine("broken.png")]);

            Assert.Empty(Effects);
            Assert.Equal(["Failed to load image"], view.Messages);
        }

        [Fact]
        public void EffectCommands_ChangeEffectsAndUndo()
        {
            AddImage("a.png");
            AddImage("b.png");
            viewModel.SelectedManager.Effect = Effects[0];

            viewModel.CopyEffectCommand.Execute(null);
            Assert.Equal(3, Effects.Count);

            viewModel.ReverseEffectsCommand.Execute(null);
            Assert.Equal("a.png", Path.GetFileName(((BaseEffect)Effects[2]).Filename));

            viewModel.SelectedManager.Effect = Effects[2];
            viewModel.DeleteEffectCommand.Execute(null);
            Assert.Equal(2, Effects.Count);

            viewModel.ClearEffectCommand.Execute(null);
            Assert.Empty(Effects);

            viewModel.UndoCommand.Execute(null);
            Assert.Equal(2, Effects.Count);
            viewModel.ChangeIndex(1, 0);
            Assert.Equal(2, Effects.Count);
            Assert.Empty(view.Messages);
        }

        [Fact]
        public void EffectCommands_NothingSelected_DoNothing()
        {
            viewModel.DeleteEffectCommand.Execute(null);
            viewModel.CopyEffectCommand.Execute(null);
            viewModel.ReverseEffectsCommand.Execute(null);
            viewModel.ClearEffectCommand.Execute(null);

            Assert.False(viewModel.CommandManager.CanUndo);
        }

        [Fact]
        public void NewEffect_AskSave_CancelKeepsEffects()
        {
            AddImage();
            view.AskSaveAnswer = null;

            viewModel.NewEffectCommand.Execute(null);
            Assert.Single(Effects);

            view.AskSaveAnswer = false;
            viewModel.NewEffectCommand.Execute(null);
            Assert.Empty(Effects);
            Assert.Equal(2, view.AskSaveCount);
        }

        [Fact]
        public void NewEffect_AskSave_YesButCancelSaving_KeepsEffects()
        {
            AddImage();
            view.AskSaveAnswer = true;
            view.SavePath = null;

            viewModel.NewEffectCommand.Execute(null);

            Assert.Single(Effects);
        }

        [Fact]
        public void SaveAsThenOpen_RestoresEffectsAndRecentFiles()
        {
            AddImage();
            view.SavePath = folder.Combine("saved.etd");
            viewModel.SaveAsCommand.Execute(null);
            Assert.True(File.Exists(folder.Combine("saved", "image.png")));
            // 保存したので、新しくするときに聞かない
            viewModel.NewEffectCommand.Execute(null);
            Assert.Equal(0, view.AskSaveCount);

            view.OpenResults.Enqueue([folder.Combine("saved.etd")]);
            viewModel.OpenCommand.Execute(null);

            Assert.Single(Effects);
            Assert.Equal(folder.Combine("saved.etd"), viewModel.RecentFiles[0].Path);
        }

        [Fact]
        public void Overwrite_WithoutFile_AsksNameThenOverwrites()
        {
            AddImage();
            view.SavePath = folder.Combine("first.etd");
            viewModel.OverwriteCommand.Execute(null);
            Assert.True(File.Exists(folder.Combine("first.etd")));

            AddImage("second.png");
            view.SavePath = folder.Combine("other.etd");
            viewModel.OverwriteCommand.Execute(null);

            Assert.False(File.Exists(folder.Combine("other.etd")));
            Assert.True(File.Exists(folder.Combine("first", "second.png")));
        }

        [Fact]
        public void RecentFiles_MoveToTopAndKeepFive()
        {
            AddImage();
            var paths = Enumerable.Range(0, 6).Select(i => folder.Combine($"e{i}.etd")).ToList();
            foreach (var path in paths)
            {
                view.SavePath = path;
                viewModel.SaveAsCommand.Execute(null);
            }

            foreach (var path in paths)
            {
                viewModel.OpenDroppedFile(path);
            }
            viewModel.OpenRecentFileCommand.Execute(viewModel.RecentFiles[3]);
            viewModel.OpenDroppedFile(folder.Combine("missing.etd"));

            Assert.Equal([paths[2], paths[5], paths[4], paths[3], paths[1]], viewModel.RecentFiles.Select(r => r.Path));
        }

        [Fact]
        public void OnClosing_SavesSettingsAndRecentFiles()
        {
            AddImage();
            view.SavePath = folder.Combine("saved.etd");
            viewModel.SaveAsCommand.Execute(null);
            viewModel.OpenDroppedFile(folder.Combine("saved.etd"));

            Assert.True(viewModel.OnClosing(new Rect(0, 0, 900, 700), true));

            var settings = EditorSettings.Load();
            Assert.Equal((900.0, 700.0), (settings.WindowWidth, settings.WindowHeight));
            Assert.True(settings.Maximized);
            Assert.Equal([folder.Combine("saved.etd")], settings.RecentFiles);
        }

        [Fact]
        public void OnClosing_Cancelled_DoesNotSave()
        {
            AddImage();
            view.AskSaveAnswer = null;

            Assert.False(viewModel.OnClosing(new Rect(0, 0, 900, 700), false));
            Assert.False(File.Exists("Effect2DEditor.ini"));
        }

        [Fact]
        public void ExitAndToggleDock_GoToView()
        {
            viewModel.ToggleDockCommand.Execute(DockKind.History);
            viewModel.ExitCommand.Execute(null);

            Assert.Equal([DockKind.History], view.ToggledDocks);
            Assert.True(view.Closed);
        }

        [Fact]
        public void ChangeCanvasSize_OkResizesCanvas()
        {
            view.Dialog = dialog =>
            {
                var size = Assert.IsType<CanvasSizeViewModel>(dialog);
                Assert.Equal("512", size.WidthText);
                size.WidthText = "300";
                size.HeightText = "200";
                return true;
            };

            viewModel.ChangeCanvasSizeCommand.Execute(null);

            Assert.Equal((300, 200), view.CanvasSize);
            Assert.Equal((300, 200), (viewModel.Settings.CanvasWidth, viewModel.Settings.CanvasHeight));
        }

        [Fact]
        public void ChangeCanvasSize_Cancel_KeepsSize()
        {
            viewModel.ChangeCanvasSizeCommand.Execute(null);

            Assert.Null(view.CanvasSize);
            Assert.Equal(512, viewModel.Settings.CanvasWidth);
        }

        [Fact]
        public void ChangeBackGround_SetsColorOrImage()
        {
            view.Dialog = dialog =>
            {
                var background = (ChangeBackGroundViewModel)dialog;
                background.Mode = BackGroundMode.Color;
                background.Color = Color.Blue;
                return true;
            };
            viewModel.ChangeBackGroundCommand.Execute(null);
            Assert.Equal((BackGroundMode.Color, Color.Blue), (viewModel.BackGroundMode, viewModel.BackGroundColor));

            view.Dialog = dialog =>
            {
                var background = (ChangeBackGroundViewModel)dialog;
                background.Mode = BackGroundMode.Image;
                background.FileName = "bg.png";
                return true;
            };
            viewModel.ChangeBackGroundCommand.Execute(null);
            Assert.Equal((BackGroundMode.Image, "bg.png"), (viewModel.BackGroundMode, viewModel.BackGroundImage));
            Assert.Equal(Color.Blue, viewModel.BackGroundColor);
            Assert.Equal(2, view.CanvasRefreshes);
        }

        [Fact]
        public void Play_SelectTypeStopAndFps()
        {
            AddImage();

            viewModel.SelectPlayTypeCommand.Execute("Loop");
            Assert.True(viewModel.IsPlaying);
            Assert.Equal("ループ再生", viewModel.PlayToolTip);
            Assert.Equal(EffectManager.PlayState.Playing, viewModel.EffectManager.State);

            viewModel.IsPlaying = false;
            Assert.Equal(EffectManager.PlayState.Pause, viewModel.EffectManager.State);
            viewModel.IsPlaying = true;
            Assert.Equal(EffectManager.PlayState.Playing, viewModel.EffectManager.State);

            viewModel.StopCommand.Execute(null);
            Assert.False(viewModel.IsPlaying);
            Assert.Equal(EffectManager.PlayState.Stop, viewModel.EffectManager.State);

            viewModel.FpsText = "30";
            Assert.Equal(30, viewModel.EffectManager.FPS);
            viewModel.FpsText = "abc";
            Assert.Equal(30, viewModel.EffectManager.FPS);
        }

        [Theory]
        [InlineData("ReverseOnce", "リバース１回再生")]
        [InlineData("ReverseLoop", "リバースループ再生")]
        public void PlayToolTip_FollowsPlayType(string type, string expected)
        {
            viewModel.PlayType = Enum.Parse<EffectManager.PlayType>(type);

            Assert.Equal(expected, viewModel.PlayToolTip);
        }

        [Fact]
        public void CreateAnimation_ConfirmedBuildsAndClearsHistory()
        {
            AddImage("a.png");
            AddImage("b.png");
            view.Dialog = dialog =>
            {
                ((AskFrameCountViewModel)dialog).FrameCountText = "10";
                return true;
            };

            view.ConfirmAnswer = false;
            viewModel.CreateAnimationCommand.Execute(null);
            Assert.Single(Effects[0].Sets);

            view.ConfirmAnswer = true;
            viewModel.CreateAnimationCommand.Execute(null);
            Assert.Equal([0, 10], Effects[1].Sets.Keys);
            Assert.False(viewModel.CommandManager.CanUndo);
        }

        [Fact]
        public void AddKeyStateThenDelete_UndoRestores()
        {
            AddImage();
            ITimeLineHost host = viewModel;
            var managerSets = viewModel.EffectManager.Sets.Count;

            host.AddState(0, 5, true);
            Assert.Equal(2, Effects[0].Sets.Count);
            Assert.Equal(5, Effects[0].Sets.Values[1].EndFrame);

            host.DeleteState(0, 5);
            Assert.Single(Effects[0].Sets);

            viewModel.UndoCommand.Execute(null);
            // 最後のステートを消したのを元に戻すと、エフェクトにステートの組が戻る
            Assert.Equal(2, Effects[0].Sets.Count);
            Assert.Equal(managerSets, viewModel.EffectManager.Sets.Count);
            viewModel.UndoCommand.Execute(null);
            Assert.Single(Effects[0].Sets);
            Assert.Empty(view.Messages);
        }

        [Fact]
        public void AddNormalStateInMiddle_SplitsSetAndUndo()
        {
            AddImage();
            ITimeLineHost host = viewModel;
            host.AddState(0, 10, true);

            host.AddState(0, 5, false);

            Assert.Equal([0, 1, 5], Effects[0].Sets.Keys);
            viewModel.UndoCommand.Execute(null);
            Assert.Equal([0, 1], Effects[0].Sets.Keys);
            Assert.Empty(view.Messages);
        }

        [Theory]
        // 組の途中にキーを入れると、キーの 1 フレーム前までの組、1 フレームの変化しない組、残りの組に分ける
        [InlineData(5, new[] { 0, 1, 4, 5 })]
        // 組の始めの次のフレームなら、始めの組はなくなる
        [InlineData(2, new[] { 0, 1, 2 })]
        public void AddKeyStateInMiddle_SplitsSetAndUndo(int key, int[] expected)
        {
            AddImage();
            ITimeLineHost host = viewModel;
            host.AddState(0, 10, false);

            host.AddState(0, key, true);

            Assert.Equal(expected, Effects[0].Sets.Keys);
            Assert.Equal(10, Effects[0].Sets.Values.Last().EndFrame);
            Assert.All(Effect2D.Utility.RatioTypeArray, type => Assert.IsType<ConstantRatioMaker>(Effects[0].Sets[key - 1][type]));
            viewModel.UndoCommand.Execute(null);
            Assert.Equal([0, 1], Effects[0].Sets.Keys);
            Assert.Equal(10, Effects[0].Sets[1].EndFrame);
            Assert.Empty(view.Messages);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void AddStateInMiddle_SplitsBezierPath(bool isKey)
        {
            AddImage();
            ITimeLineHost host = viewModel;
            host.AddState(0, 10, false);
            var set = Effects[0].Sets[1];
            viewModel.SelectedManager.Effect = Effects[0];
            viewModel.SelectedManager.Set = set;
            viewModel.SelectedManager.State = set.EndState;
            view.CanvasBezier = [new BezierControlPoint { Second = new PointF(0, 0) }, new BezierControlPoint { Second = new PointF(90, 0) }];
            viewModel.OnBezierEdited();
            Assert.True(set.IsBezierPosition);

            host.AddState(0, 4, isKey);

            Assert.All(Effects[0].Sets.Values.Skip(1), s => Assert.True(s.IsBezierPosition));
            Assert.True(Effects[0].Sets.Count > 2);
            Assert.Empty(view.Messages);
            viewModel.UndoCommand.Execute(null);
            Assert.Equal([0, 1], Effects[0].Sets.Keys);
        }

        [Fact]
        public void DeleteMiddleState_JoinsSetsAndUndo()
        {
            AddImage();
            ITimeLineHost host = viewModel;
            host.AddState(0, 5, true);
            host.AddState(0, 10, true);

            host.DeleteState(0, 5);
            Assert.Equal([0, 1], Effects[0].Sets.Keys);
            Assert.Equal(10, Effects[0].Sets.Values[1].EndFrame);

            viewModel.UndoCommand.Execute(null);
            Assert.Equal([0, 1, 5], Effects[0].Sets.Keys);
        }

        // 0-1、1-5、5-10 のステートの組を作り、1-5 の組を選ぶ
        private EffectStateRatioSet CreateThreeSetsAndSelectMiddle(bool startState)
        {
            AddImage();
            ITimeLineHost host = viewModel;
            host.AddState(0, 5, true);
            host.AddState(0, 10, true);
            var set = Effects[0].Sets.Values[1];
            viewModel.SelectedManager.Effect = Effects[0];
            viewModel.SelectedManager.Set = set;
            viewModel.SelectedManager.State = startState ? set.StartState : set.EndState;
            return set;
        }

        [Fact]
        public void MoveState_Move_UndoRedo()
        {
            CreateThreeSetsAndSelectMiddle(startState: false);
            // タイムラインではもう動かしてあるので、履歴に残すだけ。元に戻す・やり直しで動く
            ((ITimeLineHost)viewModel).MoveState(5, 7, TimeLine.KeyOperationMode.Move);

            viewModel.UndoCommand.Execute(null);
            Assert.Equal([0, 1, 5], Effects[0].Sets.Keys);
            viewModel.RedoCommand.Execute(null);
            Assert.Equal([0, 1, 7], Effects[0].Sets.Keys);
            Assert.Equal(7, Effects[0].Sets.Values[1].EndFrame);
            viewModel.UndoCommand.Execute(null);
            Assert.Equal([0, 1, 5], Effects[0].Sets.Keys);
        }

        [Fact]
        public void MoveState_MoveStartState_UndoRedo()
        {
            AddImage();
            ((ITimeLineHost)viewModel).AddState(0, 5, true);
            var set = Effects[0].Sets.Values[0];
            viewModel.SelectedManager.Effect = Effects[0];
            viewModel.SelectedManager.Set = set;
            viewModel.SelectedManager.State = set.StartState;
            ((ITimeLineHost)viewModel).MoveState(3, 0, TimeLine.KeyOperationMode.Move);

            viewModel.UndoCommand.Execute(null);
            // 終わりより後ろには動かせない
            Assert.Equal(0, set.StartFrame);
            Assert.Equal(1, set.EndFrame);
            viewModel.RedoCommand.Execute(null);
            Assert.Equal(0, set.StartFrame);
        }

        [Fact]
        public void MoveState_LeftMoveOnly_ShiftsFollowingSets()
        {
            CreateThreeSetsAndSelectMiddle(startState: false);
            ((ITimeLineHost)viewModel).MoveState(5, 8, TimeLine.KeyOperationMode.LeftMoveOnly);

            viewModel.UndoCommand.Execute(null);
            viewModel.RedoCommand.Execute(null);
            Assert.Equal([0, 1, 8], Effects[0].Sets.Keys);
            Assert.Equal(13, Effects[0].Sets.Values[2].EndFrame);
            viewModel.UndoCommand.Execute(null);
            Assert.Equal([0, 1, 5], Effects[0].Sets.Keys);
            Assert.Equal(10, Effects[0].Sets.Values[2].EndFrame);
        }

        [Fact]
        public void MoveState_RightMoveOnly_ShiftsAllSets()
        {
            CreateThreeSetsAndSelectMiddle(startState: true);
            ((ITimeLineHost)viewModel).MoveState(1, 4, TimeLine.KeyOperationMode.RightMoveOnly);

            viewModel.UndoCommand.Execute(null);
            viewModel.RedoCommand.Execute(null);
            Assert.Equal([3, 4, 8], Effects[0].Sets.Keys);
            viewModel.UndoCommand.Execute(null);
            Assert.Equal([0, 1, 5], Effects[0].Sets.Keys);
            Assert.Contains("固定", viewModel.CommandManager.UndoneCommand.Peek().Explaination);
        }

        [Fact]
        public void SelectState_UndoRedoRestoresSelection()
        {
            AddImage();
            ((ITimeLineHost)viewModel).AddState(0, 5, true);
            var sets = Effects[0].Sets.Values;
            var manager = viewModel.SelectedManager;
            // タイムラインと同じく、クリックのたびにエフェクト、組、ステートの順に選ぶ
            void Click(EffectStateRatioSet set, bool start)
            {
                manager.Effect = Effects[0];
                manager.Set = set;
                manager.State = start ? set.StartState : set.EndState;
            }
            Click(sets[0], true);
            Click(sets[1], false);

            viewModel.UndoCommand.Execute(null);
            Assert.Same(sets[0], manager.Set);
            Assert.Same(sets[0].StartState, manager.State);
            viewModel.RedoCommand.Execute(null);
            Assert.Same(sets[1].EndState, manager.State);
            viewModel.UndoCommand.Execute(null);
            viewModel.UndoCommand.Execute(null);
            Assert.Null(manager.Effect);
            Assert.Null(manager.State);
        }

        [Fact]
        public void AddDroppedEffectFile_LoadsAsChildEffect()
        {
            AddImage();
            view.SavePath = folder.Combine("child.etd");
            viewModel.SaveAsCommand.Execute(null);
            viewModel.NewEffectCommand.Execute(null);

            viewModel.AddDroppedFiles([folder.Combine("child.etd")]);

            var effect = Assert.IsType<BaseEffect>(Assert.Single(Effects));
            Assert.Single(effect.Effects);
            Assert.StartsWith("エフェクト追加", viewModel.CommandManager.LastCommand.Explaination);
            viewModel.UndoCommand.Execute(null);
            Assert.Empty(Effects);
        }

        [Fact]
        public void EffectCommands_UndoEach()
        {
            AddImage("a.png");
            AddImage("b.png");
            string Names() => string.Join(",", Effects.Select(e => Path.GetFileNameWithoutExtension(((BaseEffect)e).Filename)));
            viewModel.SelectedManager.Effect = Effects[0];

            viewModel.CopyEffectCommand.Execute(null);
            Assert.Equal("a,b,a", Names());
            viewModel.UndoCommand.Execute(null);
            Assert.Equal("a,b", Names());

            viewModel.ReverseEffectsCommand.Execute(null);
            Assert.Equal("b,a", Names());
            viewModel.UndoCommand.Execute(null);
            Assert.Equal("a,b", Names());

            viewModel.ChangeIndex(1, 0);
            Assert.Equal("b,a", Names());
            viewModel.UndoCommand.Execute(null);
            Assert.Equal("a,b", Names());

            viewModel.DeleteEffectCommand.Execute(null);
            Assert.Equal("b", Names());
            viewModel.UndoCommand.Execute(null);
            Assert.Equal("a,b", Names());
            Assert.Empty(view.Messages);
        }

        [Fact]
        public void BlendModeChange_UndoAndMerge()
        {
            AddImage();
            var set = SelectEndState();
            var count = viewModel.CommandManager.DoneCommandNumber;

            viewModel.Property.SelectedBlendMode = BlendMode.Screen;
            viewModel.Property.SelectedBlendMode = BlendMode.Multiply;
            Assert.Equal(count + 2, viewModel.CommandManager.DoneCommandNumber);

            viewModel.UndoCommand.Execute(null);
            Assert.Equal(BlendMode.Screen, set.EndState.BlendMode);
            viewModel.UndoCommand.Execute(null);
            Assert.Equal(BlendMode.None, set.EndState.BlendMode);
        }

        [Fact]
        public void MoveState_IsRecordedOnly()
        {
            AddImage();
            SelectEndState();
            var count = viewModel.CommandManager.DoneCommandNumber;

            ((ITimeLineHost)viewModel).MoveState(0, 3, TimeLine.KeyOperationMode.Move);
            Assert.Equal(count, viewModel.CommandManager.DoneCommandNumber);
            ((ITimeLineHost)viewModel).MoveState(1, 3, TimeLine.KeyOperationMode.LeftMoveOnly);

            Assert.Equal(count + 1, viewModel.CommandManager.DoneCommandNumber);
            Assert.Contains("固定", viewModel.CommandManager.LastCommand.Explaination);
        }

        [Fact]
        public void SelectState_ShowsPropertiesAndRecordsOnce()
        {
            AddImage();
            var set = SelectEndState();
            var count = viewModel.CommandManager.DoneCommandNumber;
            viewModel.SelectedManager.State = set.EndState;

            Assert.Equal(count, viewModel.CommandManager.DoneCommandNumber);
            Assert.Equal(set.EndState.Alpha.ToString(), viewModel.Property.Rows[2].Text);
            Assert.Equal(BlendMode.None, viewModel.Property.SelectedBlendMode);
            // 位置が一定でないので、始めと終わりの位置をカンバスで動かせる
            Assert.True(view.CanvasBezierEditable);
            Assert.Equal(2, view.CanvasBezier.Length);
        }

        [Fact]
        public void PropertyChange_ChangesStateAndMergesHistory()
        {
            AddImage();
            var set = SelectEndState();
            var count = viewModel.CommandManager.DoneCommandNumber;

            viewModel.Property.Rows[0].Text = "12";
            viewModel.Property.Rows[0].Text = "15";
            Assert.Equal(15, set.EndState.X);
            Assert.Equal(count + 1, viewModel.CommandManager.DoneCommandNumber);

            // 範囲の外の値は直す
            viewModel.Property.Rows[2].Text = "3";
            Assert.Equal("1", viewModel.Property.Rows[2].Text);
            Assert.Equal(1, set.EndState.Alpha);

            viewModel.Property.Rows[2].AddValue(-50);
            Assert.Equal(0.5f, set.EndState.Alpha);

            viewModel.UndoCommand.Execute(null);
            Assert.Equal(1, set.EndState.Alpha);
            Assert.Equal("1", viewModel.Property.Rows[2].Text);
        }

        [Fact]
        public void BlendModeChange_IsRecorded()
        {
            AddImage();
            var set = SelectEndState();

            viewModel.Property.SelectedBlendMode = BlendMode.Screen;

            Assert.Equal(BlendMode.Screen, set.EndState.BlendMode);
            Assert.Contains("Screen", viewModel.CommandManager.LastCommand.Explaination);
        }

        [Fact]
        public void PropertyChange_NothingSelected_DoesNothing()
        {
            viewModel.Property.Rows[0].Text = "3";
            viewModel.Property.SelectedBlendMode = BlendMode.Screen;
            viewModel.Property.EditRatioCommand.Execute(RatioType.X);

            Assert.False(viewModel.CommandManager.CanUndo);
        }

        [Fact]
        public void EditRatio_OkSetsRatioMaker()
        {
            AddImage();
            var set = SelectEndState();
            view.Dialog = dialog =>
            {
                var ratio = (RatioChangeViewModel)dialog;
                Assert.Null(ratio.Bcps);
                ratio.Editor = new FakeEditor();
                ratio.OkCommand.Execute(null);
                return true;
            };

            viewModel.Property.EditRatioCommand.Execute(RatioType.Alpha);

            Assert.IsType<BezierRatioMaker>(set[RatioType.Alpha]);
            viewModel.UndoCommand.Execute(null);
            Assert.IsNotType<BezierRatioMaker>(set[RatioType.Alpha]);
        }

        sealed class FakeEditor : IRatioEditor
        {
            public BezierControlPoint[] Bcps { get; set; } = [new BezierControlPoint { Second = new PointF(0, 0), Third = new PointF(0.5f, 0.5f) }, new BezierControlPoint { First = new PointF(0.5f, 0.5f), Second = new PointF(1, 1) }];

            public bool IsLinear => false;

            public void SetLinear()
            {
            }
        }

        [Fact]
        public void BezierEdit_MovesPathResetAndUndo()
        {
            AddImage();
            var set = SelectEndState();
            view.CanvasBezier = [new BezierControlPoint { Second = new PointF(0, 0) }, new BezierControlPoint { Second = new PointF(100, 50) }];

            viewModel.OnBezierEdited();
            view.CanvasBezier = [new BezierControlPoint { Second = new PointF(0, 0) }, new BezierControlPoint { Second = new PointF(200, 80) }];
            viewModel.OnBezierEdited();

            Assert.True(set.IsBezierPosition);
            Assert.Equal((200f, 80f), (set.EndState.X, set.EndState.Y));
            viewModel.ResetPathCommand.Execute(null);
            Assert.False(set.IsBezierPosition);
            viewModel.UndoCommand.Execute(null);
            Assert.True(set.IsBezierPosition);
            Assert.True(view.CanvasBezierEditable);
            viewModel.UndoCommand.Execute(null);
            Assert.False(set.IsBezierPosition);
            Assert.Empty(view.Messages);
        }
    }
}
