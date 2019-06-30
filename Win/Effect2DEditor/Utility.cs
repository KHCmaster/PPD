using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Effect2DEditor
{
    class Utility
    {
        public static void ShowOrHideWindow(DockPanel dockPanel, DockContent dockcontent, Keys modifierKeys)
        {
            if (!dockcontent.Visible || (DockState.DockTopAutoHide <= dockcontent.DockState && dockcontent.DockState <= DockState.DockRightAutoHide))
            {
                if (dockcontent.Pane != null)
                {
                    dockcontent.Show(dockPanel);
                    dockcontent.Pane.ActiveContent = dockcontent;
                }
                else
                {
                    dockcontent.Show(dockPanel, WeifenLuo.WinFormsUI.Docking.DockState.Float);
                }
            }
            else
            {
                if (dockPanel.ActivePane == dockcontent.Pane || dockcontent.DockState == DockState.Float)
                {
                    dockcontent.Hide();
                }
                else
                {
                    dockcontent.Focus();
                }
            }
            if (dockcontent.DockState == DockState.Unknown)
            {
                dockcontent.Show(dockPanel, DockState.Document);
                dockcontent.Pane.ActiveContent = dockcontent;
            }
            if ((modifierKeys & Keys.Shift) == Keys.Shift)
            {
                dockcontent.DockPanel = null;
                dockcontent.Show(dockPanel, DockState.Float);
                dockcontent.Pane.ActiveContent = dockcontent;
            }
        }
    }
}
