using System.Windows;
using System.Windows.Controls;

namespace ColorCanvas
{
    class CustomTextBlock : TextBlock
    {
        public string CustomText
        {
            get { return (string)GetValue(CustomTextProperty); }
            set { SetValue(CustomTextProperty, value); }
        }

        public static readonly DependencyProperty CustomTextProperty =
            DependencyProperty.Register("CustomText", typeof(string), typeof(CustomTextBlock), new PropertyMetadata(CustomTextChanged));

        public CustomTextBlock()
        {
            Width = 0;
        }

        private void Update()
        {
            Width = Utility.MeasureWidth(CustomText, FontFamily, FontSize);
            Text = CustomText;
        }

        private static void CustomTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (CustomTextBlock)d;
            target.Update();
        }
    }
}
