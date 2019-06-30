using System;
using System.Drawing;
using System.Windows.Forms;

namespace PPDEditor.Controls
{
    public class ChangeValueLabel : Label
    {
        public event ValueChangeEventHandler ValueChange;
        Timer timer;
        Point lastpos;
        public ChangeValueLabel()
        {
            timer = new Timer
            {
                Interval = 20
            };
            timer.Tick += timer_Tick;
            this.Cursor = System.Windows.Forms.Cursors.SizeWE;
            this.MouseDown += ChangeValueLabel_MouseDown;
            this.MouseUp += ChangeValueLabel_MouseUp;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            Point currentpos = Cursor.Position;
            if (lastpos.X != currentpos.X)
            {
                if (ValueChange != null)
                {
                    var ee = new ValueChangeEventArgs
                    {
                        ChangedValue = currentpos.X - lastpos.X
                    };
                    ValueChange.Invoke(this, ee);
                }
                lastpos = currentpos;
            }
        }

        void ChangeValueLabel_MouseUp(object sender, MouseEventArgs e)
        {
            timer.Stop();
        }

        void ChangeValueLabel_MouseDown(object sender, MouseEventArgs e)
        {
            lastpos = Cursor.Position;
            timer.Start();
        }

    }
    public class ValueChangeEventArgs : EventArgs
    {
        public int ChangedValue
        {
            get;
            set;
        }
    }
    public delegate void ValueChangeEventHandler(object sender, ValueChangeEventArgs e);
}
