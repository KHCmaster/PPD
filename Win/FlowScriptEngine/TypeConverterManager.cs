using System;
using System.Collections.Generic;

namespace FlowScriptEngine
{
    public static class TypeConverterManager
    {
        static Dictionary<Type, Dictionary<Type, TypeConverterBase>> dict;
        static Dictionary<Type, TypeConverterFromBase> fromDict;
        static Dictionary<Type, TypeConverterToBase> toDict;

        static TypeConverterManager()
        {
            dict = new Dictionary<Type, Dictionary<Type, TypeConverterBase>>();
            fromDict = new Dictionary<Type, TypeConverterFromBase>();
            toDict = new Dictionary<Type, TypeConverterToBase>();
        }

        private static TypeConverterBase GetConverter(Type from, Type to)
        {
            if (dict.TryGetValue(from, out Dictionary<Type, TypeConverterBase> table))
            {
                if (table.TryGetValue(to, out TypeConverterBase ret))
                {
                    return ret;
                }
            }

            return null;
        }

        internal static object Convert(Type from, Type to, object data)
        {
            var converter = GetConverter(from, to);
            if (converter != null)
            {
                return converter.Convert(data);
            }
            else
            {
                if (fromDict.TryGetValue(from, out TypeConverterFromBase converterFrom) && converterFrom.CanConvert(to))
                {
                    return converterFrom.Convert(data, to);
                }

                if (toDict.TryGetValue(to, out TypeConverterToBase converterTo) && converterTo.CanConvert(from))
                {
                    return converterTo.Convert(data, from);
                }
                return null;
            }
        }

        public static bool CanConvert(Type from, Type to)
        {
            var converter = GetConverter(from, to);
            if (converter != null)
            {
                return true;
            }
            else
            {
                if (fromDict.TryGetValue(from, out TypeConverterFromBase converterFrom) && converterFrom.CanConvert(to))
                {
                    return true;
                }

                if (toDict.TryGetValue(to, out TypeConverterToBase converterTo) && converterTo.CanConvert(from))
                {
                    return true;
                }
                return false;
            }
        }

        internal static void AddConverter(TypeConverterBase converter)
        {
            if (!dict.TryGetValue(converter.From, out Dictionary<Type, TypeConverterBase> innerDict))
            {
                innerDict = new Dictionary<Type, TypeConverterBase>();
                dict.Add(converter.From, innerDict);
            }
            if (!innerDict.ContainsKey(converter.To))
            {
                innerDict.Add(converter.To, converter);
            }
            else
            {
                Console.WriteLine("Converter {0} is multiple defined.({1}->{2})", converter, converter.From, converter.To);
            }
        }

        internal static void AddConverter(TypeConverterFromBase converterFrom)
        {
            if (!fromDict.ContainsKey(converterFrom.From))
            {
                fromDict.Add(converterFrom.From, converterFrom);
            }
            else
            {
                Console.WriteLine("Converter {0} is multiple defined.({1}->*)", converterFrom, converterFrom.From);
            }
        }

        internal static void AddConverter(TypeConverterToBase converterTo)
        {
            if (!toDict.ContainsKey(converterTo.To))
            {
                toDict.Add(converterTo.To, converterTo);
            }
            else
            {
                Console.WriteLine("Converter {0} is multiple defined.(*->{1})", converterTo, converterTo.To);
            }
        }
    }
}
