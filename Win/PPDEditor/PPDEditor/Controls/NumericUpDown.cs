using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace PPDEditor.Controls
{
    /// <summary>
    /// 数値を入力する欄です（WinForms の NumericUpDown の代わり）。上下のボタン、矢印キー、ホイールで増減します。
    /// </summary>
    public class NumericUpDown : UserControl
    {
        /// <summary>
        /// 値です（結び付けられるように依存関係プロパティにしてある）。最小値と最大値の間に収め、小数点以下の桁数で丸めます。
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value), typeof(decimal), typeof(NumericUpDown),
            new FrameworkPropertyMetadata(0m, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValuePropertyChanged, CoerceValueProperty));

        readonly TextBox textBox;
        decimal minimum;
        decimal maximum = 100;

        /// <summary>
        /// 値が変わったときに発生します。
        /// </summary>
        public event EventHandler ValueChanged;

        public NumericUpDown()
        {
            textBox = new TextBox
            {
                VerticalContentAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Right,
                BorderThickness = new Thickness(0)
            };
            textBox.LostKeyboardFocus += (sender, e) => CommitText();
            textBox.PreviewKeyDown += TextBox_PreviewKeyDown;
            textBox.MouseWheel += (sender, e) =>
            {
                Step(e.Delta > 0 ? 1 : -1);
                e.Handled = true;
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(14) });
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            Grid.SetRowSpan(textBox, 2);
            grid.Children.Add(textBox);
            var up = CreateButton("M 0 3 L 3 0 L 6 3 Z", 1);
            Grid.SetColumn(up, 1);
            grid.Children.Add(up);
            var down = CreateButton("M 0 0 L 6 0 L 3 3 Z", -1);
            Grid.SetColumn(down, 1);
            Grid.SetRow(down, 1);
            grid.Children.Add(down);

            Content = new Border
            {
                BorderBrush = SystemColors.ControlDarkBrush,
                BorderThickness = new Thickness(1),
                Child = grid
            };
            Height = 20;
            UpdateText();
        }

        private RepeatButton CreateButton(string path, int direction)
        {
            var button = new RepeatButton
            {
                Content = new System.Windows.Shapes.Path { Data = Geometry.Parse(path), Fill = Brushes.Black },
                Padding = new Thickness(0),
                Focusable = false
            };
            button.Click += (sender, e) => Step(direction);
            return button;
        }

        /// <summary>
        /// 値です。最小値と最大値の間に収めます。
        /// </summary>
        public decimal Value
        {
            get { return (decimal)GetValue(ValueProperty); }
            set
            {
                SetValue(ValueProperty, value);
                // 値が変わらなくても、入力途中の文字を今の値に戻す
                UpdateText();
            }
        }

        private static object CoerceValueProperty(DependencyObject d, object baseValue)
        {
            var control = (NumericUpDown)d;
            return Math.Clamp(Math.Round((decimal)baseValue, control.DecimalPlaces), control.minimum, control.maximum);
        }

        private static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (NumericUpDown)d;
            control.UpdateText();
            control.ValueChanged?.Invoke(control, EventArgs.Empty);
        }

        public decimal Minimum
        {
            get { return minimum; }
            set
            {
                minimum = value;
                if (maximum < minimum) maximum = minimum;
                CoerceValue(ValueProperty);
            }
        }

        public decimal Maximum
        {
            get { return maximum; }
            set
            {
                maximum = value;
                if (minimum > maximum) minimum = maximum;
                CoerceValue(ValueProperty);
            }
        }

        /// <summary>
        /// ボタンや矢印キーで増減する量です。
        /// </summary>
        public decimal Increment
        {
            get;
            set;
        } = 1;

        /// <summary>
        /// 小数点以下の桁数です。
        /// </summary>
        public int DecimalPlaces
        {
            get;
            set;
        }

        private void Step(int direction)
        {
            CommitText();
            Value += Increment * direction;
        }

        private void UpdateText()
        {
            textBox.Text = Value.ToString("F" + DecimalPlaces, CultureInfo.CurrentCulture);
        }

        private void CommitText()
        {
            if (decimal.TryParse(textBox.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out var parsed))
            {
                Value = parsed;
            }
            else
            {
                UpdateText();
            }
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    Step(1);
                    e.Handled = true;
                    break;
                case Key.Down:
                    Step(-1);
                    e.Handled = true;
                    break;
                case Key.Return:
                    CommitText();
                    break;
            }
        }
    }
}
