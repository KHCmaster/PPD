using System;
using System.Windows;
using System.Windows.Threading;

namespace PPDEditor.Forms
{
    public partial class WriteDoneWindow : Window
    {
        private readonly DispatcherTimer timer;
        private int tickCount;
        public WriteDoneWindow()
        {
            InitializeComponent();

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(10)
            };
            timer.Tick += Timer_Tick;

            IsVisibleChanged += WriteDoneWindow_IsVisibleChanged;
        }

        public void SetLang()
        {
            this.label1.Text = Utility.Language["WDFLabel1"];
        }

        void Timer_Tick(object sender, EventArgs e)
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

        void WriteDoneWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                tickCount = 0;
                Opacity = 1;
                timer.Stop();
                timer.Start();
            }
        }
    }
}
