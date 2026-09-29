using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

/// <summary>
/// 対象プロセスをデバッグ実行し、例外（初回例外を含む）を記録します。一定時間後に WM_CLOSE を送って終了させます。
/// 使い方: ExceptionWatch.exe <exe> <引数> <閉じるまでの秒数> <ログファイル> [確定キーのスキャンコード(16進)]
/// 確定キーを指定すると、WM_CLOSE 後も終了しない場合（ゲーム内の終了確認画面など）に、前面にしてそのキーを押します。
/// 終了コード: 0 = アクセス違反なし, 1 = アクセス違反あり, 2 = 終了しなかった（強制終了）
/// </summary>
static class ExceptionWatch
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct STARTUPINFO { public int cb; public string lpReserved, lpDesktop, lpTitle; public int dwX, dwY, dwXSize, dwYSize, dwXCountChars, dwYCountChars, dwFillAttribute, dwFlags; public short wShowWindow, cbReserved2; public IntPtr lpReserved2, hStdInput, hStdOutput, hStdError; }
    [StructLayout(LayoutKind.Sequential)]
    struct PROCESS_INFORMATION { public IntPtr hProcess, hThread; public int dwProcessId, dwThreadId; }

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern bool CreateProcess(string app, string cmd, IntPtr pa, IntPtr ta, bool inherit, uint flags, IntPtr env, string dir, ref STARTUPINFO si, out PROCESS_INFORMATION pi);
    [DllImport("kernel32.dll", SetLastError = true)] static extern bool WaitForDebugEvent(IntPtr evt, uint ms);
    [DllImport("kernel32.dll", SetLastError = true)] static extern bool ContinueDebugEvent(int pid, int tid, uint status);
    [DllImport("kernel32.dll", SetLastError = true)] static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr addr, byte[] buffer, int size, out IntPtr read);
    [DllImport("kernel32.dll", SetLastError = true)] static extern bool GetThreadContext(IntPtr hThread, IntPtr ctx);
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)] static extern int GetFinalPathNameByHandle(IntPtr h, StringBuilder sb, int len, int flags);
    [DllImport("user32.dll")] static extern bool PostMessage(IntPtr hwnd, uint msg, IntPtr w, IntPtr l);
    [DllImport("kernel32.dll")] static extern bool CloseHandle(IntPtr h);

    const uint DEBUG_ONLY_THIS_PROCESS = 2;
    const uint DBG_CONTINUE = 0x00010002;
    const uint DBG_EXCEPTION_NOT_HANDLED = 0x80010001;

    class Module { public string Name; public long Base; public long Size; }
    static List<Module> modules = new List<Module>();
    static IntPtr hProcess;
    static Dictionary<int, IntPtr> threads = new Dictionary<int, IntPtr>();
    static StreamWriter log;
    static Stopwatch clock = Stopwatch.StartNew();
    static byte confirmScanCode;

    [DllImport("user32.dll")] static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] static extern void keybd_event(byte vk, byte scan, uint flags, UIntPtr extra);
    [DllImport("user32.dll")] static extern uint MapVirtualKey(uint code, uint mapType);

    static IntPtr GetMainWindow(int pid)
    {
        try
        {
            return Process.GetProcessById(pid).MainWindowHandle;
        }
        catch
        {
            return IntPtr.Zero;
        }
    }

    static void SendClose(int pid)
    {
        var hwnd = GetMainWindow(pid);
        Log("---- sending WM_CLOSE to " + hwnd);
        PostMessage(hwnd, 0x10, IntPtr.Zero, IntPtr.Zero);
    }

    static void PressConfirm(int pid)
    {
        var hwnd = GetMainWindow(pid);
        if (hwnd == IntPtr.Zero)
        {
            return;
        }
        Log("---- pressing confirm key 0x" + confirmScanCode.ToString("X2"));
        keybd_event(0x12, 0, 0, UIntPtr.Zero);
        keybd_event(0x12, 0, 2, UIntPtr.Zero);
        SetForegroundWindow(hwnd);
        Thread.Sleep(300);
        // MAPVK_VSC_TO_VK = 1
        var vk = (byte)MapVirtualKey(confirmScanCode, 1);
        keybd_event(vk, confirmScanCode, 0, UIntPtr.Zero);
        Thread.Sleep(150);
        keybd_event(vk, confirmScanCode, 2, UIntPtr.Zero);
    }

    static int Main(string[] args)
    {
        var exe = args[0];
        var cmdArgs = args[1];
        var closeAfter = int.Parse(args[2]);
        log = new StreamWriter(args[3]) { AutoFlush = true };
        confirmScanCode = args.Length > 4 ? Convert.ToByte(args[4], 16) : (byte)0;
        var si = new STARTUPINFO { cb = Marshal.SizeOf(typeof(STARTUPINFO)) };
        PROCESS_INFORMATION pi;
        if (!CreateProcess(exe, "\"" + exe + "\" " + cmdArgs, IntPtr.Zero, IntPtr.Zero, false, DEBUG_ONLY_THIS_PROCESS, IntPtr.Zero, Path.GetDirectoryName(exe), ref si, out pi))
        {
            Console.WriteLine("CreateProcess failed " + Marshal.GetLastWin32Error());
            return 1;
        }
        var closeSent = false;
        var evt = Marshal.AllocHGlobal(512);
        var avCount = 0;
        long nextConfirm = 0;
        while (true)
        {
            if (!closeSent && clock.ElapsedMilliseconds > closeAfter * 1000)
            {
                closeSent = true;
                nextConfirm = clock.ElapsedMilliseconds + 3000;
                SendClose(pi.dwProcessId);
            }
            else if (closeSent && clock.ElapsedMilliseconds > (closeAfter + 60) * 1000L)
            {
                Log("---- timeout: process did not exit");
                Console.WriteLine("timeout: process did not exit");
                try { Process.GetProcessById(pi.dwProcessId).Kill(); } catch { }
                return 2;
            }
            else if (closeSent && confirmScanCode != 0 && clock.ElapsedMilliseconds > nextConfirm)
            {
                // 終了確認画面が出ている場合に備えて、前面にして確定キーを押す
                nextConfirm = clock.ElapsedMilliseconds + 3000;
                PressConfirm(pi.dwProcessId);
            }
            if (!WaitForDebugEvent(evt, 500)) continue;
            var code = Marshal.ReadInt32(evt, 0);
            var pid = Marshal.ReadInt32(evt, 4);
            var tid = Marshal.ReadInt32(evt, 8);
            var u = evt + 16;
            uint cont = DBG_CONTINUE;
            switch (code)
            {
                case 1: // EXCEPTION
                    var exCode = (uint)Marshal.ReadInt32(u, 0);
                    var exAddr = Marshal.ReadInt64(u, 16);
                    var firstChance = Marshal.ReadInt32(u, 152);
                    if (exCode != 0x80000003 && exCode != 0x406D1388 && exCode != 0xE0434352 && exCode != 0x4000001F)
                    {
                        var info1 = Marshal.ReadInt64(u, 40);
                        Log(string.Format("EXCEPTION 0x{0:X8} at {1} firstChance={2} thread={3} target=0x{4:X}", exCode, Resolve(exAddr), firstChance, tid, info1));
                        if (exCode == 0xC0000005 && avCount++ < 5)
                        {
                            DumpStack(tid);
                        }
                    }
                    else if (exCode == 0xE0434352)
                    {
                        Log(string.Format("CLR exception thread={0} firstChance={1}", tid, firstChance));
                    }
                    cont = exCode == 0x80000003 ? DBG_CONTINUE : DBG_EXCEPTION_NOT_HANDLED;
                    break;
                case 2: // CREATE_THREAD
                    threads[tid] = Marshal.ReadIntPtr(u, 0);
                    break;
                case 3: // CREATE_PROCESS
                    hProcess = Marshal.ReadIntPtr(u, 8);
                    threads[tid] = Marshal.ReadIntPtr(u, 16);
                    AddModule(Marshal.ReadIntPtr(u, 0), Marshal.ReadInt64(u, 24));
                    break;
                case 4: // EXIT_THREAD
                    break;
                case 5: // EXIT_PROCESS
                    Log("EXIT_PROCESS code=0x" + Marshal.ReadInt32(u, 0).ToString("X8") + " accessViolations=" + avCount);
                    ContinueDebugEvent(pid, tid, DBG_CONTINUE);
                    Console.WriteLine("access violations: " + avCount);
                    return avCount == 0 ? 0 : 1;
                case 6: // LOAD_DLL
                    AddModule(Marshal.ReadIntPtr(u, 0), Marshal.ReadInt64(u, 8));
                    break;
                case 7: // UNLOAD_DLL
                    var b = Marshal.ReadInt64(u, 0);
                    var m = modules.FirstOrDefault(x => x.Base == b);
                    if (m != null && (m.Name.IndexOf("sharpdx", StringComparison.OrdinalIgnoreCase) >= 0 || m.Name.IndexOf("d3d", StringComparison.OrdinalIgnoreCase) >= 0 || m.Name.IndexOf("clr", StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        Log("UNLOAD " + m.Name);
                    }
                    break;
            }
            ContinueDebugEvent(pid, tid, cont);
        }
    }

    static void Log(string s)
    {
        var line = string.Format("[{0,7:F2}s] {1}", clock.Elapsed.TotalSeconds, s);
        log.WriteLine(line);
    }

    static void AddModule(IntPtr hFile, long baseAddr)
    {
        var sb = new StringBuilder(1024);
        string name = "?";
        if (hFile != IntPtr.Zero && GetFinalPathNameByHandle(hFile, sb, sb.Capacity, 0) > 0)
        {
            name = Path.GetFileName(sb.ToString());
            CloseHandle(hFile);
        }
        long size = 0;
        var hdr = new byte[0x400];
        IntPtr read;
        if (ReadProcessMemory(hProcess, new IntPtr(baseAddr), hdr, hdr.Length, out read))
        {
            var lfanew = BitConverter.ToInt32(hdr, 0x3C);
            if (lfanew > 0 && lfanew + 0x54 < hdr.Length)
            {
                size = BitConverter.ToUInt32(hdr, lfanew + 0x50);
            }
        }
        modules.Add(new Module { Name = name, Base = baseAddr, Size = size });
    }

    static string Resolve(long addr)
    {
        foreach (var m in modules)
        {
            if (addr >= m.Base && addr < m.Base + m.Size)
            {
                return string.Format("{0}+0x{1:X}", m.Name, addr - m.Base);
            }
        }
        return string.Format("0x{0:X}", addr);
    }

    static void DumpStack(int tid)
    {
        IntPtr hThread;
        if (!threads.TryGetValue(tid, out hThread)) { Log("  no thread handle"); return; }
        var raw = Marshal.AllocHGlobal(1232 + 16);
        var ctx = new IntPtr((raw.ToInt64() + 15) & ~15L);
        for (var i = 0; i < 1232; i++) Marshal.WriteByte(ctx, i, 0);
        Marshal.WriteInt32(ctx, 0x30, 0x100003);
        if (!GetThreadContext(hThread, ctx)) { Log("  GetThreadContext failed " + Marshal.GetLastWin32Error()); return; }
        var rsp = Marshal.ReadInt64(ctx, 0x98);
        var rip = Marshal.ReadInt64(ctx, 0xF8);
        Log("  RIP " + Resolve(rip));
        var stack = new byte[0x8000];
        IntPtr read;
        ReadProcessMemory(hProcess, new IntPtr(rsp), stack, stack.Length, out read);
        var count = 0;
        for (var off = 0; off + 8 <= read.ToInt64() && count < 60; off += 8)
        {
            var v = BitConverter.ToInt64(stack, off);
            var r = Resolve(v);
            if (!r.StartsWith("0x"))
            {
                Log("  stack+0x" + off.ToString("X") + ": " + r);
                count++;
            }
        }
        Marshal.FreeHGlobal(raw);
    }
}
