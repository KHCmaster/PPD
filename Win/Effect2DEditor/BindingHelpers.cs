using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Effect2DEditor
{
    /// <summary>
    /// リソースの中から DataContext を使うための入れ物です（ポップアップに出るメニューの項目などは、ウィンドウの DataContext をたどれない）。
    /// </summary>
    public class BindingProxy : Freezable
    {
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(nameof(Data), typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));

        public object Data
        {
            get { return GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }
    }

    /// <summary>
    /// 列挙の値が ConverterParameter（値の名前）と同じかを bool にします（選んでいるメニューの項目のチェック、ラジオボタンなど）。
    /// </summary>
    public class EnumEqualsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && value.ToString() == parameter as string;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // チェックが外れたときは値を変えない（ほかのラジオボタンがチェックされたときに変わる）
            return value is true ? Enum.Parse(targetType, (string)parameter) : Binding.DoNothing;
        }
    }

    /// <summary>
    /// System.Drawing.Color をブラシにします（設定が System.Drawing.Color で色を持っているため）。
    /// </summary>
    public class DrawingColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is System.Drawing.Color color ? new SolidColorBrush(Color.FromRgb(color.R, color.G, color.B)) : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
