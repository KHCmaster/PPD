using BezierCaliculator;
using PPDConfiguration;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Effect2DEditor
{
    /// <summary>
    /// 変化の割合を編集するウィンドウです。マウスカーソルの位置に開きます。
    /// </summary>
    public partial class RatioChangeWindow : Window
    {
        public RatioChangeWindow()
        {
            InitializeComponent();
        }
        public void SetLang(SettingReader setting)
        {
            if (setting != null)
            {
                button3.Content = setting["RCFButton3"];
                button4.Content = setting["RCFButton4"];
                button5.Content = setting["RCFButton5"];
            }
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
        public MainWindow MainForm
        {
            get;
            set;
        }
        public bool IsLinear
        {
            get { return bezierRatioDrawer1.IsLinear; }
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

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            bezierRatioDrawer1.SetLinearRatio();
        }

        private void Button4_Click(object sender, RoutedEventArgs e)
        {
            BezierControlPoint[] bcps = new BezierControlPoint[2];
            for (int i = 0; i < bcps.Length; i++)
            {
                bcps[i] = bezierRatioDrawer1.BCPS[i].Clone();
            }
            MainForm.CopiedBezier = bcps;
        }

        private void Button5_Click(object sender, RoutedEventArgs e)
        {
            if (MainForm.CopiedBezier != null)
            {
                BezierControlPoint[] bcps = new BezierControlPoint[2];
                for (int i = 0; i < bcps.Length; i++)
                {
                    bcps[i] = MainForm.CopiedBezier[i].Clone();
                }
                bezierRatioDrawer1.BCPS = bcps;
                bezierRatioDrawer1.DrawandRefresh();
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
