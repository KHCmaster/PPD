using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace PPDFramework
{
    internal static class WinAPI
    {
        public const int IMN_CLOSESTATUSWINDOW = 0x1;
        public const int IMN_OPENSTATUSWINDOW = 0x2;
        public const int IMN_CHANGECANDIDATE = 0x3;
        public const int IMN_CLOSECANDIDATE = 0x4;
        public const int IMN_OPENCANDIDATE = 0x5;
        public const int IMN_SETCONVERSIONMODE = 0x6;
        public const int IMN_SETSENTENCEMODE = 0x7;
        public const int IMN_SETOPENSTATUS = 0x8;
        public const int IMN_SETCANDIDATEPOS = 0x9;
        public const int IMN_SETCOMPOSITIONFONT = 0xA;
        public const int IMN_SETCOMPOSITIONWINDOW = 0xB;
        public const int IMN_SETSTATUSWINDOWPOS = 0xC;
        public const int IMN_GUIDELINE = 0xD;
        public const int IMN_PRIVATE = 0xE;

        public const int NI_OPENCANDIDATE = 0x0010;
        public const int NI_CLOSECANDIDATE = 0x0011;
        public const int NI_SELECTCANDIDATESTR = 0x0012;
        public const int NI_CHANGECANDIDATELIST = 0x0013;
        public const int NI_FINALIZECONVERSIONRESULT = 0x0014;
        public const int NI_COMPOSITIONSTR = 0x0015;
        public const int NI_SETCANDIDATE_PAGESTART = 0x0016;
        public const int NI_SETCANDIDATE_PAGESIZE = 0x0017;
        public const int NI_IMEMENUSELECTED = 0x0018;


        public const int GCL_REVERSECONVERSION = 0x0002;

        public const int WM_IME_STARTCOMPOSITION = 0x010D;
        public const int WM_IME_ENDCOMPOSITION = 0x010E;
        public const int WM_IME_COMPOSITION = 0x010F;
        public const int WM_IME_SETCONTEXT = 0x0281;
        public const int WM_IME_NOTIFY = 0x0282;
        public const int WM_IME_CHAR = 0x0286;
        public const int WM_IME_REQUEST = 0x0288;
        public const int IMR_RECONVERTSTRING = 0x0004;

        public const Int32 GCS_COMPREADSTR = 0x0001;
        public const Int32 GCS_COMPSTR = 0x0008;
        public const Int32 GCS_RESULTSTR = 0x0800;
        public const Int32 SCS_SETSTR = (GCS_COMPREADSTR | GCS_COMPSTR);

        [StructLayout(LayoutKind.Sequential)]
        internal struct FIXED
        {
            public short fract;
            public short value;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MAT2
        {
            [MarshalAs(UnmanagedType.Struct)]
            public FIXED eM11;
            [MarshalAs(UnmanagedType.Struct)]
            public FIXED eM12;
            [MarshalAs(UnmanagedType.Struct)]
            public FIXED eM21;
            [MarshalAs(UnmanagedType.Struct)]
            public FIXED eM22;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public POINT(int x, int y) { this.x = x; this.y = y; }
            public POINT(Point pt) { x = pt.X; y = pt.Y; }
            public Int32 x, y;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct POINTFX
        {
            [MarshalAs(UnmanagedType.Struct)]
            public FIXED x;
            [MarshalAs(UnmanagedType.Struct)]
            public FIXED y;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct GLYPHMETRICS
        {
            public int gmBlackBoxX;
            public int gmBlackBoxY;
            [MarshalAs(UnmanagedType.Struct)]
            public POINT gmptGlyphOrigin;
            public short gmCellIncX;
            public short gmCellIncY;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct TEXTMETRIC
        {
            public Int32 height;
            public Int32 ascent;
            public Int32 descent;
            public Int32 internalLeading;
            public Int32 externalLeading;
            public Int32 aveCharWidth;
            public Int32 maxCharWidth;
            public Int32 weight;
            public Int32 overhang;
            public Int32 digitizedAspectX;
            public Int32 digitizedAspectY;
            public char firstChar;
            public char lastChar;
            public char defaultChar;
            public char breakChar;
            public byte italic;
            public byte underlined;
            public byte struckOut;
            public byte pitchAndFamily;
            public byte charSet;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct COMPOSITIONFORM
        {
            public UInt32 style;
            public POINT currentPos;
            public RECT area;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public RECT(Rectangle rect)
            {
                left = rect.Left;
                top = rect.Top;
                right = rect.Right;
                bottom = rect.Bottom;
            }
            public Int32 left, top, right, bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LOGFONTW
        {
            public Int32 height;
            public Int32 width;
            public Int32 escapement;
            public Int32 orientation;
            public Int32 weight;
            public byte italic;
            public byte underline;
            public byte strikeOut;
            public byte charSet;
            public byte outPrecision;
            public byte clipPrecision;
            public byte quality;
            public byte pitchAndFamily;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string faceName;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class CANDIDATELIST
        {
            public int dwSize;
            public int dwStyle;
            public int dwCount;
            public int dwSelection;
            public int dwPageStart;
            public int dwPageSize;
            public int dwOffset;
        }

        [DllImport("user32.dll")]
        internal static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern Int32 ReleaseDC(IntPtr hWnd, IntPtr dc);

        [DllImport("gdi32.dll")]
        internal static extern IntPtr SelectObject(IntPtr hdc, IntPtr gdiObj);

        [DllImport("gdi32.dll")]
        internal static extern uint GetGlyphOutlineW(IntPtr hdc, uint uChar, uint uFormat, out GLYPHMETRICS lpgm, uint cbBuffer, IntPtr lpvBuffer, ref MAT2 lpmat2);

        [DllImport("gdi32.dll")]
        internal static extern bool GetTextMetrics(IntPtr hdc, ref TEXTMETRIC lptm);

        [DllImport("gdi32.dll")]
        public static extern Int32 GetDeviceCaps(IntPtr dc, Int32 index);

        [DllImport("kernel32.dll")]
        internal static extern void FillMemory(IntPtr ptr, uint size, byte fill);

        [DllImport("kernel32.dll")]
        internal static extern void ZeroMemory(IntPtr ptr, uint size);

        [DllImport("gdi32.dll")]
        internal static extern int DeleteObject(IntPtr gdiObj);

        [DllImport("imm32.dll")]
        public static extern IntPtr ImmGetContext(IntPtr hWnd);

        [DllImport("imm32.dll")]
        public static extern Int32 ImmReleaseContext(IntPtr hWnd, IntPtr context);

        [DllImport("imm32.dll")]
        public static extern Int32 ImmGetCompositionStringW(IntPtr imContext, int index, byte[] out_string, int maxStringLen);

        [DllImport("imm32.dll")]
        public static extern Int32 ImmSetCompositionStringW(IntPtr imContext, UInt32 index, StringBuilder lpComp, UInt32 dwCompLen, StringBuilder lpRead, UInt32 readLen);

        [DllImport("imm32.dll")]
        static extern Int32 ImmSetCompositionWindow(IntPtr imContext, ref COMPOSITIONFORM compForm);

        [DllImport("imm32.dll")]
        static extern Int32 ImmGetCompositionWindow(IntPtr imContext, out COMPOSITIONFORM compForm);

        [DllImport("imm32.dll")]
        static extern Int32 ImmSetCompositionFontW(IntPtr imContext, ref LOGFONTW logFont);

        [DllImport("imm32.dll")]
        static extern Int32 ImmSetStatusWindowPos(IntPtr imContext, ref POINT point);

        [DllImport("imm32.dll")]
        public static extern UInt32 ImmGetProperty(IntPtr inputLocale, UInt32 index);

        [DllImport("imm32.dll")]
        static extern int ImmGetCandidateList(IntPtr imContext, int deIndex, IntPtr lpCandidateList, int dwBufLen);

        [DllImport("user32.dll")]
        static extern Int32 GetCaretPos(ref POINT outPos);

        [DllImport("user32.dll", EntryPoint = "SetCaretPos")]
        static extern Int32 SetCaretPos_(Int32 x, Int32 y);

        [DllImport("user32.dll", EntryPoint = "CreateCaret")]
        static extern Int32 CreateCaret_(IntPtr window, IntPtr hBitmap, Int32 width, Int32 height);

        [DllImport("user32.dll")]
        public static extern Int32 DestroyCaret();

        [DllImport("user32.dll")]
        public static extern Int32 ShowCaret(IntPtr window);

        [DllImport("user32.dll")]
        public static extern Int32 HideCaret(IntPtr window);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        public static void CreateCaret(IntPtr window, Size size)
        {
            CreateCaret_(window, IntPtr.Zero, size.Width, size.Height);
        }

        public static Point GetCaretPos()
        {
            var point = new POINT();
            var rc = GetCaretPos(ref point);
            if (rc == 0)
            {
                throw new Exception("failed to get caret location");
            }
            return new Point(point.x, point.y);
        }

        public static bool SetCaretPos(Point point)
        {
            var rc = SetCaretPos_(point.X, point.Y);
            return (rc != 0);
        }

        public static void SetImeWindowPos(IntPtr window, Point screenPos)
        {
            var imContext = ImmGetContext(window);
            SetImeWindowPos(imContext, window, screenPos);
            ImmReleaseContext(window, imContext);
        }

        /// <summary>Sets location of the IME composition window (pre-edit window) </summary>
        public static void SetImeWindowPos(IntPtr imContext, IntPtr window, Point screenPos)
        {
            const int CFS_POINT = 0x0002;
            var compForm = new COMPOSITIONFORM
            {
                style = CFS_POINT,
                currentPos = new POINT(screenPos),
                area = new RECT()
            };
            ImmSetCompositionWindow(imContext, ref compForm);
        }

        public static void SetImeWindowFont(IntPtr window, Font font)
        {
            IntPtr imContext;

            imContext = ImmGetContext(window);
            CreateLogFont(window, font, out LOGFONTW logicalFont);
            ImmSetCompositionFontW(imContext, ref logicalFont);
            ImmReleaseContext(window, imContext);
        }

        public static void GetImeWindowPos(IntPtr window)
        {
            var imContext = ImmGetContext(window);
            var compForm = new COMPOSITIONFORM();
            ImmGetCompositionWindow(imContext, out compForm);
            Console.WriteLine(compForm.area.left - compForm.area.right);
            ImmReleaseContext(window, imContext);
        }

        public static string[] GetImeCandidateList(IntPtr window)
        {
            var imContext = ImmGetContext(window);
            var list = new CANDIDATELIST();
            var dwSize = ImmGetCandidateList(imContext, 0, IntPtr.Zero, 0);
            if (dwSize > 0)
            {
                var BufList = Marshal.AllocHGlobal(dwSize);
                ImmGetCandidateList(imContext, 0, BufList, dwSize);
                Marshal.PtrToStructure(BufList, list);
                byte[] buf = new byte[dwSize];
                Marshal.Copy(BufList, buf, 0, dwSize);
                Marshal.FreeHGlobal(BufList);
                int os = list.dwOffset;
                var str = System.Text.Encoding.Default.GetString(buf, os, buf.Length - os);
                str = Regex.Replace(str, @"\0+$", "");
                var par = "\0".ToCharArray();
                ImmReleaseContext(window, imContext);
                return str.Split(par);
            }
            else
            {
                ImmReleaseContext(window, imContext);
                return new string[0];
            }
        }

        public static void CreateLogFont(IntPtr window, Font font, out LOGFONTW lf)
        {
            const int LOGPIXELSY = 90;
            lf = new LOGFONTW();
            IntPtr dc;
            int dpi_y;

            dc = GetDC(window);
            {
                dpi_y = GetDeviceCaps(dc, LOGPIXELSY);
            }
            ReleaseDC(window, dc);
            lf.escapement = 0;
            lf.orientation = 0;
            lf.height = -(int)(font.Size * dpi_y / 72);
            lf.weight = (font.Style & FontStyle.Bold) != 0 ? 700 : 400; // FW_BOLD or FW_NORMAL
            lf.italic = (byte)((font.Style & FontStyle.Italic) != 0 ? 1 : 0);
            lf.underline = 0;//(byte)( (font.Style & FontStyle.Underline) != 0 ? 1 : 0 );
            lf.strikeOut = 0;//(byte)( (font.Style & FontStyle.Strikeout) != 0 ? 1 : 0 );
            lf.charSet = 1; // DEFAULT_CHARSET
            lf.outPrecision = 0; // OUT_DEFAULT_PRECIS
            lf.clipPrecision = 0; // CLIP_DEFAULT_PRECIS
            lf.quality = 0; // DEFAULT_QUALITY
            lf.pitchAndFamily = 0; // DEFAULT_PITCH

            // set font name
            lf.faceName = font.Name;
        }


        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        [FlagsAttribute]
        public enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
            // Legacy flag, should not be used.
            // ES_USER_PRESENT = 0x00000004
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
    }
}
