﻿using FlowScriptEngine;
using System;
using System.Xml.Linq;

namespace FlowScriptEngineBasic.TypeSerializers
{
    public class FloatSerializer : TypeSerializerBase
    {
        public override Type Type
        {
            get { return typeof(float); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            SetValue(element, value.ToString());
        }
    }
}
