using PPDEditor;
using PPDEditor.Models;
using PPDEditor.ViewModels.Docks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Xunit;

namespace UnitTests.PPDEditor
{
    /// <summary>
    /// 位置と角度の保存と当てはめのドックのテストです。保存したデータは一時フォルダーの posdat に置く。
    /// </summary>
    public class PosAndAngleViewModelTests : IDisposable
    {
        sealed class FakePosAndAngleHost : IPosAndAngleHost
        {
            public List<string> Messages { get; } = [];
            public (string FileName, bool WithAngle)? SaveName { get; set; }
            public bool HasMarks { get; set; } = true;
            public bool ApplyResult { get; set; } = true;
            public List<(bool Reverse, int Mode, bool WithAngle)> Applied { get; } = [];
            public (Vector2[] Positions, float[] Angles) Preview { get; private set; }
            public string LastCaption { get; private set; }

            public void ShowMessage(string text) => Messages.Add(text);

            public (string FileName, bool WithAngle)? AskSaveName(string caption)
            {
                LastCaption = caption;
                return SaveName;
            }

            public bool GetSortedMarks(bool reverse, bool inAreaSelection, out Mark[] marks)
            {
                // マークは描画の装置が要るので、空の一覧を返す
                marks = [];
                return HasMarks;
            }

            public bool ApplyToMarks(Vector2[] positions, float[] angles, bool reverse, int mode, bool withAngle)
            {
                Applied.Add((reverse, mode, withAngle));
                return ApplyResult;
            }

            public void ShowPreview(Vector2[] positions, float[] angles) => Preview = (positions, angles);
        }

        readonly TempFolder folder = new(setCurrentDirectory: true);
        readonly FakePosAndAngleHost host = new();

        public PosAndAngleViewModelTests()
        {
            AppFiles.CopyTo("PPDEditor", folder);
            Utility.ChangeLanguage("jp");
            Directory.CreateDirectory("posdat");
            PosAndAngleInfo.Save([new PosAndAngleInfo(new Vector2(100, 50), 1), new PosAndAngleInfo(new Vector2(300, 150), 2)], Path.Combine("posdat", "a.txt"));
            PosAndAngleInfo.Save([new PosAndAngleInfo(new Vector2(10, 20))], Path.Combine("posdat", "b.txt"));
        }

        public void Dispose()
        {
            folder.Dispose();
        }

        [Fact]
        public void SelectFile_ShowsPreview()
        {
            var viewModel = new PosAndAngleViewModel(host);
            Assert.Equal(["a", "b"], viewModel.FileNames);
            Assert.Equal(3, viewModel.LineModeNames.Length);

            viewModel.SelectedFileIndex = 0;
            Assert.Equal([new Vector2(100, 50), new Vector2(300, 150)], host.Preview.Positions);
            Assert.Equal(1, host.Preview.Angles[0], 4);
            viewModel.DisplayAngle = true;
            Assert.True(viewModel.DrawAngle);

            // 角度のないデータ
            viewModel.SelectedFileIndex = 1;
            Assert.False(viewModel.DrawAngle);
            Assert.Equal([0f], host.Preview.Angles);
        }

        [Fact]
        public void ApplyModes_ChangePreviewAndEnabledFields()
        {
            var viewModel = new PosAndAngleViewModel(host) { SelectedFileIndex = 0 };
            Assert.False(viewModel.IsPointEnabled || viewModel.IsLineEnabled || viewModel.IsSymmetricAngleEnabled);

            viewModel.ApplyMode = PosAndAngleApplyMode.Point;
            Assert.True(viewModel.IsPointEnabled && viewModel.IsSymmetricAngleEnabled);
            Assert.Equal(new Vector2(700, 400), host.Preview.Positions[0]);

            viewModel.ApplyMode = PosAndAngleApplyMode.Line;
            Assert.True(viewModel.IsLineXEnabled && viewModel.IsLineYEnabled);
            Assert.Equal(400, host.Preview.Positions[0].Y, 3);

            viewModel.LineMode = LineSymmetryMode.Vertical;
            Assert.Equal(("400", "0", "400", "450"), (viewModel.LineX1, viewModel.LineY1, viewModel.LineX2, viewModel.LineY2));
            Assert.False(viewModel.IsLineYEnabled);
            Assert.Equal(700, host.Preview.Positions[0].X, 3);
            viewModel.LineMode = LineSymmetryMode.Horizontal;
            Assert.Equal(("0", "225", "800", "225"), (viewModel.LineX1, viewModel.LineY1, viewModel.LineX2, viewModel.LineY2));
            Assert.False(viewModel.IsLineXEnabled);
            viewModel.LineMode = LineSymmetryMode.Free;
            Assert.True(viewModel.IsLineXEnabled);

            viewModel.SymmetricAngle = true;
            Assert.InRange(host.Preview.Angles[0], 0, MathF.PI * 2);
        }

