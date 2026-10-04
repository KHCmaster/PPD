using BezierCaliculator;
using Effect2DEditor.ViewModels.Dialogs;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Effect2DEditor
{
    /// <summary>
    /// 変化の割合を編集するウィンドウです。マウスカーソルの位置に開きます。処理は <see cref="RatioChangeViewModel"/> にあります。
    /// </summary>
    public sealed partial class RatioChangeWindow : Window, IRatioEditor
    {
        public RatioChangeWindow()
        {
            InitializeComponent();
            DataContextChanged += (sender, e) =>
            {
                if (e.NewValue is RatioChangeViewModel viewModel)
                {
                    viewModel.Editor = this;
                    if (viewModel.Bcps != null)
                    {
                        Bcps = viewModel.Bcps;
                    }
                }
            };
        }

        public BezierControlPoint[] Bcps
        {
            get
            {
                return bezierRatioDrawer1.BCPS;
            }
            set
            {
                bezierRatioDrawer1.BCPS = value;
                bezierRatioDrawer1.DrawandRefresh();
            }
        }

        public bool IsLinear
        {
            get { return bezierRatioDrawer1.IsLinear; }
        }

        void IRatioEditor.SetLinear()
        {
            bezierRatioDrawer1.SetLinearRatio();
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            // カーソルの位置（物理ピクセル）を WPF の座標にして、そこに置く
            if (NativeMethods.GetCursorPos(out var point) && PresentationSource.FromVisual(this) is HwndSource source)
            {
                var position = source.CompositionTarget.TransformFromDevice.Transform(new Point(point.X, point.Y));
                Left = position.X;
                Top = position.Y;
            }
        }

        static class NativeMethods
        {
            [StructLayout(LayoutKind.Sequential)]
            internal struct POINT
            {
                public int X;
                public int Y;
            }

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool GetCursorPos(out POINT lpPoint);
        }
    }
}
