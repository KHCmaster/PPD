using FlowScriptEngine;
using System;

namespace FlowScriptEngineBasic.TypeConverters
{
    public class StringEnumConverter : TypeConverterFromBase
    {
        public override Type From
        {
            get { return typeof(string); }
        }

        public override bool CanConvert(Type to)
        {
            return to.IsEnum;
        }

        public override object Convert(object data, Type to)
        {
            return Enum.Parse(to, data as string);
        }
    }
}
