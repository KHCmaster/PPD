using FlowScriptEngine;
using System;
using System.Globalization;

namespace FlowScriptEngineBasic.TypeFormatters
{
    public class IntFormatter : TypeFormatterBase
    {
        public override Type Type
        {
            get { return typeof(int); }
        }

        public override bool Format(string str, out object value)
        {
            value = null;
            if (int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out int val))
            {
                value = val;
                return true;
            }
            return false;
        }

        public override string CorrentFormat
        {
            get { return "100"; }
        }
    }
}
