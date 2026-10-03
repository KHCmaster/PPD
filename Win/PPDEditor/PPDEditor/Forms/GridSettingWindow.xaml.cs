using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Color = System.Drawing.Color;

namespace PPDEditor.Forms
{
    /// <summary>
    /// グリッドの設定です（以前は表の中で編集していた）。
    /// </summary>
    public partial class GridSettingWindow : Window
    {
        Color gridColor = Color.White;

        public GridSettingWindow()
        {
            InitializeComponent();
            widthBox.Text = "5";
            heightBox.Text = "5";
            offsetXBox.Text = "0";
            offsetYBox.Text = "0";
            UpdateColor();
        }

        public void SetLang()
        {
            widthLabel.Text = Utility.Language["GSFText1"];
            heightLabel.Text = Utility.Language["GSFText2"];
            offsetXLabel.Text = Utility.Language["GSFText3"];
            offsetYLabel.Text = Utility.Language["GSFText4"];
            colorLabel.Text = Utility.Language["GSFText5"];
        }

        public SquareGrid Grid
        {
            get
            {
                return new SquareGrid
                {
                    Width = GetValue(widthBox, 5, x => x < 1 || x > 800 ? 5 : x),
                    Height = GetValue(heightBox, 5, x => x < 1 || x > 400 ? 5 : x),
                    OffsetX = GetValue(offsetXBox, 0, x => x),
                    OffsetY = GetValue(offsetYBox, 0, x => x),
                    GridColor = gridColor
                };
            }
            set
            {
                widthBox.Text = value.Width.ToString();
                heightBox.Text = value.Height.ToString();
                offsetXBox.Text = value.OffsetX.ToString();
                offsetYBox.Text = value.OffsetY.ToString();
                gridColor = value.GridColor;
                UpdateColor();
            }
        }

        private void UpdateColor()
        {
            colorPreview.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(gridColor.R, gridColor.G, gridColor.B));
            colorText.Text = "#" + gridColor.R.ToString("X2") + gridColor.G.ToString("X2") + gridColor.B.ToString("X2");
            // 背景の色に合わせて、読める色にする
            colorText.Foreground = gridColor.GetBrightness() > 0.5f ? Brushes.Black : Brushes.White;
        }

        private static int GetValue(TextBox textBox, int errorvalue, Func<int, int> func)
        {
            if (!int.TryParse(textBox.Text, out int ret)) ret = errorvalue;
            return func(ret);
        }

        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FlowScriptDrawControl.Dialog.ColorDialog
            {
                Owner = this,
                CurrentColor = System.Windows.Media.Color.FromRgb(gridColor.R, gridColor.G, gridColor.B)
            };
            if (dialog.ShowDialog() == true)
            {
                gridColor = Color.FromArgb(dialog.CurrentColor.R, dialog.CurrentColor.G, dialog.CurrentColor.B);
                UpdateColor();
            }
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
