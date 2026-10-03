using System;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;
using Point = Vortice.Mathematics.Int2;

namespace PPDEditor.Controls
{
    /// <summary>
    /// ゲームの画面です。DirectX が描く Win32 のウィンドウを、WPF の中に埋め込みます。
    /// </summary>
    /// <remarks>
    /// ドッキングで別のウィンドウに移すと、WPF は埋め込んだウィンドウを作り直そうとする。
    /// スワップチェーンはウィンドウに結びついているので、ウィンドウは作り直さずに、親を付け替えて使い回す
    /// （外されている間は、表示しない親のウィンドウに置いておく）。
    /// </remarks>
    public sealed class GameView : HwndHost
    {
        const string ClassName = "PPDEditorGameView";
        const uint WS_CHILD = 0x40000000;
        const uint WS_POPUP = 0x80000000;
        const uint WS_CLIPSIBLINGS = 0x04000000;
        const uint WS_CLIPCHILDREN = 0x02000000;
        const int SW_SHOW = 5;
        const uint WM_MOUSEMOVE = 0x0200;
        const uint WM_LBUTTONDOWN = 0x0201;
        const uint WM_LBUTTONUP = 0x0202;
        const uint WM_RBUTTONUP = 0x0205;
        const uint WM_MOUSELEAVE = 0x02A3;
        const uint WM_KEYDOWN = 0x0100;
        const uint WM_SYSKEYDOWN = 0x0104;
        const uint WM_GETDLGCODE = 0x0087;
        const uint WM_ERASEBKGND = 0x0014;
        const uint WM_CAPTURECHANGED = 0x0215;
        const int DLGC_WANTALLKEYS = 0x0004;
        const uint TME_LEAVE = 0x00000002;
        const int IDC_ARROW = 32512;

        // ウィンドウのクラスは 1 回だけ登録する。WndProc はガベージコレクションされないように持っておく
        static readonly NativeMethods.WndProc classWndProc = ClassWndProc;
        static bool classRegistered;
        static IntPtr parkingWindow;
        static GameView instance;

        // 描画先のウィンドウ（1 つしか作らないので static に持つ）
        static IntPtr hwnd;
        bool trackingLeave;

        /// <summary>
        /// 左のボタンを押したときに発生します。
        /// </summary>
        public event EventHandler<Point> GameMouseDown;

        /// <summary>
        /// 左のボタンを離したときに発生します。
        /// </summary>
        public event EventHandler<Point> GameMouseUp;

        /// <summary>
        /// マウスが動いたときに発生します。
        /// </summary>
        public event EventHandler<Point> GameMouseMove;

        /// <summary>
        /// マウスが出ていったときに発生します。
        /// </summary>
        public event EventHandler GameMouseLeave;

        /// <summary>
        /// 右のボタンを離したときに発生します（メニューを出す）。
        /// </summary>
        public event EventHandler GameRightClick;

        /// <summary>
        /// キーを押したときに発生します。処理したら <see cref="GameKeyEventArgs.Handled"/> を true にする。
        /// </summary>
        public event EventHandler<GameKeyEventArgs> GameKeyDown;

        /// <summary>
        /// 描画先のウィンドウです。
        /// </summary>
        public IntPtr RenderHandle
        {
            get { return hwnd; }
        }

        /// <summary>
        /// 描画先のウィンドウの大きさ（ピクセル）です。
        /// </summary>
        public int ClientWidth
        {
            get
            {
                NativeMethods.GetClientRect(hwnd, out var rect);
                return rect.Right - rect.Left;
            }
        }

        public int ClientHeight
        {
            get
            {
                NativeMethods.GetClientRect(hwnd, out var rect);
                return rect.Bottom - rect.Top;
            }
        }

        public GameView()
        {
            if (instance != null)
            {
                throw new InvalidOperationException("GameView can be created only once.");
            }
            instance = this;
            RegisterClass();
            parkingWindow = NativeMethods.CreateWindowEx(0, ClassName, "", WS_POPUP, 0, 0, 800, 450,
                IntPtr.Zero, IntPtr.Zero, NativeMethods.GetModuleHandle(null), IntPtr.Zero);
            hwnd = NativeMethods.CreateWindowEx(0, ClassName, "", WS_CHILD | WS_CLIPSIBLINGS | WS_CLIPCHILDREN, 0, 0, 800, 450,
                parkingWindow, IntPtr.Zero, NativeMethods.GetModuleHandle(null), IntPtr.Zero);
            if (hwnd == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to create the game window: " + Marshal.GetLastWin32Error());
            }
        }

