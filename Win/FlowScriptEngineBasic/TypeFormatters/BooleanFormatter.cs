using FlowScriptEngine;
using System;

namespace FlowScriptEngineBasic.TypeFormatters
{
    public class BooleanFormatter : TypeFormatterBase
    {
        private string[] allowed = { "False", "True" };
        public override Type Type
        {
            get { return typeof(bool); }
        }

        public override bool Format(string str, out object value)
        {
            value = str == "True";
            return true;
        }

        public override string[] AllowedPropertyString
        {
            get
            {
                return allowed;
            }
        }
    }
}
