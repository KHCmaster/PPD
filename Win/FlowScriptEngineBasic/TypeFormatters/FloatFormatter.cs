using FlowScriptEngine;
using System;
using System.Globalization;

namespace FlowScriptEngineBasic.TypeFormatters
{
    public class FloatFormatter : TypeFormatterBase
    {

        public override Type Type
        {
            get { return typeof(float); }
        }

        public override bool Format(string str, out object value)
        {
            value = null;
            if (float.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out float val))
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
