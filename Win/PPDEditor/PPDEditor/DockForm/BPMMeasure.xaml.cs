using PPDEditor.Forms;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace PPDEditor
{
    /// <summary>
    /// ボタンやキーを拍に合わせて押して、BPM を測ります。
    /// </summary>
    public partial class BPMMeasure : DockView
    {
        readonly List<long> data;
        public BPMMeasure()
        {
            InitializeComponent();
            data = new List<long>(10);
        }
        public void SetLang()
        {
            this.label1.Text = Utility.Language["BMLabel1"];
            this.label2.Text = Utility.Language["BMLabel2"];
            this.button1.Content = Utility.Language["BMButton1"];
            this.button2.Content = Utility.Language["BMButton2"];
        }
        public void SetSkin()
        {
            var brush = WpfSkin.TransparentTextBrush;
            this.label1.Foreground = brush;
            this.label2.Foreground = brush;
            this.label3.Foreground = brush;
            this.label4.Foreground = brush;
        }
        private void UpdateText()
        {
            if (data.Count <= 1) return;
            long tempdata = data[^1] - data[0];
            this.label3.Text = (data[^1] - data[^2]).ToString();
            this.label4.Text = (60f / (tempdata / 1000f) * data.Count).ToString();
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            data.Clear();
            this.label3.Text = "0";
            this.label4.Text = "0";
        }

        private void Button1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            data.Add(NativeMethods.timeGetTime());
            UpdateText();
        }

        bool down;
        private void BPMMeasure_KeyDown(object sender, KeyEventArgs e)
        {
            if (!down)
            {
                down = true;
                if (e.Key == Key.Return || e.Key == Key.Space)
                {
                    e.Handled = true;
                    data.Add(NativeMethods.timeGetTime());
                    UpdateText();
                }
            }
            else if (e.Key == Key.Return || e.Key == Key.Space)
            {
                // 押しっぱなしの繰り返しでボタンが押されないようにする
                e.Handled = true;
            }
        }

        private void BPMMeasure_KeyUp(object sender, KeyEventArgs e)
        {
            down = false;
        }
    }
}
