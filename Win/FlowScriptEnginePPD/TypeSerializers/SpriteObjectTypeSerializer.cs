using FlowScriptEngine;
using PPDFramework;
using System;
using System.Xml.Linq;

namespace FlowScriptEnginePPD.TypeSerializers
{
    public class SpriteObjectTypeSerializer : GameComponentTypeSerializer
    {
        public override Type Type
        {
            get { return typeof(SpriteObject); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            Serialize(serializer, element, (GameComponent)value);
        }
    }
}
