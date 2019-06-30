using FlowScriptEngine;
using System;

namespace FlowScriptEngineBasic.TypeFormatters
{
    public class StringFormatter : TypeFormatterBase
    {
        public override Type Type
        {
            get { return typeof(string); }
        }

        public override bool Format(string str, out object value)
        {
            value = str;
            return true;
        }
    }
}
