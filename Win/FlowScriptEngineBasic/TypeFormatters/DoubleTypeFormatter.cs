using FlowScriptEngine;
using System;
using System.Globalization;

namespace FlowScriptEngineBasic.TypeFormatters
{
    public class DoubleFormatter : TypeFormatterBase
    {

        public override Type Type
        {
            get { return typeof(double); }
        }

        public override bool Format(string str, out object value)
        {
            value = null;
            if (double.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out double val))
            {
                value = val;
                return true;
            }
            return false;
        }

        public override string CorrentFormat
        {
            get { return "100.0"; }
        }
    }
}
