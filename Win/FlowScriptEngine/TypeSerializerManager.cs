using System;
using System.Collections.Generic;

namespace FlowScriptEngine
{
    public static class TypeSerializerManager
    {
        static Dictionary<Type, TypeSerializerBase> serializers;

        static TypeSerializerManager()
        {
            serializers = new Dictionary<Type, TypeSerializerBase>();
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
            if (!serializers.ContainsKey(serializer.Type))
            {
                serializers.Add(serializer.Type, serializer);
            }
            else
            {
                Console.WriteLine("Serializer {0} is multiple defined. Type:{1}", serializer, serializer.Type);
            }
        }
    }
}
