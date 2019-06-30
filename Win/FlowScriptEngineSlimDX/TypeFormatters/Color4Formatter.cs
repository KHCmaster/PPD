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
            get { return typeof(SharpDX.Color4); }
        }

        public override bool Format(string str, out object value)
        {
            value = null;
            SharpDX.Color4 color;
            if (str.StartsWith("#"))
            {
                if (GetSharpColor(str.Substring(1), out color))
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
                    color = new SharpDX.Color4(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
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

        private bool GetSharpColor(string str, out SharpDX.Color4 color)
        {
            color = new SharpDX.Color4();
            try
            {
                int a, r, g, b;
                switch (str.Length)
                {
                    case 3:
                        r = int.Parse(str.Substring(0, 1), NumberStyles.HexNumber);
                        g = int.Parse(str.Substring(1, 1), NumberStyles.HexNumber);
                        b = int.Parse(str.Substring(2, 1), NumberStyles.HexNumber);
                        color = new SharpDX.Color4(r / 255f, g / 255f, b / 255f, 1f);
                        return true;
                    case 6:
                        r = int.Parse(str.Substring(0, 2), NumberStyles.HexNumber);
                        g = int.Parse(str.Substring(2, 2), NumberStyles.HexNumber);
                        b = int.Parse(str.Substring(4, 2), NumberStyles.HexNumber);
                        color = new SharpDX.Color4(r / 255f, g / 255f, b / 255f, 1f);
                        return true;
                    case 8:
                        a = int.Parse(str.Substring(0, 2), NumberStyles.HexNumber);
                        r = int.Parse(str.Substring(2, 2), NumberStyles.HexNumber);
                        g = int.Parse(str.Substring(4, 2), NumberStyles.HexNumber);
                        b = int.Parse(str.Substring(6, 2), NumberStyles.HexNumber);
                        color = new SharpDX.Color4(r / 255f, g / 255f, b / 255f, a / 255f);
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
