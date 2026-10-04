using PPDEditor.Forms;
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
    /// フラグの列挙の値が、ConverterParameter（値の名前）のフラグを持つかを bool にします（難易度のメニューを使えるかなど）。
    /// </summary>
    public class HasFlagConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Enum flags && flags.HasFlag((Enum)Enum.Parse(flags.GetType(), (string)parameter));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// System.Drawing.Color をブラシにします（設定ファイルやスキンが System.Drawing.Color で色を持っているため）。
    /// </summary>
    public class DrawingColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is System.Drawing.Color color ? WpfSkin.ToBrush(color) : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// 列挙の値と、その番号（コンボボックスの SelectedIndex など）を変換します。
    /// </summary>
    public class EnumIndexConverter : IValueConverter
    {
        public static EnumIndexConverter Instance { get; } = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Enum ? System.Convert.ToInt32(value, CultureInfo.InvariantCulture) : -1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is int index && index >= 0 ? Enum.ToObject(targetType, index) : Binding.DoNothing;
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
