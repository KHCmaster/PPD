using FlowScriptEngine;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace FlowScriptEngineSlimDX.TypeFormatters
{
    public class Vector3Formatter : TypeFormatterBase
    {
        static Regex regex = new Regex("^X:(?<X>[0-9.-]+) +Y:(?<Y>[0-9.-]+) +Z:(?<Z>[0-9.-]+)$");

        public override Type Type
        {
            get { return typeof(SharpDX.Vector3); }
        }

        public override bool Format(string str, out object value)
        {
            value = null;
            var m = regex.Match(str);
            if (m.Success)
            {
                if (float.TryParse(m.Groups["X"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float x)
                    && float.TryParse(m.Groups["Y"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float y)
                    && float.TryParse(m.Groups["Z"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
                {
                    value = new SharpDX.Vector3(x, y, z);
                    return true;
                }
            }

            return false;
        }

        public override string CorrentFormat
        {
            get { return "X:2.5 Y:2.2 Z:1.0"; }
        }
    }
}
