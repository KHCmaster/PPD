using FlowScriptEngine;
using System;

namespace FlowScriptEngineBasic.TypeConverters
{
    public class EnumIntConverter : TypeConverterToBase
    {
        public override Type To
        {
            get { return typeof(int); }
        }

        public override bool CanConvert(Type from)
        {
            return from.IsEnum;
        }

        public override object Convert(object data, Type from)
        {
            return (int)data;
        }
    }
}
