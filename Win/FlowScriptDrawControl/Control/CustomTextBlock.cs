using System;
using System.Windows;
using System.Windows.Controls;

namespace FlowScriptDrawControl.Control
{
    class CustomTextBlock : TextBlock
    {
        public string CustomText
        {
            get { return (string)GetValue(CustomTextProperty); }
            set { SetValue(CustomTextProperty, value); }
        }

        public double CustomMaxWidth
        {
            get { return (double)GetValue(CustomMaxWidthProperty); }
            set { SetValue(CustomMaxWidthProperty, value); }
        }

        public bool IsCollapsed
        {
            get { return (bool)GetValue(IsCollapsedProperty); }
            set { SetValue(IsCollapsedProperty, value); }
        }

        public static readonly DependencyProperty CustomTextProperty =
            DependencyProperty.Register("CustomText", typeof(string), typeof(CustomTextBlock), new PropertyMetadata(CustomTextChanged));
        public static readonly DependencyProperty CustomMaxWidthProperty =
            DependencyProperty.Register("CustomMaxWidth", typeof(double), typeof(CustomTextBlock), new PropertyMetadata(double.PositiveInfinity, CustomMaxWidthChanged));
        public static readonly DependencyProperty IsCollapsedProperty =
            DependencyProperty.Register("IsCollapsed", typeof(bool), typeof(CustomTextBlock), new PropertyMetadata(IsCollapsedChanged));

        static CustomTextBlock()
        {
        }

        public CustomTextBlock()
        {
            Width = 0;
        }

        private void Update()
        {
            if (double.IsInfinity(CustomMaxWidth) || double.IsNaN(CustomMaxWidth) || CustomMaxWidth < 0 || !IsCollapsed)
            {
                Width = Utility.MeasureWidth(CustomText, FontFamily, FontSize);
                Text = CustomText;
            }
            else
            {
                var width = Utility.MeasureWidth(CustomText, FontFamily, FontSize);
                var text = CustomText;
                var iter = 0;
                var halfPos = text.Length / 2;
                Func<string> getText = () =>
                {
                    if (iter == 0)
                    {
                        return text;
                    }
                    return $"{text.Substring(0, halfPos - iter)}…{text.Substring(halfPos + iter)}";
                };
                while (width > CustomMaxWidth)
                {
                    iter++;
                    width = Utility.MeasureWidth(getText(), FontFamily, FontSize);
                }
                Width = width;
                Text = getText();
            }
        }

        private static void CustomTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (CustomTextBlock)d;
            target.Update();
        }

        private static void IsCollapsedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (CustomTextBlock)d;
            target.Update();
        }

        private static void CustomMaxWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (CustomTextBlock)d;
            target.Update();
        }
    }
}
