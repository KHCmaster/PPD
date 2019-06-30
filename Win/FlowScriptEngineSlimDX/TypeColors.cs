using FlowScriptEngine;
using SharpDX;
using System;
using System.Collections.Generic;

namespace FlowScriptEngineSlimDX
{
    public class TypeColors : TypeColorBase
    {
        Dictionary<Type, System.Drawing.Color> dictionary;
        public TypeColors()
        {
            dictionary = new Dictionary<Type, System.Drawing.Color>
            {
                { typeof(Color4), System.Drawing.Color.FromArgb(64, 30, 215) },
                { typeof(Vector2), System.Drawing.Color.FromArgb(0, 92, 230) },
                { typeof(Vector3), System.Drawing.Color.FromArgb(41, 117, 230) },
                { typeof(Vector4), System.Drawing.Color.FromArgb(75, 137, 230) },
                { typeof(Matrix), System.Drawing.Color.FromArgb(25, 134, 170) },
                { typeof(Quaternion), System.Drawing.Color.FromArgb(75, 186, 223) }
            };
        }

        public override IEnumerable<KeyValuePair<Type, System.Drawing.Color>> EnumerateColors()
        {
            return dictionary;
        }
    }
}
