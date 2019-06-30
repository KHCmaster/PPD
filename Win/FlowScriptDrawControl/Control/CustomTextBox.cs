using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace FlowScriptDrawControl.Control
{
    class CustomTextBox : TextBox
    {
        public string CustomText
        {
            get { return (string)GetValue(CustomTextProperty); }
            set { SetValue(CustomTextProperty, value); }
        }

        public static readonly DependencyProperty CustomTextProperty =
            DependencyProperty.Register("CustomText", typeof(string), typeof(CustomTextBox), new PropertyMetadata(CustomTextChanged));

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
            Width = text.Split('\n').Max(s => Utility.MeasureWidth(s + " ", FontFamily, FontSize));
            Text = text;
        }

        private static void CustomTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (CustomTextBox)d;
            target.Update();
        }
    }
}
