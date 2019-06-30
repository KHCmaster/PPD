using FlowScriptEngine;
using System;
using System.Diagnostics;
using System.Xml.Linq;

namespace FlowScriptEngineBasic.TypeSerializers
{
    public class StopwatchTypeSerializer : TypeSerializerBase
    {
        public override Type Type
        {
            get { return typeof(Stopwatch); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var stopwatch = (Stopwatch)value;
            var elepsedMSElem = serializer.CreateElement("ElapsedMilliSeconds");
            var isRunningElem = serializer.CreateElement("IsRunning");
            element.Add(elepsedMSElem, isRunningElem);
            serializer.Serialize(elepsedMSElem, stopwatch.ElapsedMilliseconds);
            serializer.Serialize(isRunningElem, stopwatch.IsRunning);
        }
    }
}
