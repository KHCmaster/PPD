using System.Drawing;
using WeifenLuo.WinFormsUI.Docking;

namespace PPDEditor
{
    public class CustomFloatWindow : FloatWindow
    {
        public CustomFloatWindow(DockPanel dockPanel, DockPane pane)
            : base(dockPanel, pane)
        {
            DoubleClickTitleBarToDock = false;
        }

        public CustomFloatWindow(DockPanel dockPanel, DockPane pane, Rectangle bounds)
            : base(dockPanel, pane, bounds)
        {
            DoubleClickTitleBarToDock = false;
        }
    }
}
