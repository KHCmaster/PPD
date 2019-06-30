using FlowScriptEngine;
using System;

namespace FlowScriptEngineBasic.TypeConverters
{
    public class ObjectFromConverter : TypeConverterFromBase
    {
        public override Type From
        {
            get { return typeof(object); }
        }

        public override bool CanConvert(Type to)
        {
            return true;
        }

        public override object Convert(object data, Type to)
        {
            return data;
        }
    }
}
