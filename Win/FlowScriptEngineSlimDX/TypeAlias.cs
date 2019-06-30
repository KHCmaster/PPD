using FlowScriptEngine;
using System;
using System.Collections.Generic;

namespace FlowScriptEngineSlimDX
{
    public class TypeAlias : TypeAliasBase
    {
        Dictionary<Type, string> dictionary;
        public TypeAlias()
        {
            dictionary = new Dictionary<Type, string>
            {
                { typeof(SharpDX.Vector2), "Vector2" },
                { typeof(SharpDX.Vector3), "Vector3" },
                { typeof(SharpDX.Vector4), "Vector4" },
                { typeof(SharpDX.Matrix), "Matrix" },
                { typeof(SharpDX.Quaternion), "Quaternion" },
                { typeof(SharpDX.Color4), "Color4" }
            };
        }

        public override IEnumerable<KeyValuePair<Type, string>> EnumerateAlias()
        {
            return dictionary;
        }
    }
}