        private static void RegisterClass()
        {
            if (classRegistered)
            {
                return;
            }
            var wndClass = new NativeMethods.WNDCLASSEX
            {
                cbSize = (uint)Marshal.SizeOf<NativeMethods.WNDCLASSEX>(),
                lpfnWndProc = Marshal.GetFunctionPointerForDelegate(classWndProc),
                hInstance = NativeMethods.GetModuleHandle(null),
                hCursor = NativeMethods.LoadCursor(IntPtr.Zero, IDC_ARROW),
                lpszClassName = ClassName
            };
            if (NativeMethods.RegisterClassEx(ref wndClass) == 0)
            {
                throw new InvalidOperationException("Failed to register the game window class: " + Marshal.GetLastWin32Error());
            }
            classRegistered = true;
        }

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            NativeMethods.SetParent(hwnd, hwndParent.Handle);
            NativeMethods.ShowWindow(hwnd, SW_SHOW);
            return new HandleRef(this, hwnd);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            // 壊さずに、表示しない親に移して置いておく
            NativeMethods.SetParent(hwnd.Handle, parkingWindow);
        }

        /// <summary>
        /// 終了するときに、描画先のウィンドウを壊します（DirectX を解放した後に呼ぶ）。
        /// </summary>
        public static void DestroyRenderWindow()
        {
            NativeMethods.DestroyWindow(hwnd);
            NativeMethods.DestroyWindow(parkingWindow);
            hwnd = IntPtr.Zero;
            parkingWindow = IntPtr.Zero;
            instance = null;
        }

        private static IntPtr ClassWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            var view = instance;
            if (view != null && hWnd == hwnd)
            {
                var result = view.ProcessMessage(msg, wParam, lParam, out bool handled);
                if (handled)
                {
                    return result;
                }
            }
            return NativeMethods.DefWindowProc(hWnd, msg, wParam, lParam);
        }

        private IntPtr ProcessMessage(uint msg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            handled = false;
            switch (msg)
            {
                case WM_ERASEBKGND:
                    // DirectX が描くので、背景は塗らない（ちらつき防止）
                    handled = true;
                    return new IntPtr(1);
                case WM_GETDLGCODE:
                    // 矢印や Tab もこのウィンドウで受け取る
                    handled = true;
                    return new IntPtr(DLGC_WANTALLKEYS);
                case WM_LBUTTONDOWN:
                    NativeMethods.SetFocus(hwnd);
                    NativeMethods.SetCapture(hwnd);
                    GameMouseDown?.Invoke(this, GetPoint(lParam));
                    handled = true;
                    break;
                case WM_LBUTTONUP:
                    NativeMethods.ReleaseCapture();
                    GameMouseUp?.Invoke(this, GetPoint(lParam));
                    handled = true;
                    break;
                case WM_RBUTTONUP:
                    NativeMethods.SetFocus(hwnd);
                    GameRightClick?.Invoke(this, EventArgs.Empty);
                    handled = true;
                    break;
                case WM_MOUSEMOVE:
                    if (!trackingLeave)
                    {
                        var track = new NativeMethods.TRACKMOUSEEVENT
                        {
                            cbSize = (uint)Marshal.SizeOf<NativeMethods.TRACKMOUSEEVENT>(),
                            dwFlags = TME_LEAVE,
                            hwndTrack = hwnd
                        };
                        trackingLeave = NativeMethods.TrackMouseEvent(ref track);
                    }
                    GameMouseMove?.Invoke(this, GetPoint(lParam));
                    handled = true;
                    break;
                case WM_MOUSELEAVE:
                    trackingLeave = false;
                    GameMouseLeave?.Invoke(this, EventArgs.Empty);
                    handled = true;
                    break;
                case WM_CAPTURECHANGED:
                    break;
                case WM_KEYDOWN:
                case WM_SYSKEYDOWN:
                    var key = KeyInterop.KeyFromVirtualKey(wParam.ToInt32());
                    var args = new GameKeyEventArgs(key, GetModifiers());
                    GameKeyDown?.Invoke(this, args);
                    handled = args.Handled;
                    break;
            }
            return IntPtr.Zero;
        }

        /// <summary>
        /// 修飾キーの状態です。WPF の外のウィンドウなので、システムから直接読む。
        /// </summary>
        public static ModifierKeys GetModifiers()
        {
            const int VK_SHIFT = 0x10, VK_CONTROL = 0x11, VK_MENU = 0x12;
            var modifiers = ModifierKeys.None;
            if (NativeMethods.GetKeyState(VK_SHIFT) < 0) modifiers |= ModifierKeys.Shift;
            if (NativeMethods.GetKeyState(VK_CONTROL) < 0) modifiers |= ModifierKeys.Control;
            if (NativeMethods.GetKeyState(VK_MENU) < 0) modifiers |= ModifierKeys.Alt;
            return modifiers;
        }

        private static Point GetPoint(IntPtr lParam)
        {
            var value = lParam.ToInt64();
            return new Point((short)(value & 0xFFFF), (short)((value >> 16) & 0xFFFF));
        }
    }

    /// <summary>
    /// ゲームの画面で押したキーです。
    /// </summary>
    public class GameKeyEventArgs(Key key, ModifierKeys modifiers) : EventArgs
    {
        public Key Key
        {
            get;
        } = key;

        public ModifierKeys Modifiers
        {
            get;
        } = modifiers;

        public bool Handled
        {
            get;
            set;
        }
    }
}
