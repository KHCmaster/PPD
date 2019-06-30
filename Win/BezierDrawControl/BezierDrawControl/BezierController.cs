using BezierCaliculator;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace BezierDrawControl
{
    public class BezierController
    {
        public delegate void PaintEventHandler(object sender, UserPaintEventArgs e);
        public delegate void BezierControlPointEditEventHandler(object sender, BezierControlPointEditEventArgs e);

        public event PaintEventHandler BeforePaint;
        public event PaintEventHandler AfterPaint;
        public event BezierControlPointEditEventHandler Edited;
        public event Func<PointF, PointF> PreprocessPoint;
        public event Action DrawRequired;

        enum TransformMode
        {
            None = 0,
            Still,
            Rotate,
            Move,
            ExpandTop,
            ExpandLeft,
            ExpandBottom,
            ExpandRight,
            ExpandTopLeft,
            ExpandTopRight,
            ExpandBottomLeft,
            ExpandBottomRight
        }

        [Flags]
        enum DrawMode
        {
            None = 0,
            Line = 1,
            Anchor = 2
        }

        enum Mode
        {
            None = 0,
            Move,
            RestrictMove,
            ChangeFirst,
            ChangeThird,
            ChangeBoth,
            ChangeOnlyFirst,
            ChangeOnlyThird
        }

        Mode mode = Mode.None;
        DrawMode dmode = DrawMode.Anchor | DrawMode.Line;
        TransformMode tmode = TransformMode.None;
        bool fixAspect;
        TransformBezierControlPoints transBeziers;
        PointF mouseDownPos;
        //RectangleF mousedownrec;

        private bool changedFlag;
        private bool mouseDown;
        private RingBuffer<List<BezierControlPoint>> bufferData = new RingBuffer<List<BezierControlPoint>>(100);
        private List<BezierControlPoint> data = new List<BezierControlPoint>();
        private BezierControlPoint selectedPoint;

        private BezierControlPoint SelectedPoint
        {
            get { return selectedPoint; }
            set
            {
                selectedPoint = value;
                if (selectedPoint == null) SelectedIndex = -1;
                else SelectedIndex = data.FindIndex((bcp) => (bcp == selectedPoint));
            }
        }

        public int RestrictAngleSplit
        {
            get;
            set;
        }

        public int BCPSCount
        {
            get
            {
                return data.Count;
            }
        }

        public BezierControlPoint[] BCPS
        {
            get
            {
                if (tmode > TransformMode.None)
                {
                    return TransBCPS;
                }
                else
                {
                    return Copy(data).ToArray();
                }
            }
            set
            {
                data = new List<BezierControlPoint>(value);
                bufferData.Add(Copy(data));
                OnDrawRequired();
            }
        }

        public BezierControlPoint[] TransBCPS
        {
            get
            {
                var ret = new List<BezierControlPoint>(transBeziers.BCPSCount);
                foreach (BezierControlPoint bcp in transBeziers.BCPS)
                {
                    ret.Add(bcp);
                }
                return ret.ToArray();
            }
        }

        public bool IsTransformMode
        {
            get
            {
                return tmode != TransformMode.None;
            }
        }

        public PointF TransformScale
        {
            get
            {
                return transBeziers == null ? PointF.Empty : transBeziers.TransformRectange.Scale;
            }
            set
            {
                if (transBeziers != null)
                {
                    transBeziers.TransformRectange.Scale = value;
                }
            }
        }

        public float TransformRotation
        {
            get
            {
                return transBeziers == null ? 0 : transBeziers.TransformRectange.Rotation;
            }
            set
            {
                if (transBeziers != null)
                {
                    transBeziers.TransformRectange.Rotation = value;
                }
            }
        }

        public PointF TransformTranslation
        {
            get
            {
                return transBeziers == null ? PointF.Empty : transBeziers.TransformRectange.Move;
            }
            set
            {
                if (transBeziers != null)
                {
                    transBeziers.TransformRectange.Move = value;
                }
            }
        }

        public bool AllowMouseOperation
        {
            get;
            set;
        }

        public Point Center
        {
            get;
            set;
        }

        public int SelectedIndex
        {
            get;
            private set;
        }

        public BezierController()
        {
            bufferData.Add(Copy(data));
            AllowMouseOperation = true;
        }

        public void Draw(IBezierDrawContext context)
        {
            OnBeforePaint(context);
            if (tmode > TransformMode.None)
            {
                DrawTransData(context);
            }
            else
            {
                DrawNormalData(context);
            }
            OnAfterPaint(context);
        }

        private void DrawTransData(IBezierDrawContext context)
        {
            context.DrawLines(Color.Black, new PointF[] { transBeziers.TransformRectange.TopLeft, transBeziers.TransformRectange.TopRight, transBeziers.TransformRectange.BottomRight, transBeziers.TransformRectange.BottomLeft, transBeziers.TransformRectange.TopLeft });
            DrawSquare(transBeziers.TransformRectange.TopLeft, context);
            DrawSquare(transBeziers.TransformRectange.TopRight, context);
            DrawSquare(transBeziers.TransformRectange.BottomLeft, context);
            DrawSquare(transBeziers.TransformRectange.BottomRight, context);
            DrawSquare(transBeziers.TransformRectange.TopMiddle, context);
            DrawSquare(transBeziers.TransformRectange.LeftMiddle, context);
            DrawSquare(transBeziers.TransformRectange.RightMiddle, context);
            DrawSquare(transBeziers.TransformRectange.BottomMiddle, context);
            PointF center = transBeziers.TransformRectange.Center;
            context.DrawEllipse(Color.Black, new RectangleF
            {
                X = center.X - 3,
                Width = 6,
                Y = center.Y - 3,
                Height = 6
            });

            BezierControlPoint previous = null;
            foreach (BezierControlPoint bcp in transBeziers.BCPS)
            {
                if (previous != null)
                {
                    if (previous.ValidThird)
                    {
                        if (!bcp.ValidFirst)
                        {
                            //second bezier
                            DrawBezeier(previous.Second, previous.Third, bcp.Second, bcp.Second, context);
                        }
                        else
                        {
                            //third bezier
                            DrawBezeier(previous.Second, previous.Third, bcp.First, bcp.Second, context);
                        }
                    }
                    else
                    {
                        if (!bcp.ValidFirst)
                        {
                            //first bezier
                            context.DrawLine(Color.Black, previous.Second, bcp.Second);
                        }
                        else
                        {
                            //second bezier
                            DrawBezeier(previous.Second, previous.Second, bcp.First, bcp.Second, context);
                        }
                    }
                }
                previous = bcp;
            }
        }

        private void DrawNormalData(IBezierDrawContext context)
        {
            BezierControlPoint previous = null;
            for (int i = 0; i < data.Count; i++)
            {
                var bcp = data[i] as BezierControlPoint;
                if ((dmode & DrawMode.Anchor) == DrawMode.Anchor)
                {
                    DrawAnchor(bcp, context);
                }
                if ((dmode & DrawMode.Line) == DrawMode.Line)
                {
                    if (previous != null)
                    {
                        if (previous.ValidThird)
                        {
                            if (!bcp.ValidFirst)
                            {
                                //second bezier
                                DrawBezeier(previous.Second, previous.Third, bcp.Second, bcp.Second, context);
                            }
                            else
                            {
                                //third bezier
                                DrawBezeier(previous.Second, previous.Third, bcp.First, bcp.Second, context);
                            }
                        }
                        else
                        {
                            if (!bcp.ValidFirst)
                            {
                                //first bezier
                                context.DrawLine(Color.Black, previous.Second, bcp.Second);
                            }
                            else
                            {
                                //second bezier
                                DrawBezeier(previous.Second, previous.Second, bcp.First, bcp.Second, context);
                            }
                        }
                    }
                }
                previous = bcp;
            }
        }

        private void DrawAnchor(BezierControlPoint bcp, IBezierDrawContext context)
        {
            if (SelectedPoint == bcp)
            {
                FillSquare(bcp.Second, context);
                if (bcp.ValidFirst)
                {
                    FillSquare(bcp.First, context);
                    context.DrawLine(Color.Black, bcp.First, bcp.Second);
                }
                if (bcp.ValidThird)
                {
                    FillSquare(bcp.Third, context);
                    context.DrawLine(Color.Black, bcp.Second, bcp.Third);
                }
            }
            else
            {
                DrawSquare(bcp.Second, context);
            }
        }

        private void DrawBezeier(PointF p1, PointF p2, PointF p3, PointF p4, IBezierDrawContext context)
        {
            PointF lastbezeirpoint, bezeirpoint;
            lastbezeirpoint = p1;
            var count = BezierCaliculate.AboutBezeirCount(ref p1, ref p2, ref p3, ref p4);
            for (int i = count; i >= 0; i--)
            {
                float t = (float)i / count;
                bezeirpoint = BezierCaliculate.GetBezeirPoint(ref p1, ref p2, ref p3, ref p4, ref t);
                context.DrawLine(Color.Black, lastbezeirpoint, bezeirpoint);
                lastbezeirpoint = bezeirpoint;
            }
        }

        private void FillSquare(PointF p, IBezierDrawContext context)
        {
            context.FillRectangle(Color.Black, p.X - 3, p.Y - 3, 6, 6);
        }

        private void DrawSquare(PointF p, IBezierDrawContext context)
        {
            context.DrawRectangle(Color.Black, p.X - 3, p.Y - 3, 6, 6);
        }

        private PointF CreateDevisionPoint(PointF p1, PointF p2, float t)
        {
            float invt = 1 - t;
            return new PointF(p1.X * t + p2.X * invt, p1.Y * t + p2.Y * invt);
        }

        private float GetLength(PointF p1)
        {
            var length = (float)(Math.Sqrt(p1.X * p1.X + p1.Y * p1.Y));
            return length;
        }

        private PointF GetNormalizePoint(PointF p1, PointF p2)
        {
            var temp = new PointF(p1.X + p2.X, p1.Y + p2.Y);
            return BezierCaliculate.GetNormalizePoint(temp);
        }

        private bool IsInside(PointF target, PointF src)
        {
            if (target.X - 5 <= src.X && src.X <= target.X + 5)
            {
                if (target.Y - 5 <= src.Y && src.Y <= target.Y + 5)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsInside(RectangleF target, PointF src)
        {
            if (target.X <= src.X && src.X <= target.X + target.Width)
            {
                if (target.Y <= src.Y && src.Y <= target.Y + target.Height)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsInside(PointF p, PointF a, PointF b, PointF c)
        {
            if (Cross(new PointF(p.X - a.X, p.Y - a.Y), new PointF(b.X - a.X, b.Y - a.Y)) < 0.0) return false;
            if (Cross(new PointF(p.X - b.X, p.Y - b.Y), new PointF(c.X - b.X, c.Y - b.Y)) < 0.0) return false;
            if (Cross(new PointF(p.X - c.X, p.Y - c.Y), new PointF(a.X - c.X, a.Y - c.Y)) < 0.0) return false;
            return true;
        }

        private bool IsInside(TransformRectangle target, PointF src)
        {
            return IsInside(src, target.TopLeft, target.BottomLeft, target.BottomRight) || IsInside(src, target.BottomRight, target.TopRight, target.TopLeft);
        }

        private float Cross(PointF a, PointF b)
        {
            return a.X * b.Y - a.Y * b.X;
        }

        private PointF ChangePointFromCenter(float x, float y)
        {
            return new PointF(x - Center.X, y - Center.Y);
        }

        public void MouseDown(float x, float y, MouseButtons buttons, Keys modifierKeys)
        {
            if (!AllowMouseOperation) return;
            mouseDown = true;
            var p = ChangePointFromCenter(x, y);
            if (tmode > TransformMode.None)
            {
                TransMouseDown(p, modifierKeys);
            }
            else
            {
                NormalMouseDown(p, buttons == MouseButtons.Left, modifierKeys);
            }
        }

        private void TransMouseDown(PointF downPos, Keys modifierKeys)
        {
            mouseDownPos = downPos;
            fixAspect = ((modifierKeys & Keys.Shift) == Keys.Shift);
            if (IsInside(transBeziers.TransformRectange.TopLeft, downPos))
            {
                tmode = TransformMode.ExpandTopLeft;
            }
            else if (IsInside(transBeziers.TransformRectange.TopRight, downPos))
            {
                tmode = TransformMode.ExpandTopRight;
            }
            else if (IsInside(transBeziers.TransformRectange.BottomLeft, downPos))
            {
                tmode = TransformMode.ExpandBottomLeft;
            }
            else if (IsInside(transBeziers.TransformRectange.BottomRight, downPos))
            {
                tmode = TransformMode.ExpandBottomRight;
            }
            else if (IsInside(transBeziers.TransformRectange.TopMiddle, downPos))
            {
                tmode = TransformMode.ExpandTop;
            }
            else if (IsInside(transBeziers.TransformRectange.BottomMiddle, downPos))
            {
                tmode = TransformMode.ExpandBottom;
            }
            else if (IsInside(transBeziers.TransformRectange.LeftMiddle, downPos))
            {
                tmode = TransformMode.ExpandLeft;
            }
            else if (IsInside(transBeziers.TransformRectange.RightMiddle, downPos))
            {
                tmode = TransformMode.ExpandRight;
            }
            else if (IsInside(transBeziers.TransformRectange, downPos) && (modifierKeys & Keys.Control) == Keys.Control)
            {
                tmode = TransformMode.Move;
            }
            else
            {
                tmode = TransformMode.Rotate;
            }
        }

        private void NormalMouseDown(PointF downpos, bool isLeftButton, Keys modifierKeys)
        {
            if ((modifierKeys & Keys.Control) == Keys.Control)
            {
                bool foundanchor = false;
                if (SelectedPoint != null && SelectedPoint.ValidFirst && SelectedPoint.ValidThird)
                {
                    if (IsInside(SelectedPoint.First, downpos))
                    {
                        foundanchor = true;
                        changedFlag = true;
                        mode = Mode.ChangeFirst;
                    }
                    else if (IsInside(SelectedPoint.Third, downpos))
                    {
                        foundanchor = true;
                        changedFlag = true;
                        mode = Mode.ChangeThird;
                    }
                }
                if (!foundanchor)
                {
                    SelectedPoint = null;
                    for (int i = 0; i < data.Count; i++)
                    {
                        var bcp = data[i] as BezierControlPoint;
                        if (IsInside(bcp.Second, downpos))
                        {
                            SelectedPoint = bcp;
                            changedFlag = true;
                            mode = Mode.Move;
                            break;
                        }
                    }
                }
            }
            else if ((modifierKeys & Keys.Shift) == Keys.Shift)
            {
                SelectedPoint = null;
                for (int i = 0; i < data.Count; i++)
                {
                    var bcp = data[i] as BezierControlPoint;
                    if (IsInside(bcp.Second, downpos))
                    {
                        SelectedPoint = bcp;
                        changedFlag = true;
                        mode = Mode.RestrictMove;
                        break;
                    }
                }
            }
            else if ((modifierKeys & Keys.Alt) == Keys.Alt)
            {
                bool foundanchor = false;
                if (SelectedPoint != null && SelectedPoint.ValidFirst && SelectedPoint.ValidThird)
                {
                    if (IsInside(SelectedPoint.First, downpos))
                    {
                        foundanchor = true;
                        changedFlag = true;
                        mode = Mode.ChangeOnlyFirst;
                    }
                    else if (IsInside(SelectedPoint.Third, downpos))
                    {
                        foundanchor = true;
                        changedFlag = true;
                        mode = Mode.ChangeOnlyThird;
                    }
                }
                if (!foundanchor)
                {
                    SelectedPoint = null;
                    for (int i = 0; i < data.Count; i++)
                    {
                        var bcp = data[i] as BezierControlPoint;
                        if (IsInside(bcp.Second, downpos))
                        {
                            SelectedPoint = bcp;
                            bcp.ValidFirst = false;
                            bcp.ValidThird = false;
                            bcp.FirstDirection = new PointF(0, 0);
                            bcp.FirstLength = 0;
                            bcp.ThirdDirection = new PointF(0, 0);
                            bcp.ThirdLength = 0;
                            mode = Mode.ChangeBoth;
                            changedFlag = true;
                            break;
                        }
                    }
                }
            }
            else if (isLeftButton)
            {
                BezierControlPoint next = null;
                if (IsOnLine(downpos, out BezierControlPoint previous, out float t))
                {
                    if (data.Count >= 2 && previous == data[0] && data[0] != data[data.Count - 1] && t >= 0.95)
                    {
                        //loop
                        SelectedPoint = previous;
                        data.Add(previous);
                        changedFlag = true;
                        OnEdited(EditType.Add, data.Count - 1);
                    }
                    else
                    {
                        var index = data.IndexOf(previous);
                        if (index < data.Count - 1)
                        {
                            next = data[index + 1] as BezierControlPoint;
                        }
                        BezierCaliculate.GetDevidedBeziers(previous, next, t, out BezierControlPoint bcp1, out BezierControlPoint bcp2, out BezierControlPoint bcp3);
                        var previousIndex = data.IndexOf(previous);
                        data[previousIndex] = bcp1;
                        data[previousIndex + 1] = bcp3;
                        data.Insert(previousIndex + 1, bcp2);
                        SelectedPoint = bcp2;
                        changedFlag = true;
                        OnEdited(EditType.Add, previousIndex + 1);
                    }
                }
                else
                {
                    var bcp = new BezierControlPoint
                    {
                        Second = downpos
                    };
                    data.Add(bcp);
                    SelectedPoint = bcp;
                    changedFlag = true;
                    OnEdited(EditType.Add, data.Count - 1);
                }
            }
        }

        private bool IsOnLine(PointF p, out BezierControlPoint previousBCP, out float outt)
        {
            previousBCP = null;
            outt = -1;
            bool ret = false;
            BezierControlPoint previous = null;
            for (int i = 0; i < data.Count; i++)
            {
                var bcp = data[i] as BezierControlPoint;
                if (previous != null)
                {
                    if (previous.ValidThird)
                    {
                        if (!bcp.ValidFirst)
                        {
                            //second bezier
                            if (BezierCaliculate.OnBezeier(previous.Second, previous.Third, bcp.Second, bcp.Second, p, out outt))
                            {
                                previousBCP = previous;
                                return true;
                            }
                        }
                        else
                        {
                            //third bezier
                            if (BezierCaliculate.OnBezeier(previous.Second, previous.Third, bcp.First, bcp.Second, p, out outt))
                            {
                                previousBCP = previous;
                                return true;
                            }
                        }
                    }
                    else
                    {
                        if (!bcp.ValidFirst)
                        {
                            //first bezier
                            if (OnStaraightLine(previous.Second, bcp.Second, p, out outt))
                            {
                                previousBCP = previous;
                                return true;
                            }
                        }
                        else
                        {
                            //second bezier
                            if (BezierCaliculate.OnBezeier(previous.Second, previous.Second, bcp.First, bcp.Second, p, out outt))
                            {
                                previousBCP = previous;
                                return true;
                            }
                        }
                    }
                }
                previous = bcp;
            }
            return ret;
        }

        private float DotProduct(PointF p1, PointF p2)
        {
            return p1.X * p2.X + p1.Y * p2.Y;
        }

        private bool OnStaraightLine(PointF p1, PointF p2, PointF c, out float outt)
        {
            bool found = false;
            outt = -1;
            var evector = new PointF(p2.X - p1.X, p2.Y - p1.Y);
            var cvector = new PointF(c.X - p1.X, c.Y - p1.Y);
            if (CollisionLineAndCircle(p1, p2, c, 2))
            {
                found = true;
            }
            if (found)
            {
                outt = (c.X - p2.X) / (p1.X - p2.X);
                if (outt == 0)
                {
                    outt = (c.Y - p2.Y) / (p1.Y - p2.Y);
                }
                var truepos = CreateDevisionPoint(p1, p2, outt);
                float startt = 0;
                float midt = 0.5f;
                float endt = 1;
                int iter = 0;
                while (iter < 40)
                {
                    float f1 = (startt + midt) / 2;
                    float f2 = (midt + endt) / 2;
                    var v1 = BezierCaliculate.GetBezeirPoint(ref p1, ref p1, ref p2, ref p2, ref f1);
                    var v2 = BezierCaliculate.GetBezeirPoint(ref p1, ref p1, ref p2, ref p2, ref f2);
                    var l1 = GetLength(new PointF(v1.X - truepos.X, v1.Y - truepos.Y));
                    var l2 = GetLength(new PointF(v2.X - truepos.X, v2.Y - truepos.Y));
                    if (l1 < l2)
                    {
                        endt = midt;
                        midt = f1;
                    }
                    else
                    {
                        startt = midt;
                        midt = f2;
                    }
                    outt = midt;
                    iter++;
                }
                return true;
            }
            return false;
        }

        private bool CollisionLineAndCircle(PointF p1, PointF p2, PointF center, float radius)
        {
            var V = new PointF(p2.X - p1.X, p2.Y - p1.Y);
            var C = new PointF(center.X - p1.X, center.Y - p1.Y);

            var n1 = DotProduct(V, C);
            if (n1 < 0)
            {
                return GetLength(C) < radius;
            }
            var n2 = DotProduct(V, V);
            if (n1 > n2)
            {
                var len = (float)(Math.Pow(GetLength(new PointF(center.X - p2.X, center.Y - p2.Y)), 2));

                return len < radius * radius;
            }
            else
            {
                var n3 = DotProduct(C, C);
                return (n3 - (n1 / n2) * n1 < radius * radius);
            }
        }

        public void MouseUp()
        {
            if (!AllowMouseOperation) return;
            mouseDown = false;
            if (tmode > TransformMode.None)
            {
                tmode = TransformMode.Still;
            }
            else
            {
                if (changedFlag)
                {
                    bufferData.Add(Copy(data));
                    changedFlag = false;
                }
                mode = Mode.None;
            }
        }

        public Cursor MouseMove(float x, float y, Keys modifierKeys)
        {
            if (!AllowMouseOperation) return null;
            var p = ChangePointFromCenter(x, y);
            p = OnPreprocessPoint(p);
            if (mouseDown)
            {
                if (tmode > TransformMode.None)
                {
                    TransMouseMove(p);
                }
                else
                {
                    NormalMouseMove(p);
                }
            }
            else
            {
                if (tmode > TransformMode.None)
                {
                    return CheckTransMouseMove(p, modifierKeys);
                }
                else
                {
                    return CheckNormalMouseMove(p, modifierKeys);
                }
            }
            return null;
        }

        private Cursor CheckTransMouseMove(PointF mousePos, Keys modifierKeys)
        {
            if (IsInside(transBeziers.TransformRectange.TopLeft, mousePos))
            {
                return GetCursor(transBeziers.TransformRectange.TopRight, transBeziers.TransformRectange.BottomLeft, transBeziers.TransformRectange.TopLeft);
            }
            else if (IsInside(transBeziers.TransformRectange.TopRight, mousePos))
            {
                return GetCursor(transBeziers.TransformRectange.TopLeft, transBeziers.TransformRectange.BottomRight, transBeziers.TransformRectange.TopRight);
            }
            else if (IsInside(transBeziers.TransformRectange.BottomLeft, mousePos))
            {
                return GetCursor(transBeziers.TransformRectange.TopLeft, transBeziers.TransformRectange.BottomRight, transBeziers.TransformRectange.BottomLeft);
            }
            else if (IsInside(transBeziers.TransformRectange.BottomRight, mousePos))
            {
                return GetCursor(transBeziers.TransformRectange.TopRight, transBeziers.TransformRectange.BottomLeft, transBeziers.TransformRectange.BottomRight);
            }
            else if (IsInside(transBeziers.TransformRectange.TopMiddle, mousePos))
            {
                return GetCursor(transBeziers.TransformRectange.TopLeft, transBeziers.TransformRectange.BottomLeft);
            }
            else if (IsInside(transBeziers.TransformRectange.BottomMiddle, mousePos))
            {
                return GetCursor(transBeziers.TransformRectange.TopLeft, transBeziers.TransformRectange.BottomLeft);
            }
            else if (IsInside(transBeziers.TransformRectange.LeftMiddle, mousePos))
            {
                return GetCursor(transBeziers.TransformRectange.TopLeft, transBeziers.TransformRectange.TopRight);
            }
            else if (IsInside(transBeziers.TransformRectange.RightMiddle, mousePos))
            {
                return GetCursor(transBeziers.TransformRectange.TopLeft, transBeziers.TransformRectange.TopRight);
            }
            else if (IsInside(transBeziers.TransformRectange, mousePos) && (modifierKeys & Keys.Control) == Keys.Control)
            {
                return Cursors.SizeAll;
            }
            else
            {
                return MyCursors.Rotate;
            }
        }

        private Cursor GetCursor(PointF p1, PointF p2)
        {
            var vec1 = BezierCaliculate.GetNormalizePoint(Substruct(p1, p2));
            var rotate = GetRotation(BezierCaliculate.GetNormalizePoint(vec1));
            return MyCursors.CreateArrowCursor((int)(rotate * 180 / Math.PI));
        }

        private Cursor GetCursor(PointF p1, PointF p2, PointF p3)
        {
            var vec1 = BezierCaliculate.GetNormalizePoint(Substruct(p1, p3));
            var vec2 = BezierCaliculate.GetNormalizePoint(Substruct(p2, p3));
            var rotate = GetRotation(Inverse(BezierCaliculate.GetNormalizePoint(Add(vec1, vec2))));
            return MyCursors.CreateArrowCursor((int)(rotate * 180 / Math.PI));
        }

        private Cursor CheckNormalMouseMove(PointF mousePos, Keys modifierKeys)
        {
            if ((modifierKeys & Keys.Control) == Keys.Control)
            {
                if (SelectedPoint != null && SelectedPoint.ValidFirst && SelectedPoint.ValidThird)
                {
                    if (IsInside(SelectedPoint.First, mousePos))
                    {
                        return MyCursors.Handle;
                    }
                    else if (IsInside(SelectedPoint.Third, mousePos))
                    {
                        return MyCursors.Handle;
                    }
                }
                for (int i = 0; i < data.Count; i++)
                {
                    var bcp = data[i] as BezierControlPoint;
                    if (IsInside(bcp.Second, mousePos))
                    {
                        return MyCursors.Anchor;
                    }
                }
                return Cursors.Default;
            }
            else if ((modifierKeys & Keys.Shift) == Keys.Shift)
            {
                for (int i = 0; i < data.Count; i++)
                {
                    var bcp = data[i] as BezierControlPoint;
                    if (IsInside(bcp.Second, mousePos))
                    {
                        return MyCursors.Anchor;
                    }
                }
                return Cursors.Default;
            }
            else if ((modifierKeys & Keys.Alt) == Keys.Alt)
            {
                if (SelectedPoint != null && SelectedPoint.ValidFirst && SelectedPoint.ValidThird)
                {
                    if (IsInside(SelectedPoint.First, mousePos))
                    {
                        return MyCursors.Handle;
                    }
                    else if (IsInside(SelectedPoint.Third, mousePos))
                    {
                        return MyCursors.Handle;
                    }
                }
                for (int i = 0; i < data.Count; i++)
                {
                    var bcp = data[i] as BezierControlPoint;
                    if (IsInside(bcp.Second, mousePos))
                    {
                        return MyCursors.Handle;
                    }
                }
                return Cursors.Default;
            }
            else
            {
                if (IsOnLine(mousePos, out BezierControlPoint previous, out float t))
                {
                    return MyCursors.AnchorPlus;
                }
                else
                {
                    return Cursors.Default;
                }
            }
        }

        private void TransMouseMove(PointF mousePos)
        {
            if (tmode >= TransformMode.ExpandTop)
            {
                switch (tmode)
                {
                    case TransformMode.ExpandBottom:
                        transBeziers.Expand(TransformRectangle.ExpandType.Bottom, mousePos, fixAspect);
                        break;
                    case TransformMode.ExpandBottomLeft:
                        transBeziers.Expand(TransformRectangle.ExpandType.BottomLeft, mousePos, fixAspect);
                        break;
                    case TransformMode.ExpandBottomRight:
                        transBeziers.Expand(TransformRectangle.ExpandType.BottomRight, mousePos, fixAspect);
                        break;
                    case TransformMode.ExpandLeft:
                        transBeziers.Expand(TransformRectangle.ExpandType.Left, mousePos, fixAspect);
                        break;
                    case TransformMode.ExpandRight:
                        transBeziers.Expand(TransformRectangle.ExpandType.Right, mousePos, fixAspect);
                        break;
                    case TransformMode.ExpandTop:
                        transBeziers.Expand(TransformRectangle.ExpandType.Top, mousePos, fixAspect);
                        break;
                    case TransformMode.ExpandTopLeft:
                        transBeziers.Expand(TransformRectangle.ExpandType.TopLeft, mousePos, fixAspect);
                        break;
                    case TransformMode.ExpandTopRight:
                        transBeziers.Expand(TransformRectangle.ExpandType.TopRight, mousePos, fixAspect);
                        break;
                }
            }
            else if (tmode == TransformMode.Move)
            {
                float xdiff = mousePos.X - mouseDownPos.X;
                float ydiff = mousePos.Y - mouseDownPos.Y;
                transBeziers.TransformRectange.Move = new PointF(transBeziers.TransformRectange.Move.X + xdiff, transBeziers.TransformRectange.Move.Y + ydiff);
                mouseDownPos = mousePos;
            }
            else if (tmode == TransformMode.Rotate)
            {
                PointF center = transBeziers.TransformRectange.Center;
                var dir = new PointF(mouseDownPos.X - center.X, mouseDownPos.Y - center.Y);
                var currentdir = new PointF(mousePos.X - center.X, mousePos.Y - center.Y);
                dir = BezierCaliculate.GetNormalizePoint(dir);
                currentdir = BezierCaliculate.GetNormalizePoint(currentdir);
                var rot = GetRotation(dir);
                var currentrot = GetRotation(currentdir);
                transBeziers.TransformRectange.Rotation = transBeziers.TransformRectange.Rotation + (float)((currentrot - rot) * 180 / Math.PI);
                mouseDownPos = mousePos;
            }
            OnEdited(EditType.Transform);
            OnDrawRequired();
        }

        private float GetRotation(PointF p)
        {
            float ret = 0;
            if (p.Y < 0)
            {
                ret += (float)Math.PI;
                ret += (float)(Math.PI - Math.Acos(p.X));
            }
            else
            {
                ret += (float)Math.Acos(p.X);
            }
            return ret;
        }

        private PointF Rotate(PointF p, float r)
        {
            return new PointF((float)(Math.Cos(r) * p.X - Math.Sin(r) * p.Y), (float)(Math.Sin(r) * p.X + Math.Cos(r) * p.Y));
        }

        private PointF Add(PointF p1, PointF p2)
        {
            return new PointF(p1.X + p2.X, p1.Y + p2.Y);
        }

        private PointF Substruct(PointF p1, PointF p2)
        {
            return new PointF(p1.X - p2.X, p1.Y - p2.Y);
        }

        private PointF Inverse(PointF p)
        {
            return new PointF(-p.X, -p.Y);
        }

        private void NormalMouseMove(PointF mousePos)
        {
            if (SelectedPoint != null)
            {
                switch (mode)
                {
                    case Mode.Move:
                        SelectedPoint.Second = mousePos;
                        break;
                    case Mode.RestrictMove:
                        if (RestrictAngleSplit <= 0)
                        {
                            SelectedPoint.Second = mousePos;
                        }
                        else
                        {
                            var neiborbcp = GetNeiborPoint(SelectedPoint);
                            if (neiborbcp != null)
                            {
                                var vec = Substruct(mousePos, neiborbcp.Second);
                                int anglebase = 0;
                                if (vec.X >= 0)
                                {
                                    if (vec.Y >= 0)
                                    {
                                        anglebase = 0;
                                    }
                                    else
                                    {
                                        anglebase = 270;
                                    }
                                }
                                else
                                {
                                    if (vec.Y >= 0)
                                    {
                                        anglebase = 90;
                                    }
                                    else
                                    {
                                        anglebase = 180;
                                    }
                                }
                                var poses = new List<PointF>();
                                var length = GetLength(vec);
                                for (int i = 0; i <= RestrictAngleSplit; i++)
                                {
                                    double angle = Math.PI * ((anglebase + 90 * ((double)i / RestrictAngleSplit))) / 180;
                                    poses.Add(new PointF((float)(length * Math.Cos(angle)), (float)(length * Math.Sin(angle))));
                                }
                                int nearestindex = -1;
                                float nearestlength = float.MaxValue;
                                int index = 0;
                                foreach (PointF pos in poses)
                                {
                                    var p = Substruct(vec, pos);
                                    length = GetLength(p);
                                    if (length < nearestlength)
                                    {
                                        nearestlength = length;
                                        nearestindex = index;
                                    }
                                    index++;
                                }
                                if (index >= 0)
                                {
                                    SelectedPoint.Second = Add(poses[nearestindex], neiborbcp.Second);
                                }
                            }
                        }

                        break;
                    case Mode.ChangeFirst:
                        var f = new PointF(mousePos.X - SelectedPoint.Second.X, mousePos.Y - SelectedPoint.Second.Y);
                        var nv = BezierCaliculate.GetNormalizePoint(f);
                        SelectedPoint.FirstDirection = nv;
                        SelectedPoint.FirstLength = GetLength(f);
                        SelectedPoint.ThirdDirection = new PointF(-nv.X, -nv.Y);
                        break;
                    case Mode.ChangeThird:
                        var t = new PointF(mousePos.X - SelectedPoint.Second.X, mousePos.Y - SelectedPoint.Second.Y);
                        nv = BezierCaliculate.GetNormalizePoint(t);
                        SelectedPoint.ThirdDirection = nv;
                        SelectedPoint.ThirdLength = GetLength(t);
                        SelectedPoint.FirstDirection = new PointF(-nv.X, -nv.Y);
                        break;
                    case Mode.ChangeBoth:
                        f = new PointF(mousePos.X - SelectedPoint.Second.X, mousePos.Y - SelectedPoint.Second.Y);
                        nv = BezierCaliculate.GetNormalizePoint(f);
                        if (IsInside(mousePos, SelectedPoint.Second))
                        {
                            SelectedPoint.ValidFirst = false;
                            SelectedPoint.ValidThird = false;
                        }
                        else
                        {
                            SelectedPoint.ValidFirst = true;
                            SelectedPoint.ValidThird = true;
                            SelectedPoint.FirstDirection = nv;
                            SelectedPoint.FirstLength = GetLength(f);
                            SelectedPoint.ThirdDirection = new PointF(-nv.X, -nv.Y);
                            SelectedPoint.ThirdLength = SelectedPoint.FirstLength;
                        }

                        break;
                    case Mode.ChangeOnlyFirst:
                        f = new PointF(mousePos.X - SelectedPoint.Second.X, mousePos.Y - SelectedPoint.Second.Y);
                        nv = BezierCaliculate.GetNormalizePoint(f);
                        SelectedPoint.FirstDirection = nv;
                        SelectedPoint.FirstLength = GetLength(f);
                        break;
                    case Mode.ChangeOnlyThird:
                        t = new PointF(mousePos.X - SelectedPoint.Second.X, mousePos.Y - SelectedPoint.Second.Y);
                        nv = BezierCaliculate.GetNormalizePoint(t);
                        SelectedPoint.ThirdDirection = nv;
                        SelectedPoint.ThirdLength = GetLength(t);
                        break;
                }

                if (Edited != null)
                {
                    if (mode == Mode.Move) OnEdited(EditType.Anchor, SelectedIndex);
                    else OnEdited(EditType.Handle, SelectedIndex);
                }
                OnDrawRequired();
            }
        }

        private BezierControlPoint GetNeiborPoint(BezierControlPoint bcp)
        {
            if (data == null || data.Count <= 1)
            {
                return null;
            }
            var index = data.IndexOf(bcp);
            if (index < 0)
            {
                return null;
            }
            else
            {
                if (index == 0)
                {
                    return data[1];
                }
                else
                {
                    return data[index - 1];
                }
            }
        }

        public void Undo()
        {
            if (tmode == TransformMode.None && bufferData.CanUndo)
            {
                data = Copy(bufferData.Previous);
                OnDrawRequired();
            }
        }

        public void Redo()
        {
            if (tmode == TransformMode.None && bufferData.CanRedo)
            {
                data = Copy(bufferData.Next);
                OnDrawRequired();
            }
        }

        public void Delete()
        {
            if (SelectedPoint != null)
            {
                var index = SelectedIndex;
                data.Remove(SelectedPoint);
                SelectedPoint = null;
                bufferData.Add(Copy(data));
                OnEdited(EditType.Delete, index);
                OnDrawRequired();
            }
        }

        public void StartTransform()
        {
            if (data.Count < 2)
            {
                tmode = TransformMode.None;
            }
            else
            {
                tmode = TransformMode.Still;
                transBeziers = new TransformBezierControlPoints(data.ToArray());
                OnDrawRequired();
            }
        }

        public void EndTransform()
        {
            if (tmode > TransformMode.None)
            {
                tmode = TransformMode.None;
                int iter = 0;
                foreach (BezierControlPoint bcp in transBeziers.BCPS)
                {
                    data[iter] = bcp;
                    iter++;
                }
                if (data.Count >= 3 && data[0].Serialize() == data[data.Count - 1].Serialize())
                {
                    data[data.Count - 1] = data[0];
                }
                bufferData.Add(Copy(data));
                OnEdited(EditType.TransformEnd);
                OnDrawRequired();
            }
        }

        private List<BezierControlPoint> Copy(List<BezierControlPoint> src)
        {
            var ret = new List<BezierControlPoint>(src.Count);
            bool loop = (src.Count >= 3 && src[0].Serialize() == src[src.Count - 1].Serialize());
            foreach (BezierControlPoint bcp in src)
            {
                ret.Add(bcp.Clone());
            }
            if (loop)
            {
                ret[ret.Count - 1] = ret[0];
            }
            return ret;
        }

        public void Clear()
        {
            data.Clear();
            bufferData.Add(Copy(data));
            OnEdited(EditType.Delete);
            OnDrawRequired();
        }

        private PointF OnPreprocessPoint(PointF point)
        {
            if (PreprocessPoint != null)
            {
                return PreprocessPoint(point);
            }
            return point;
        }

        private void OnBeforePaint(IBezierDrawContext context)
        {
            if (BeforePaint != null)
            {
                BeforePaint.Invoke(this, new UserPaintEventArgs(context));
            }
        }

        private void OnAfterPaint(IBezierDrawContext context)
        {
            if (AfterPaint != null)
            {
                AfterPaint.Invoke(this, new UserPaintEventArgs(context));
            }
        }

        private void OnEdited(EditType editType)
        {
            Edited?.Invoke(this, new BezierControlPointEditEventArgs(editType));
        }

        private void OnEdited(EditType editType, int editIndex)
        {
            Edited?.Invoke(this, new BezierControlPointEditEventArgs(editType, editIndex));
        }

        private void OnDrawRequired()
        {
            DrawRequired?.Invoke();
        }
    }
}
