using System;
using System.Windows;
using System.Windows.Controls;

namespace PPDEditor.Controls
{
    /// <summary>
    /// 音量（0 から 100）を選びます。
    /// </summary>
    public partial class SoundVolumeControl : UserControl
    {
        public static readonly DependencyProperty VolumePercentProperty = DependencyProperty.Register(
            nameof(VolumePercent), typeof(int), typeof(SoundVolumeControl),
            new FrameworkPropertyMetadata(100, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, null, CoerceVolumePercent));

        public SoundVolumeControl()
        {
            InitializeComponent();
        }

        public int VolumePercent
        {
            get { return (int)GetValue(VolumePercentProperty); }
            set { SetValue(VolumePercentProperty, value); }
        }

        private static object CoerceVolumePercent(DependencyObject d, object value)
        {
            return Math.Clamp((int)value, 0, 100);
        }
    }
}
