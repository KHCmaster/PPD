using PPDFramework;
using PPDMovie;
using System.Windows.Forms;

namespace PPD
{
    public class MovieForm : GameForm
    {
        private const int WM_GRAPHNOTIFY = 0x0400 + 13;

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_GRAPHNOTIFY:
                    {
                        MovieUtility.OnGraphNotify();
                        break;
                    }
            }
            base.WndProc(ref m);
        }
    }
}
