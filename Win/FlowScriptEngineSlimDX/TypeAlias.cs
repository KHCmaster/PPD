using PPDMath;
using FlowScriptEngine;
using System;
using System.Collections.Generic;

namespace FlowScriptEngineSlimDX
{
    public class TypeAlias : TypeAliasBase
    {
        readonly Dictionary<Type, string> dictionary;
        public TypeAlias()
        {
            dictionary = new Dictionary<Type, string>
            {
                { typeof(System.Numerics.Vector2), "Vector2" },
                { typeof(System.Numerics.Vector3), "Vector3" },
                { typeof(System.Numerics.Vector4), "Vector4" },
                { typeof(System.Numerics.Matrix4x4), "Matrix" },
                { typeof(System.Numerics.Quaternion), "Quaternion" },
                { typeof(Vortice.Mathematics.Color4), "Color4" }
            };
        }

        public override IEnumerable<KeyValuePair<Type, string>> EnumerateAlias()
        {
            return dictionary;
        }
    }
}
