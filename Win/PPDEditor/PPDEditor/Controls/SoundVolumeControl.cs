using System;
using System.Drawing;
using System.Windows.Forms;

namespace PPDEditor.Controls
{
    public partial class SoundVolumeControl : UserControl
    {
        public SoundVolumeControl()
        {
            InitializeComponent();
        }
        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            this.label1.Text = trackBar1.Value.ToString();
        }
        public int VolumePercent
        {
            get
            {
                return this.trackBar1.Value;
            }
            set
            {
                if (value < 0)
                {
                    this.trackBar1.Value = 0;
                }
                else if (value > 100)
                {
                    this.trackBar1.Value = 100;
                }
                else
                {
                    this.trackBar1.Value = value;
                }
                label1.Text = trackBar1.Value.ToString();
            }
        }
        public void SetBackColor(Color c)
        {
            this.label1.BackColor = c;
            this.trackBar1.BackColor = c;
        }
    }
}
