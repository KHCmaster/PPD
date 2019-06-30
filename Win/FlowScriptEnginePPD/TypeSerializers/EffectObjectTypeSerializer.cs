using FlowScriptEngine;
using PPDFramework;
using System;
using System.Xml.Linq;

namespace FlowScriptEnginePPD.TypeSerializers
{
    public class EffectObjectTypeSerializer : GameComponentTypeSerializer
    {
        public override Type Type
        {
            get { return typeof(EffectObject); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var effectObject = (EffectObject)value;
            Serialize(serializer, element, effectObject);
            var alignmentElem = serializer.CreateElement("Alignment");
            var playStateElem = serializer.CreateElement("PlayState");
        }
    }
}
