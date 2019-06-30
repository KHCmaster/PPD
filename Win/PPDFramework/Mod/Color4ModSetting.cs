using SharpDX;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PPDFramework.Mod
{
    /// <summary>
    /// 色の設定を扱うクラスです。
    /// </summary>
    public class Color4ModSetting : TemplateModSetting<Color4>
    {
        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="defaultValue"></param>
        public Color4ModSetting(string key, string name, string description, Color4 defaultValue)
            : base(key, name, description, defaultValue)
        {
        }

        /// <summary>
        /// 文字列を取得します。
        /// </summary>
        /// <param name="value">入力データ。</param>
        /// <returns>文字列。</returns>
        public override string GetStringValue(object value)
        {
            var color = (Color4)value;
            return String.Format("#{0:X2}{1:X2}{2:X2}", (int)(color.Red * 255), (int)(color.Green * 255), (int)(color.Blue * 255));
        }

        /// <summary>
        /// バリデートを行います。
        /// </summary>
        /// <param name="value">入力文字列。</param>
        /// <param name="val">出力データ。</param>
        /// <returns>バリデートの結果。</returns>
        public override bool Validate(string value, out object val)
        {
            var regex = new Regex("^#([0-9a-fA-F]{2})([0-9a-fA-F]{2})([0-9a-fA-F]{2})$");
            var regex2 = new Regex("^#([0-9a-fA-F]{1})([0-9a-fA-F]{1})([0-9a-fA-F]{1})$");
            var match = regex.Match(value);
            if (!match.Success)
            {
                match = regex2.Match(value);
            }

            if (!match.Success)
            {
                val = null;
                return false;
            }

            int r = int.Parse(match.Groups[1].Value, NumberStyles.HexNumber),
                g = int.Parse(match.Groups[2].Value, NumberStyles.HexNumber),
                b = int.Parse(match.Groups[3].Value, NumberStyles.HexNumber);
            val = new Color4(r / 255f, g / 255f, b / 255f, 1f);
            return true;
        }
    }
}
