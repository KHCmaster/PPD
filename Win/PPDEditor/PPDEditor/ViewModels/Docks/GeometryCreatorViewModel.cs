using BezierCaliculator;
using BezierDrawControl;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PPDEditor.Command.PPDSheet;
using PPDEditor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using DrawingColor = System.Drawing.Color;
using PointF = System.Drawing.PointF;
using RectangleF = System.Drawing.RectangleF;

namespace PPDEditor.ViewModels.Docks
{
    /// <summary>
    /// ViewModel から使う、エディターと画面の機能です。
    /// </summary>
    public interface IGeometryHost
    {
        /// <summary>
        /// 選んでいるレイヤーの譜面です。
        /// </summary>
        PPDSheet SelectedSheet { get; }

        float Bpm { get; }

        /// <summary>
        /// 描く場所の大きさです。
        /// </summary>
        (int Width, int Height) CanvasSize { get; }

        void ShowMessage(string text);

        /// <summary>
        /// グリッドを設定させます。
        /// </summary>
        /// <returns>キャンセルしたときは null。</returns>
        SquareGrid EditGrid(SquareGrid grid);

        /// <summary>
        /// 描き直します。
        /// </summary>
        void Redraw();
    }

    /// <summary>
    /// 曲線の形（作るか、描くか）を選び、選んでいる範囲のマークをその上に並べます。
    /// </summary>
    /// <remarks>
    /// 曲線の編集は描画のコントロールが持っている <see cref="BezierController"/> が行うので、<see cref="Attach"/> で受け取る。
    /// </remarks>
    public partial class GeometryCreatorViewModel : DockViewModel
    {
        const int StartX = 100;
        const int StartY = 50;
        const float CircleRadius = 60;
        const float CenterX = 300;
        const float CenterY = 162;

        readonly IGeometryHost host;
        BezierController controller;
        Vector2[] positions = [];
        Vector2[] directions = [];
        bool updatingTransform;

        [ObservableProperty]
        private bool antialias;

        [ObservableProperty]
        private bool drawPos = true;

        [ObservableProperty]
        private bool drawAngle = true;

        [ObservableProperty]
        private bool squareGridEnabled;

