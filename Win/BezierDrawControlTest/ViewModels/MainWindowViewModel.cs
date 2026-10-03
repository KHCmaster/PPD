using BezierCaliculator;
using BezierDrawControl;
using BezierDrawControlTest.Models;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Drawing;

namespace BezierDrawControlTest.ViewModels
{
    /// <summary>
    /// ベジエ曲線の編集を試す画面です。格子を描き、点を格子に吸い付けます。
    /// </summary>
    /// <remarks>
    /// 編集の処理（<see cref="BezierController"/>）は描画のコントロールが持っているので、ウィンドウから <see cref="Attach"/> で受け取る。
    /// </remarks>
    public partial class MainWindowViewModel
    {
        const float SnapDistance = 6;

        readonly PointF[] poses = new PointF[10];
        readonly PointF[] dirs = new PointF[10];

        readonly SquareGrid grid = new()
        {
            Width = 50,
            Height = 50,
            OffsetX = 200,
            OffsetY = 200
        };

        BezierController controller;

        public int CanvasWidth
        {
            get { return 800; }
        }

        public int CanvasHeight
        {
            get { return 450; }
        }

        /// <summary>
        /// 描画のコントロールの編集の処理を使います。
        /// </summary>
        public void Attach(BezierController bezierController)
        {
            controller = bezierController;
            controller.AfterPaint += Controller_AfterPaint;
            controller.PreprocessPoint += point => grid.Snap(point, SnapDistance);
            controller.RestrictAngleSplit = 3;
            //CreateCircle();
        }

        [RelayCommand]
        private void Undo()
        {
            controller?.Undo();
        }

        [RelayCommand]
        private void Redo()
        {
            controller?.Redo();
        }

        [RelayCommand]
        private void StartTransform()
        {
            controller?.StartTransform();
        }

        [RelayCommand]
        private void EndTransform()
        {
            controller?.EndTransform();
        }

        [RelayCommand]
        private void Delete()
        {
            controller?.Delete();
        }

        // 曲線の上の点を求めてみて（解析の確認用）、格子を描く
        private void Controller_AfterPaint(object sender, UserPaintEventArgs e)
        {
            if (controller.BCPSCount >= 2)
            {
                var ba = new BezierAnalyzer(controller.BCPS);
                for (int i = 0; i < poses.Length; i++)
                {
                    ba.GetPoint(i * 99.9999f / 9, out PointF pos, out PointF dir);
                    poses[i] = pos;
                    dirs[i] = dir;
                }
                ba.GetYFromX(CanvasWidth / 2);
            }
            for (int i = -grid.OffsetX / grid.Width; i <= CanvasWidth / grid.Width; i++)
            {
                e.Context.DrawLine(Color.Black, new PointF(i * grid.Width + grid.OffsetX, 0), new PointF(i * grid.Width + grid.OffsetX, CanvasHeight));
            }
            for (int i = -grid.OffsetY / grid.Height; i <= CanvasHeight / grid.Height; i++)
            {
                e.Context.DrawLine(Color.Black, new PointF(0, i * grid.Height + grid.OffsetY), new PointF(CanvasWidth, i * grid.Height + grid.OffsetY));
            }
        }

        // 円の曲線を作る（試すとき用）
        private void CreateCircle()
        {
            float temp = 0.5522847f;
            float radius = 100;
            var data = new List<BezierControlPoint>
            {
                new()
                {
                    Second = new PointF(radius, 0),
                    First = new PointF(radius, -radius * temp),
                    Third = new PointF(radius, radius * temp),
                    ValidFirst = true,
                    ValidThird = true
                },
                new()
                {
                    Second = new PointF(0, radius),
                    First = new PointF(radius * temp, radius),
                    Third = new PointF(-radius * temp, radius),
                    ValidFirst = true,
                    ValidThird = true
                },
                new()
                {
                    Second = new PointF(-radius, 0),
                    First = new PointF(-radius, radius * temp),
                    Third = new PointF(-radius, -radius * temp),
                    ValidFirst = true,
                    ValidThird = true
                },
                new()
                {
                    Second = new PointF(0, -radius),
                    First = new PointF(-radius * temp, -radius),
                    Third = new PointF(radius * temp, -radius),
                    ValidFirst = true,
                    ValidThird = true
                }
            };
            data.Add(data[0]);
            controller.BCPS = [.. data];
        }
    }
}
