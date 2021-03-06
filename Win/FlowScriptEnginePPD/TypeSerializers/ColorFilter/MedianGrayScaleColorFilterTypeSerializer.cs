﻿using FlowScriptEngine;
using PPDFramework.Shaders;
using System;
using System.Xml.Linq;

namespace FlowScriptEnginePPD.TypeSerializers.ColorFilter
{
    public class MedianGrayScaleColorFilterTypeSerializer : ColorFilterBaseTypeSerializer
    {
        public override Type Type
        {
            get { return typeof(MedianGrayScaleColorFilter); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var filter = (MedianGrayScaleColorFilter)value;
            Serialize(serializer, element, filter);
        }
    }
}
