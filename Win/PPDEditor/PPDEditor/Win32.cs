using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PPDEditor
{
    class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct TVITEM
        {
            public uint mask;
            public IntPtr hItem;
            public uint state;
            public uint stateMask;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string pszText;
            public int cchTextMax;
            public int iImage;
            public int iSelectedImage;
            public int cChildren;
            public uint lParam;
            public int iIntegral;
        }

        private const int TVIF_STATE = 0x0008;
        private const int TVIS_OVERLAYMASK = 0x0F00;
        private const int TVM_SETITEMW = 0x113F;
        private const int TVM_GETITEMW = 0x110C;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, ref TVITEM lParam);

        [DllImport("comctl32.dll")]
        public static extern int ImageList_SetOverlayImage(IntPtr himl, int iImage, int iOverlay);

        public static void SetTreeViewOverlay(TreeNode node, uint overlayIndex)
        {
            // TreeView_SetItemState(node.TreeView.Handle, node.Handle,
            //     overlayIndex << 8, TVIS_OVERLAYMASK); 相当の処理
            var tvi = new TVITEM
            {
                mask = NativeMethods.TVIF_STATE,
                hItem = node.Handle,
                stateMask = NativeMethods.TVIS_OVERLAYMASK,
                state = (overlayIndex << 8)
            };
            NativeMethods.SendMessage(node.TreeView.Handle, NativeMethods.TVM_SETITEMW, 0, ref tvi);
        }

        public static uint GetTreeViewOverlay(TreeNode node)
        {
            var tvi = new TVITEM
            {
                mask = NativeMethods.TVIF_STATE,
                hItem = node.Handle,
                stateMask = NativeMethods.TVIS_OVERLAYMASK,
                state = 0
            };
            _ = NativeMethods.SendMessage(node.TreeView.Handle, NativeMethods.TVM_GETITEMW, 0, ref tvi);
            return tvi.state >> 8;
        }

        [DllImport("winmm.dll")]
        internal static extern long timeGetTime();

        [DllImport("winmm.dll")]
        internal static extern void timeBeginPeriod(int x);

        [DllImport("winmm.dll")]
        internal static extern void timeEndPeriod(int x);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        internal static extern int GetMenuItemCount(IntPtr hMenu);

        [DllImport("user32.dll")]
        internal static extern bool DrawMenuBar(IntPtr hWnd);

        [DllImport("user32.dll")]
        internal static extern bool RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);

    }
}
