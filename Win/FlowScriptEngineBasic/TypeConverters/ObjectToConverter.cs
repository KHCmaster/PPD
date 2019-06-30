using FlowScriptEngine;
using System;

namespace FlowScriptEngineBasic.TypeConverters
{
    public class ObjectToConverter : TypeConverterToBase
    {
        public override Type To
        {
            get { return typeof(object); }
        }

        public override bool CanConvert(Type from)
        {
            return true;
        }

        public override object Convert(object data, Type from)
        {
            return data;
        }
    }
}
