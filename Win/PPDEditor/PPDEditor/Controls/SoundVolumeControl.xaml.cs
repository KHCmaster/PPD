using System;
using System.Windows.Controls;

namespace PPDEditor.Controls
{
    /// <summary>
    /// 音量（0 から 100）を選びます。
    /// </summary>
    public partial class SoundVolumeControl : UserControl
    {
        public SoundVolumeControl()
        {
            InitializeComponent();
        }
        public int VolumePercent
        {
            get
            {
                return (int)Math.Round(this.trackBar1.Value);
            }
            set
            {
                this.trackBar1.Value = Math.Clamp(value, 0, 100);
            }
        }
    }
}
