using PPDMath;
using FlowScriptEngine;
using System;
using System.Drawing;
using System.Globalization;

namespace FlowScriptEngineSlimDX.TypeFormatters
{
    public class Color4Formatter : TypeFormatterBase
    {
        public override Type Type
        {
            get { return typeof(Vortice.Mathematics.Color4); }
        }

        public override bool Format(string str, out object value)
        {
            value = null;
            Vortice.Mathematics.Color4 color;
            if (str.StartsWith('#'))
            {
                if (GetSharpColor(str[1..], out color))
                {
                    value = color;
                    return true;
                }
            }
            else
            {
                try
                {
                    var c = ColorTranslator.FromHtml(str);
                    color = new Vortice.Mathematics.Color4(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
                    value = color;
                    return true;
                }
                catch
                {
                    if (GetSharpColor(str, out color))
                    {
                        value = color;
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool GetSharpColor(string str, out Vortice.Mathematics.Color4 color)
        {
            color = new Vortice.Mathematics.Color4();
            try
            {
                int a, r, g, b;
                switch (str.Length)
                {
                    case 3:
                        r = int.Parse(str[..1], NumberStyles.HexNumber);
                        g = int.Parse(str.Substring(1, 1), NumberStyles.HexNumber);
                        b = int.Parse(str.Substring(2, 1), NumberStyles.HexNumber);
                        color = new Vortice.Mathematics.Color4(r / 255f, g / 255f, b / 255f, 1f);
                        return true;
                    case 6:
                        r = int.Parse(str[..2], NumberStyles.HexNumber);
                        g = int.Parse(str.Substring(2, 2), NumberStyles.HexNumber);
                        b = int.Parse(str.Substring(4, 2), NumberStyles.HexNumber);
                        color = new Vortice.Mathematics.Color4(r / 255f, g / 255f, b / 255f, 1f);
                        return true;
                    case 8:
                        a = int.Parse(str[..2], NumberStyles.HexNumber);
                        r = int.Parse(str.Substring(2, 2), NumberStyles.HexNumber);
                        g = int.Parse(str.Substring(4, 2), NumberStyles.HexNumber);
                        b = int.Parse(str.Substring(6, 2), NumberStyles.HexNumber);
                        color = new Vortice.Mathematics.Color4(r / 255f, g / 255f, b / 255f, a / 255f);
                        return true;
                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public override string CorrentFormat
        {
            get { return "#FFF or #FFFFFF or #FFFFFFFF or white"; }
        }
    }
}
