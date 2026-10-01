using PPDMath;
using FlowScriptEngine;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace FlowScriptEngineSlimDX.TypeFormatters
{
    public class Vector3Formatter : TypeFormatterBase
    {
        static readonly Regex regex = new("^X:(?<X>[0-9.-]+) +Y:(?<Y>[0-9.-]+) +Z:(?<Z>[0-9.-]+)$");

        public override Type Type
        {
            get { return typeof(System.Numerics.Vector3); }
        }

        public override bool Format(string str, out object value)
        {
            value = null;
            var m = regex.Match(str);
            if (m.Success)
            {
                if (LegacyNumberFormat.TryParse(m.Groups["X"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float x)
                    && LegacyNumberFormat.TryParse(m.Groups["Y"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float y)
                    && LegacyNumberFormat.TryParse(m.Groups["Z"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
                {
                    value = new System.Numerics.Vector3(x, y, z);
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