        /// <summary>
        /// 曲線を動かすたびにマークに当てはめるかです。
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ApplyCommand))]
        private bool applyMoved;

        /// <summary>
        /// 多角形を作るか（でなければ円）です。
        /// </summary>
        [ObservableProperty]
        private bool createPolygon;

        [ObservableProperty]
        private string polygonCount = "3";

        [ObservableProperty]
        private GeometryIntervalMode intervalMode;

        [ObservableProperty]
        private string fixedDistanceText = "30";

        [ObservableProperty]
        private GeometryAngleMode angleMode;

        /// <summary>
        /// Shift+ドラッグで動かすときの角度の刻みです（0 は制限なし）。
        /// </summary>
        [ObservableProperty]
        private decimal angleRestriction;

        /// <summary>
        /// 変形しているかです（変形の値の欄を出す）。
        /// </summary>
        [ObservableProperty]
        private bool isTransforming;

        [ObservableProperty]
        private string transformWidth = "";

        [ObservableProperty]
        private string transformHeight = "";

        [ObservableProperty]
        private string transformRotation = "";

        [ObservableProperty]
        private string transformX = "";

        [ObservableProperty]
        private string transformY = "";

        public GeometryCreatorViewModel(IGeometryHost host)
        {
            this.host = host;
        }

        public SquareGrid SquareGrid
        {
            get;
            set;
        } = new SquareGrid
        {
            GridColor = DrawingColor.Green,
            Height = 25,
            Width = 25,
            OffsetX = 0,
            OffsetY = 0
        };

        /// <summary>
        /// 4 分固定距離のときの、4 分音符 1 つ分の距離です。
        /// </summary>
        public int FixedDistance
        {
            get { return int.TryParse(FixedDistanceText, out int value) ? value : 0; }
            set { FixedDistanceText = value.ToString(); }
        }

        public bool IsFixedDistanceEnabled
        {
            get { return IntervalMode == GeometryIntervalMode.FixedDistance; }
        }

        /// <summary>
        /// マウスを止めたときに出す、今のモードの操作方法です。
        /// </summary>
        public string HintText
        {
            get { return controller != null && controller.IsTransformMode ? Texts["GCHint2"] : Texts["GCHint1"]; }
        }

        /// <summary>
        /// 描画のコントロールの編集の処理を使います。
        /// </summary>
        public void Attach(BezierController bezierController)
        {
            controller = bezierController;
            controller.BeforePaint += Controller_BeforePaint;
            controller.AfterPaint += Controller_AfterPaint;
            controller.PreprocessPoint += SnapToGrid;
        }

        partial void OnDrawPosChanged(bool value) => host.Redraw();

        partial void OnDrawAngleChanged(bool value) => host.Redraw();

        partial void OnSquareGridEnabledChanged(bool value) => host.Redraw();

        partial void OnAngleModeChanged(GeometryAngleMode value) => host.Redraw();

        partial void OnIntervalModeChanged(GeometryIntervalMode value)
        {
            OnPropertyChanged(nameof(IsFixedDistanceEnabled));
            host.Redraw();
        }

        partial void OnFixedDistanceTextChanged(string value)
        {
            if (FixedDistance != 0)
            {
                host.Redraw();
            }
        }

        partial void OnAngleRestrictionChanged(decimal value)
        {
            if (controller != null)
            {
                controller.RestrictAngleSplit = value == 0 ? 0 : 90 / (int)value;
            }
        }

        partial void OnTransformWidthChanged(string value) => ApplyTransformValues();

        partial void OnTransformHeightChanged(string value) => ApplyTransformValues();

        partial void OnTransformRotationChanged(string value) => ApplyTransformValues();

        partial void OnTransformXChanged(string value) => ApplyTransformValues();

        partial void OnTransformYChanged(string value) => ApplyTransformValues();

        // 変形の値の欄を書き換えたら、変形に当てはめる
        private void ApplyTransformValues()
        {
            if (updatingTransform || controller == null)
            {
                return;
            }
            controller.TransformScale = new PointF(ParseValue(TransformWidth, "%") / 100, ParseValue(TransformHeight, "%") / 100);
            controller.TransformTranslation = new PointF(ParseValue(TransformX, "px"), ParseValue(TransformY, "px"));
            controller.TransformRotation = ParseValue(TransformRotation, "");
            host.Redraw();
        }

        private static float ParseValue(string text, string unit)
        {
            if (text == null)
            {
                return 0;
            }
            if (unit != "" && text.EndsWith(unit))
            {
                text = text[..^unit.Length];
            }
            return float.TryParse(text, out float value) ? value : 0;
        }

        // 変形しているときは、今の変形の値を欄に出す
        private void ShowTransformValues()
        {
            updatingTransform = true;
            TransformWidth = (controller.TransformScale.X * 100) + "%";
            TransformHeight = (controller.TransformScale.Y * 100) + "%";
            TransformRotation = controller.TransformRotation.ToString();
            TransformX = controller.TransformTranslation.X + "px";
            TransformY = controller.TransformTranslation.Y + "px";
            updatingTransform = false;
        }

        private void Controller_BeforePaint(object sender, UserPaintEventArgs e)
        {
            if (SquareGrid == null || !SquareGridEnabled)
            {
                return;
            }
            var context = e.Context;
            var (width, height) = host.CanvasSize;
            for (int i = (-SquareGrid.NormalizedOffsetX - StartX) / SquareGrid.Width; i <= width / SquareGrid.Width; i++)
            {
                context.DrawLine(SquareGrid.GridColor, new PointF(i * SquareGrid.Width + SquareGrid.OffsetX + StartX, 0),
                    new PointF(i * SquareGrid.Width + SquareGrid.OffsetX + StartX, height));
            }
            for (int i = (-SquareGrid.NormalizedOffsetY - StartY) / SquareGrid.Height; i <= height / SquareGrid.Height; i++)
            {
                context.DrawLine(SquareGrid.GridColor, new PointF(0, i * SquareGrid.Height + SquareGrid.OffsetY + StartY),
                    new PointF(width, i * SquareGrid.Height + SquareGrid.OffsetY + StartY));
            }
        }

        // マークを並べる位置と向きを求め直して、描く（曲線を動かすたびに当てはめるときは、ここで当てはめる）
        private void Controller_AfterPaint(object sender, UserPaintEventArgs e)
        {
            var context = e.Context;
            UpdatePlacement();
            if (controller.BCPSCount < 2 || controller.IsTransformMode)
            {
                if (controller.IsTransformMode)
                {
                    ShowTransformValues();
                }
            }
            else
            {
                if (DrawPos)
                {
                    foreach (var pos in positions)
                    {
                        context.DrawEllipse(pos == positions[0] ? DrawingColor.Green : DrawingColor.Black, new RectangleF(pos.X - 5, pos.Y - 5, 10, 10));
                    }
                }
                if (DrawAngle && AngleMode != GeometryAngleMode.None)
                {
                    for (int i = 0; i < directions.Length; i++)
                    {
                        DrawArrow(new PointF(positions[i].X, positions[i].Y), new PointF(directions[i].X, directions[i].Y), context);
                    }
                }
            }
            context.DrawRectangle(DrawingColor.Red, StartX, StartY, 400, 225);
        }

        private void UpdatePlacement()
        {
            var sheet = host.SelectedSheet;
            if (sheet == null)
            {
                positions = [];
                directions = [];
                return;
            }
            var marks = sheet.GetAreaData();
            (positions, directions) = GeometryPlacement.Compute(controller.BCPS, [.. marks.Select(m => m.Time)], IntervalMode, AngleMode, host.Bpm, FixedDistance);
            if (ApplyMoved)
            {
                ApplyToMarks();
            }
        }

        private static void DrawArrow(PointF pos, PointF dir, IBezierDrawContext context)
        {
            float length = 100;
            float length2 = 30;
            context.DrawLine(DrawingColor.Blue, pos, new PointF(pos.X - dir.X * length, pos.Y - dir.Y * length));
            var r1 = GeometryPlacement.Rotate(new Vector2(dir.X, dir.Y), (float)(Math.PI / 6));
            var r2 = GeometryPlacement.Rotate(new Vector2(dir.X, dir.Y), (float)(-Math.PI / 6));
            context.DrawLine(DrawingColor.Blue, pos, new PointF(pos.X - r1.X * length2, pos.Y - r1.Y * length2));
            context.DrawLine(DrawingColor.Blue, pos, new PointF(pos.X - r2.X * length2, pos.Y - r2.Y * length2));
        }

        // グリッドを表示しているときは、点をグリッドに吸い付ける
        private PointF SnapToGrid(PointF point)
        {
            if (SquareGrid == null || !SquareGridEnabled)
            {
                return point;
            }
            var x = point.X - StartX;
            var y = point.Y - StartY;
            var nearest = new PointF((int)((x + SquareGrid.Width / 2f) / SquareGrid.Width) * SquareGrid.Width + SquareGrid.NormalizedOffsetX,
                (int)((y + SquareGrid.Height / 2f) / SquareGrid.Height) * SquareGrid.Height + SquareGrid.NormalizedOffsetY);
            var diffX = nearest.X - x;
            var diffY = nearest.Y - y;
            if (Math.Sqrt(diffX * diffX + diffY * diffY) >= 10)
            {
                return point;
            }
            return new PointF(nearest.X + StartX, nearest.Y + StartY);
        }

        private bool ApplyToMarks()
        {
            if (controller.BCPSCount < 2)
            {
                return false;
            }
            var sheet = host.SelectedSheet;
            if (sheet == null)
            {
                return false;
            }
            sheet.ApplyTrans(positions, directions, AngleMode != GeometryAngleMode.None);
            return true;
        }

        private bool CanApply()
        {
            return !ApplyMoved;
        }

        [RelayCommand(CanExecute = nameof(CanApply))]
        private void Apply()
        {
            ApplyToMarks();
        }

        /// <summary>
        /// 円か多角形の曲線を作ります。
        /// </summary>
        [RelayCommand]
        private void Generate()
        {
            if (!CreatePolygon)
            {
                CreateCircle();
                return;
            }
            if (!int.TryParse(PolygonCount, out int count))
            {
                host.ShowMessage(Texts["GCError1"]);
            }
            else if (count < 3)
            {
                host.ShowMessage(Texts["GCError2"]);
            }
            else
            {
                CreatePolygonPoints(count);
            }
        }

        private void CreateCircle()
        {
            float temp = 0.5522847f;
            var data = new List<BezierControlPoint>
            {
                new()
                {
                    Second = new PointF(CenterX + CircleRadius, CenterY),
                    First = new PointF(CenterX + CircleRadius, CenterY - CircleRadius * temp),
                    Third = new PointF(CenterX + CircleRadius, CenterY + CircleRadius * temp),
                    ValidFirst = true,
                    ValidThird = true
                },
                new()
                {
                    Second = new PointF(CenterX, CenterY + CircleRadius),
                    First = new PointF(CenterX + CircleRadius * temp, CenterY + CircleRadius),
                    Third = new PointF(CenterX - CircleRadius * temp, CenterY + CircleRadius),
                    ValidFirst = true,
                    ValidThird = true
                },
                new()
                {
                    Second = new PointF(CenterX - CircleRadius, CenterY),
                    First = new PointF(CenterX - CircleRadius, CenterY + CircleRadius * temp),
                    Third = new PointF(CenterX - CircleRadius, CenterY - CircleRadius * temp),
                    ValidFirst = true,
                    ValidThird = true
                },
                new()
                {
                    Second = new PointF(CenterX, CenterY - CircleRadius),
                    First = new PointF(CenterX - CircleRadius * temp, CenterY - CircleRadius),
                    Third = new PointF(CenterX + CircleRadius * temp, CenterY - CircleRadius),
                    ValidFirst = true,
                    ValidThird = true
                }
            };
            data.Add(data[0]);
            controller.BCPS = [.. data];
        }

        private void CreatePolygonPoints(int count)
        {
            var data = new List<BezierControlPoint>();
            for (int i = 0; i < count; i++)
            {
                var cos = (float)Math.Cos(Math.PI * 2 * i / count);
                var sin = (float)Math.Sin(Math.PI * 2 * i / count);
                data.Add(new BezierControlPoint { Second = new PointF(CenterX + CircleRadius * cos, CenterY + CircleRadius * sin) });
            }
            data.Add(data[0]);
            controller.BCPS = [.. data];
        }

        [RelayCommand]
        private void Clear()
        {
            if (!controller.IsTransformMode)
            {
                controller.Clear();
            }
        }

        /// <summary>
        /// 多角形の角数の名前をドラッグして、角数を変えます。
        /// </summary>
        public void NudgePolygonCount(int delta)
        {
            if (int.TryParse(PolygonCount, out int value))
            {
                PolygonCount = (value + delta).ToString();
            }
        }

        // グリッドの設定の画面では、2 倍の大きさで表示する（以前と同じ）
        [RelayCommand]
        private void EditGrid()
        {
            var grid = host.EditGrid(new SquareGrid
            {
                Width = SquareGrid.Width * 2,
                Height = SquareGrid.Height * 2,
                GridColor = SquareGrid.GridColor,
                OffsetX = SquareGrid.OffsetX * 2,
                OffsetY = SquareGrid.OffsetY * 2
            });
            if (grid != null)
            {
                SquareGrid = new SquareGrid
                {
                    Width = grid.Width / 2,
                    Height = grid.Height / 2,
                    GridColor = grid.GridColor,
                    OffsetX = grid.OffsetX / 2,
                    OffsetY = grid.OffsetY / 2
                };
                host.Redraw();
            }
        }

        [RelayCommand]
        private void ResetTransform()
        {
            if (controller.IsTransformMode)
            {
                controller.TransformRotation = 0;
                controller.TransformScale = new PointF(1, 1);
                controller.TransformTranslation = new PointF(0, 0);
                host.Redraw();
            }
        }

        [RelayCommand]
        private void Undo()
        {
            controller.Undo();
        }

        [RelayCommand]
        private void Redo()
        {
            controller.Redo();
        }

        [RelayCommand]
        private void StartTransform()
        {
            IsTransforming = true;
            controller.StartTransform();
        }

        [RelayCommand]
        private void EndTransform()
        {
            controller.EndTransform();
            IsTransforming = false;
        }

        [RelayCommand]
        private void Delete()
        {
            controller.Delete();
        }
    }
}
