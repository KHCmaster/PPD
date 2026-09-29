using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

/// <summary>
/// PPDTest を起動し、Y キーで全シーンを送りながらスクリーンショットを撮ります。
/// 使い方: SceneCapture.exe <PPDTest.exe> <出力フォルダ> <シーン数>
/// </summary>
static class SceneCapture
{
    [DllImport("user32.dll")] static extern bool SetProcessDPIAware();
    [DllImport("user32.dll")] static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] static extern bool GetClientRect(IntPtr hWnd, out RECT rect);
    [DllImport("user32.dll")] static extern bool ClientToScreen(IntPtr hWnd, ref POINT point);
    [DllImport("user32.dll")] static extern void keybd_event(byte vk, byte scan, uint flags, UIntPtr extra);
    [DllImport("user32.dll")] static extern bool ShowWindow(IntPtr hWnd, int cmd);

    struct RECT { public int Left, Top, Right, Bottom; }
    struct POINT { public int X, Y; }

    const uint KEYEVENTF_KEYUP = 2;
    const byte VK_MENU = 0x12;
    const byte VK_Y = 0x59;
    const byte SCAN_Y = 0x15;

    static int Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("usage: SceneCapture.exe <PPDTest.exe> <outputDir> <sceneCount>");
            return 2;
        }
        SetProcessDPIAware();
        var exe = Path.GetFullPath(args[0]);
        var outDir = args[1];
        var sceneCount = int.Parse(args[2]);
        Directory.CreateDirectory(outDir);
        var process = Process.Start(new ProcessStartInfo(exe) { WorkingDirectory = Path.GetDirectoryName(exe), UseShellExecute = false });
        try
        {
            var hwnd = WaitForWindow(process);
            if (hwnd == IntPtr.Zero)
            {
                Console.WriteLine("window not found");
                return 1;
            }
            Thread.Sleep(4000);
            for (var scene = 0; scene < sceneCount; scene++)
            {
                if (!BringToFront(hwnd))
                {
                    Console.WriteLine("could not bring window to front at scene " + scene);
                    return 1;
                }
                Thread.Sleep(2500);
                if (process.HasExited)
                {
                    Console.WriteLine("process exited at scene " + scene + " code " + process.ExitCode);
                    return 1;
                }
                var path = Path.Combine(outDir, string.Format("scene{0}.png", scene));
                var stable = CaptureStable(hwnd, path);
                Console.WriteLine("captured " + path + (stable ? "" : " (not stable)"));
                // DirectInput に届くようにスキャンコード付きで送る
                keybd_event(VK_Y, SCAN_Y, 0, UIntPtr.Zero);
                Thread.Sleep(150);
                keybd_event(VK_Y, SCAN_Y, KEYEVENTF_KEYUP, UIntPtr.Zero);
                Thread.Sleep(500);
            }
            return 0;
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill();
            }
        }
    }

    static IntPtr WaitForWindow(Process process)
    {
        for (var i = 0; i < 100; i++)
        {
            Thread.Sleep(200);
            process.Refresh();
            if (process.MainWindowHandle != IntPtr.Zero)
            {
                return process.MainWindowHandle;
            }
        }
        return IntPtr.Zero;
    }

    static bool BringToFront(IntPtr hwnd)
    {
        for (var i = 0; i < 10; i++)
        {
            ShowWindow(hwnd, 9);
            // 他のプロセスのウィンドウを前面にするための制限を回避する（Alt キーを押して離す）
            keybd_event(VK_MENU, 0, 0, UIntPtr.Zero);
            keybd_event(VK_MENU, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            SetForegroundWindow(hwnd);
            Thread.Sleep(200);
            if (GetForegroundWindow() == hwnd)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 描画が落ち着く（連続した 2 回の撮影結果が一致する）まで待ってから保存します。
    /// 動画など常に変化するシーンでは、最大回数に達した時点の画像を保存します。
    /// </summary>
    static bool CaptureStable(IntPtr hwnd, string path)
    {
        byte[] previous = null;
        Bitmap bitmap = null;
        try
        {
            for (var i = 0; i < 20; i++)
            {
                bitmap?.Dispose();
                bitmap = Capture(hwnd);
                var current = ToBytes(bitmap);
                if (previous != null && IsSame(previous, current) && !IsUniform(current))
                {
                    bitmap.Save(path, System.Drawing.Imaging.ImageFormat.Png);
                    return true;
                }
                previous = current;
                Thread.Sleep(500);
            }
            bitmap.Save(path, System.Drawing.Imaging.ImageFormat.Png);
            return false;
        }
        finally
        {
            bitmap?.Dispose();
        }
    }

    static Bitmap Capture(IntPtr hwnd)
    {
        RECT rect;
        GetClientRect(hwnd, out rect);
        var point = new POINT();
        ClientToScreen(hwnd, ref point);
        var w = rect.Right - rect.Left;
        var h = rect.Bottom - rect.Top;
        var bitmap = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.CopyFromScreen(point.X, point.Y, 0, 0, new Size(w, h));
        }
        return bitmap;
    }

    static byte[] ToBytes(Bitmap bitmap)
    {
        var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        try
        {
            var bytes = new byte[data.Stride * data.Height];
            Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
            return bytes;
        }
        finally
        {
            bitmap.UnlockBits(data);
        }
    }

    static bool IsSame(byte[] a, byte[] b)
    {
        if (a.Length != b.Length) return false;
        for (var i = 0; i < a.Length; i++)
        {
            if (a[i] != b[i]) return false;
        }
        return true;
    }

    static bool IsUniform(byte[] a)
    {
        for (var i = 4; i < a.Length; i += 4)
        {
            if (a[i] != a[0] || a[i + 1] != a[1] || a[i + 2] != a[2]) return false;
        }
        return true;
    }
}