        [Fact]
        public void Coordinates_ValidateAndNudge()
        {
            var viewModel = new PosAndAngleViewModel(host)
            {
                PointX = "abc"
            };
            Assert.Equal("0", viewModel.PointX);
            Assert.Equal([Utility.Language["PAALSInvalidNumber"]], host.Messages);

            // 使えない欄は動かさない
            viewModel.Nudge("X", 5);
            Assert.Equal("0", viewModel.PointX);
            viewModel.ApplyMode = PosAndAngleApplyMode.Point;
            viewModel.Nudge("X", 5);
            viewModel.Nudge("Y", -5);
            Assert.Equal(("5", "220"), (viewModel.PointX, viewModel.PointY));

            viewModel.ApplyMode = PosAndAngleApplyMode.Line;
            viewModel.LineMode = LineSymmetryMode.Vertical;
            viewModel.Nudge("X1", 10);
            Assert.Equal(("410", "410"), (viewModel.LineX1, viewModel.LineX2));
            viewModel.Nudge("X2", -20);
            Assert.Equal(("390", "390"), (viewModel.LineX1, viewModel.LineX2));
            viewModel.Nudge("Y1", 10);
            Assert.Equal("0", viewModel.LineY1);

            viewModel.LineMode = LineSymmetryMode.Horizontal;
            viewModel.Nudge("Y1", 10);
            Assert.Equal(("235", "235"), (viewModel.LineY1, viewModel.LineY2));
            viewModel.Nudge("Y2", -5);
            Assert.Equal(("230", "230"), (viewModel.LineY1, viewModel.LineY2));
            viewModel.Nudge("Z", 1);
        }

        [Fact]
        public void Apply_ReportsWhenNothingSelected()
        {
            var viewModel = new PosAndAngleViewModel(host);
            viewModel.ApplyAfterSelectionCommand.Execute(null);
            Assert.Empty(host.Applied);

            viewModel.SelectedFileIndex = 0;
            viewModel.ApplyAfterSelectionCommand.Execute(null);
            viewModel.ApplyBeforeSelectionCommand.Execute(null);
            host.ApplyResult = false;
            viewModel.ApplyInAreaForwardCommand.Execute(null);
            viewModel.ApplyInAreaBackwardCommand.Execute(null);

            Assert.Equal([(false, 0, true), (true, 0, true), (false, 1, true), (true, 1, true)], host.Applied);
            Assert.Equal([Utility.Language["PAALSNoSelectedArea"], Utility.Language["PAALSNoSelectedArea"]], host.Messages);
        }

        [Fact]
        public void Save_WritesFileOrReports()
        {
            var viewModel = new PosAndAngleViewModel(host);

            viewModel.SaveAfterSelectionCommand.Execute(null);
            Assert.Equal(Utility.Language["PAALSButton5"], host.LastCaption);

            host.SaveName = ("saved", true);
            host.HasMarks = false;
            viewModel.SaveBeforeSelectionCommand.Execute(null);
            Assert.Equal([Utility.Language["PAALSNoSelectedPoint"]], host.Messages);

            host.HasMarks = true;
            viewModel.SaveInAreaForwardCommand.Execute(null);
            Assert.True(File.Exists(Path.Combine("posdat", "saved.txt")));
            Assert.Equal(["a", "b", "saved"], viewModel.FileNames);
            viewModel.SaveInAreaBackwardCommand.Execute(null);
            Assert.Equal(Utility.Language["PAALSButton8"], host.LastCaption);
        }

        [Fact]
        public void DeleteAndBrokenFile()
        {
            File.WriteAllText(Path.Combine("posdat", "c.txt"), "broken\n");
            var viewModel = new PosAndAngleViewModel(host);

            viewModel.DeleteSelectedCommand.Execute(null);
            viewModel.SelectedFileIndex = 2;
            viewModel.SelectedFileIndex = 0;
            viewModel.DeleteSelectedCommand.Execute(null);

            Assert.Equal(["b", "c"], viewModel.FileNames);
            Assert.False(File.Exists(Path.Combine("posdat", "a.txt")));
            viewModel.SelectedFileIndex = 5;
        }
    }
}
