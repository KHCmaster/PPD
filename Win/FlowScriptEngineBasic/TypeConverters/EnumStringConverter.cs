using FlowScriptEngine;
using System;

namespace FlowScriptEngineBasic.TypeConverters
{
    public class EnumStringConverter : TypeConverterToBase
    {
        public override Type To
        {
            get { return typeof(string); }
        }

        public override bool CanConvert(Type from)
        {
            return from.IsEnum;
        }

        public override object Convert(object data, Type from)
        {
            return Enum.GetName(from, data);
        }
    }
}
