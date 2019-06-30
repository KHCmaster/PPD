using FlowScriptControl.Classes;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace FlowScriptControl.Controls
{
    class CustomToolStripMenuItem : ToolStripMenuItem
    {
        public FlowSourceDumper Dumper
        {
            get;
            private set;
        }

        public Classes.IToolTipText CustomToolTipText
        {
            get;
            private set;
        }

        public bool IsFolder
        {
            get
            {
                return Dumper == null;
            }
        }

        private Timer timer;
        private ToolTip toolTip;
        private FlowDrawPanel panel;

        public CustomToolStripMenuItem(Classes.IToolTipText toolTipText, FlowDrawPanel panel, ToolTip tooltip, string text)
            : base(text)
        {
            this.panel = panel;
            this.toolTip = tooltip;
            CustomToolTipText = toolTipText;
            Initialize();
        }

        public CustomToolStripMenuItem(FlowSourceDumper dumper, FlowDrawPanel panel, ToolTip tooltip, string text)
            : base(text)
        {
            this.panel = panel;
            this.toolTip = tooltip;
            CustomToolTipText = dumper;
            Dumper = dumper;
            Initialize();
        }

        private void Initialize()
        {
            timer = new Timer
            {
                Interval = 1000
            };
            timer.Tick += timer_Tick;
            MouseEnter += CustomToolStripMenuItem_MouseEnter;
            MouseLeave += CustomToolStripMenuItem_MouseLeave;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            var form = panel.FindForm();
            if (form != null)
            {
                toolTip.Hide(form);
                toolTip.Show(CustomToolTipText.ToolTipText, form, form.PointToClient(new Point(Cursor.Position.X + 15, Cursor.Position.Y + 15)));
            }
        }

        void CustomToolStripMenuItem_MouseLeave(object sender, EventArgs e)
        {
            timer.Stop();
            var form = panel.FindForm();
            if (form != null)
            {
                toolTip.Hide(form);
            }
        }

        void CustomToolStripMenuItem_MouseEnter(object sender, EventArgs e)
        {
            timer.Start();
        }


    }
}
