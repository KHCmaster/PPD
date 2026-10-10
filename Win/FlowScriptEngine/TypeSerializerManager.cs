using System;
using System.Collections.Generic;

namespace FlowScriptEngine
{
    public static class TypeSerializerManager
    {
        static readonly Dictionary<Type, TypeSerializerBase> serializers;

        static TypeSerializerManager()
        {
            serializers = [];
        }

        public static TypeSerializerBase GetSerializer(Type type)
        {
            if (type == null)
            {
                return null;
            }

            serializers.TryGetValue(type, out TypeSerializerBase serializer);
            return serializer;
        }

        internal static void AddSerializer(TypeSerializerBase serializer)
        {
            if (!serializers.TryAdd(serializer.Type, serializer))
            {
                Console.WriteLine("Serializer {0} is multiple defined. Type:{1}", serializer, serializer.Type);
            }
        }
    }
}
