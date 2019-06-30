using System.Drawing;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking
{
    internal static class DockHelper
    {
        public static bool IsDockStateAutoHide(DockState dockState)
        {
            if (dockState == DockState.DockLeftAutoHide ||
                dockState == DockState.DockRightAutoHide ||
                dockState == DockState.DockTopAutoHide ||
                dockState == DockState.DockBottomAutoHide)
                return true;
            else
                return false;
        }

        public static bool IsDockStateValid(DockState dockState, DockAreas dockableAreas)
        {
            if (((dockableAreas & DockAreas.Float) == 0) &&
                (dockState == DockState.Float))
                return false;
            else if (((dockableAreas & DockAreas.Document) == 0) &&
                (dockState == DockState.Document))
                return false;
            else if (((dockableAreas & DockAreas.DockLeft) == 0) &&
                (dockState == DockState.DockLeft || dockState == DockState.DockLeftAutoHide))
                return false;
            else if (((dockableAreas & DockAreas.DockRight) == 0) &&
                (dockState == DockState.DockRight || dockState == DockState.DockRightAutoHide))
                return false;
            else if (((dockableAreas & DockAreas.DockTop) == 0) &&
                (dockState == DockState.DockTop || dockState == DockState.DockTopAutoHide))
                return false;
            else if (((dockableAreas & DockAreas.DockBottom) == 0) &&
                (dockState == DockState.DockBottom || dockState == DockState.DockBottomAutoHide))
                return false;
            else
                return true;
        }

        public static bool IsDockWindowState(DockState state)
        {
            if (state == DockState.DockTop || state == DockState.DockBottom || state == DockState.DockLeft ||
                state == DockState.DockRight || state == DockState.Document)
                return true;
            else
                return false;
        }

        public static DockState ToggleAutoHideState(DockState state)
        {
            switch (state)
            {
                case DockState.DockLeft:
                    return DockState.DockLeftAutoHide;
                case DockState.DockRight:
                    return DockState.DockRightAutoHide;
                case DockState.DockTop:
                    return DockState.DockTopAutoHide;
                case DockState.DockBottom:
                    return DockState.DockBottomAutoHide;
                case DockState.DockLeftAutoHide:
                    return DockState.DockLeft;
                case DockState.DockRightAutoHide:
                    return DockState.DockRight;
                case DockState.DockTopAutoHide:
                    return DockState.DockTop;
                case DockState.DockBottomAutoHide:
                    return DockState.DockBottom;
                default:
                    return state;
            }
        }

        public static DockPane PaneAtPoint(Point pt, DockPanel dockPanel)
        {
            if (!Win32Helper.IsRunningOnMono)
                for (Control control = Win32Helper.ControlAtPoint(pt); control != null; control = control.Parent)
                {
                    if (control is IDockContent content && content.DockHandler.DockPanel == dockPanel)
                        return content.DockHandler.Pane;

                    if (control is DockPane pane && pane.DockPanel == dockPanel)
                        return pane;
                }

            return null;
        }

        public static FloatWindow FloatWindowAtPoint(Point pt, DockPanel dockPanel)
        {
            if (!Win32Helper.IsRunningOnMono)
                for (Control control = Win32Helper.ControlAtPoint(pt); control != null; control = control.Parent)
                {
                    if (control is FloatWindow floatWindow && floatWindow.DockPanel == dockPanel)
                        return floatWindow;
                }

            return null;
        }
    }
}
