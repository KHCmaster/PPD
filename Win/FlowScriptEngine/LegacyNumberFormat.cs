using System;
using System.Globalization;

namespace FlowScriptEngine
{
    /// <summary>
    /// float / double を .NET Framework と同じ書式で文字列にします（スクリプトの互換のため）。
    /// .NET Core 3.0 以降は、書式を指定しない ToString が「元の値に戻せる最短の表記」になり、
    /// 例えば 1f / 3 が "0.3333333" ではなく "0.33333334"、0.1 + 0.2 が "0.3" ではなく "0.30000000000000004"、-0 が "0" ではなく "-0" になる。
    /// .NET Framework は float を有効数字 7 桁（G7）、double を 15 桁（G15）で書式化し、-0 を "0" と表示していた。
    /// </summary>
    public static class LegacyNumberFormat
    {
        /// <summary>
        /// string.Format などで使う書式プロバイダーです。書式指定のない float / double を .NET Framework と同じ書式にします。
        /// </summary>
        public static IFormatProvider CurrentCulture
        {
            get { return new Provider(CultureInfo.CurrentCulture); }
        }

        /// <summary>
        /// 指定したカルチャーをもとにした書式プロバイダーを取得します。
        /// </summary>
        /// <param name="culture"></param>
        /// <returns></returns>
        public static IFormatProvider Create(IFormatProvider culture)
        {
            return new Provider(culture ?? CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// float を解析します。.NET Framework と同じく、"-0" は 0 として解析します
        /// （.NET Core 3.0 以降の float.TryParse は -0 を返すため、Atan2 などの結果が変わる）。
        /// </summary>
        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out float result)
        {
            if (!float.TryParse(s, style, provider, out result))
            {
                return false;
            }
            if (result == 0)
            {
                result = 0f;
            }
            return true;
        }

        /// <summary>
        /// double を解析します。.NET Framework と同じく、"-0" は 0 として解析します。
        /// </summary>
        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out double result)
        {
            if (!double.TryParse(s, style, provider, out result))
            {
                return false;
            }
            if (result == 0)
            {
                result = 0d;
            }
            return true;
        }

        public static string ToString(float value)
        {
            return ToString(value, CultureInfo.CurrentCulture);
        }

        public static string ToString(float value, IFormatProvider provider)
        {
            // -0 は 0 として表示する（.NET Framework と同じ）
            if (value == 0)
            {
                return 0f.ToString("G7", provider);
            }
            // ちょうど真ん中の値は、.NET Framework と同じく 0 から遠い方に丸める（.NET Core は偶数の方に丸める）
            if (float.IsFinite(value) && IsTie(value.ToString("R", CultureInfo.InvariantCulture), () => value.ToString("E120", CultureInfo.InvariantCulture), 7))
            {
                value = value > 0 ? MathF.BitIncrement(value) : MathF.BitDecrement(value);
            }
            return value.ToString("G7", provider);
        }

        public static string ToString(double value)
        {
            return ToString(value, CultureInfo.CurrentCulture);
        }

        public static string ToString(double value, IFormatProvider provider)
        {
            if (value == 0)
            {
                return 0d.ToString("G15", provider);
            }
            if (double.IsFinite(value) && IsTie(value.ToString("R", CultureInfo.InvariantCulture), () => value.ToString("E800", CultureInfo.InvariantCulture), 15))
            {
                value = value > 0 ? Math.BitIncrement(value) : Math.BitDecrement(value);
            }
            return value.ToString("G15", provider);
        }

        /// <summary>
        /// 有効数字 <paramref name="precision"/> 桁に丸めるときに、ちょうど真ん中（次の桁が 5 で、その後がすべて 0）かを調べます。
        /// </summary>
        /// <param name="shortest">値を表す一番短い文字列（"R"）。真ん中の値はこれが precision + 1 桁で 5 で終わるので、まずこれで調べる。</param>
        /// <param name="exact">値のすべての桁を表す文字列（"E" で桁を多く指定したもの）。</param>
        private static bool IsTie(string shortest, Func<string> exact, int precision)
        {
            var digits = SignificantDigits(shortest);
            if (digits.Length != precision + 1 || digits[^1] != '5')
            {
                return false;
            }
            var exactDigits = SignificantDigits(exact());
            return exactDigits.Length > precision && exactDigits[precision] == '5' && exactDigits.AsSpan(precision + 1).TrimEnd('0').IsEmpty;
        }

        // 符号、小数点、指数と、前後の余計な 0 を除いた数字
        private static string SignificantDigits(string text)
        {
            var exponent = text.IndexOfAny(['E', 'e']);
            if (exponent >= 0)
            {
                text = text[..exponent];
            }
            var digits = new System.Text.StringBuilder(text.Length);
            foreach (var c in text)
            {
                if (c >= '0' && c <= '9')
                {
                    digits.Append(c);
                }
            }
            return digits.ToString().TrimStart('0').TrimEnd('0');
        }

        /// <summary>
        /// 値を文字列にします。float / double は .NET Framework と同じ書式にし、それ以外は ToString を使います。
        /// </summary>
        public static string ToString(object value)
        {
            return value switch
            {
                null => null,
                float f => ToString(f),
                double d => ToString(d),
                _ => value.ToString(),
            };
        }

        private class Provider(IFormatProvider inner) : IFormatProvider, ICustomFormatter
        {
            private readonly IFormatProvider inner = inner;

            public object GetFormat(Type formatType)
            {
                if (formatType == typeof(ICustomFormatter))
                {
                    return this;
                }
                return inner.GetFormat(formatType);
            }

            public string Format(string format, object arg, IFormatProvider formatProvider)
            {
                // 書式の指定がない（または "G" だけの）float / double だけを .NET Framework と同じにする
                var isDefaultFormat = String.IsNullOrEmpty(format) || format == "G" || format == "g";
                if (isDefaultFormat)
                {
                    if (arg is float f)
                    {
                        return LegacyNumberFormat.ToString(f, inner);
                    }
                    if (arg is double d)
                    {
                        return LegacyNumberFormat.ToString(d, inner);
                    }
                }
                if (arg is IFormattable formattable)
                {
                    return formattable.ToString(format, inner);
                }
                return arg?.ToString() ?? String.Empty;
            }
        }
    }
}
