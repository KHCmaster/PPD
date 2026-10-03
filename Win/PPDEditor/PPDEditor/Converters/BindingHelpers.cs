using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PPDEditor.Converters
{
    /// <summary>
    /// true と false を入れ替えます（2 つのラジオボタンを 1 つの bool に結び付けるときなど）。
    /// </summary>
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b ? !b : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b ? !b : value;
        }
    }

    /// <summary>
    /// 列挙の値が ConverterParameter（値の名前）と同じかを bool にします（ラジオボタンを列挙に結び付けるときなど）。
    /// </summary>
    public class EnumToBooleanConverter : IValueConverter
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
    /// リソースの中から DataContext を使うための入れ物です（DataGrid の列やメニューの CompositeCollection などは DataContext を受け継がない）。
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
}
