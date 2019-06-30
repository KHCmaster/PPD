using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ColorCanvas
{
    class CustomTextBox : TextBox
    {
        public string CustomText
        {
            get { return (string)GetValue(CustomTextProperty); }
            set { SetValue(CustomTextProperty, value); }
        }

        public static readonly DependencyProperty CustomTextProperty =
            DependencyProperty.Register("CustomText", typeof(string),
            typeof(TextBox),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                new PropertyChangedCallback(CustomTextChanged), new CoerceValueCallback(CoerceText),
                true, UpdateSourceTrigger.LostFocus));

        public CustomTextBox()
        {
            Width = 0;
            TextChanged += CustomTextBox_TextChanged;
        }

        void CustomTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CustomText = Text;
        }

        private void Update()
        {
            string text = CustomText ?? "";
            Width = text.Split('\n').Max(s => Utility.MeasureWidth(s + " ", FontFamily, FontSize)) +
                Padding.Left + Padding.Right + BorderThickness.Left + BorderThickness.Right;
            Text = text;
        }

        private static object CoerceText(DependencyObject d, object value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            return value;
        }

        private static void CustomTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (CustomTextBox)d;
            target.Update();
        }
    }
}
