using FlowScriptEngine;
using System;

namespace FlowScriptEngineBasic.TypeConverters
{
    public class IntEnumConverter : TypeConverterFromBase
    {
        public override Type From
        {
            get { return typeof(int); }
        }

        public override bool CanConvert(Type to)
        {
            return to.IsEnum;
        }

        public override object Convert(object data, Type to)
        {
            return Enum.ToObject(to, data);
        }
    }
}
