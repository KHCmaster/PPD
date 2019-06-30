using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace FlowScriptDrawControl
{
    class Utility
    {
        static string[] colors = {
            "#E60012",            "#F39800",            "#FFF100",            "#8FC31F",            "#009944",            "#009E96",            "#00A0E9",            "#0068B7",            "#1D2088",            "#920783",            "#E4007F",            "#E5004F"        };

        internal static bool IsLoading
        {
            get;
            set;
        }

        public static Color GetRandomColor()
        {
            var rand = new Random();
            return (Color)ColorConverter.ConvertFromString(colors[rand.Next(0, colors.Length)]);
        }

        public static T GetParent<T>(DependencyObject current)
            where T : class
        {
            var parent = VisualTreeHelper.GetParent(current);
            while (parent != null && parent.GetType() != typeof(T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as T;
        }

        public static double MeasureWidth(string text, FontFamily fontFamily, double fontSize)
        {
            return MeasureSize(text, fontFamily, fontSize).Width;
        }

        public static Size MeasureSize(string text, FontFamily fontFamily, double fontSize)
        {
            if (text == null)
            {
                text = "";
            }

            System.Windows.FontStyle fontStyle = FontStyles.Normal;
            FontWeight fontWeight = FontWeights.Medium;
            var formattedText = new FormattedText(
                text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(
                    fontFamily,
                    fontStyle,
                    fontWeight,
                    FontStretches.Normal),
                    fontSize,
                    System.Windows.Media.Brushes.Black
                );
            var _textGeometry = formattedText.BuildGeometry(new System.Windows.Point(0, 0));
            var _textHighLightGeometry = formattedText.BuildHighlightGeometry(new System.Windows.Point(0, 0));
            return _textHighLightGeometry == null ? new Size(0, 0) : _textHighLightGeometry.Bounds.Size;
        }
    }
}
