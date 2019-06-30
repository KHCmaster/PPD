using System;
using System.Windows.Forms;

namespace PPDEditor.Forms
{
    public partial class WriteDoneForm : Form
    {
        private Timer timer;
        private int tickCount;
        public WriteDoneForm()
        {
            InitializeComponent();

            timer = new Timer
            {
                Interval = 10
            };
            timer.Tick += timer_Tick;

            VisibleChanged += WriteDoneForm_VisibleChanged;
        }

        public void SetLang()
        {
            this.label1.Text = Utility.Language["WDFLabel1"];
        }

        void timer_Tick(object sender, EventArgs e)
        {
            tickCount++;

            double value = 1;
            if (tickCount > 40)
            {
                value = this.Opacity - 0.01f;
            }

            if (value < 0)
            {
                value = 0;
            }
            this.Opacity = value;
            if (this.Opacity <= 0)
            {
                timer.Stop();
                this.Hide();
            }
        }

        void WriteDoneForm_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
            {
                tickCount = 0;
                timer.Stop();
                timer.Start();
            }
        }
    }
}
