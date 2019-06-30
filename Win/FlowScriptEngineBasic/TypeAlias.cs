using FlowScriptEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FlowScriptEngineBasic
{
    public class TypeAlias : TypeAliasBase
    {
        Dictionary<Type, string> dictionary;
        public TypeAlias()
        {
            dictionary = new Dictionary<Type, string>
            {
                { typeof(int), "Int32" },
                { typeof(float), "Float" },
                { typeof(double), "Double" },
                { typeof(bool), "Boolean" },
                { typeof(string), "String" },
                { typeof(object), "Object" },
                { typeof(object[]), "Array" },
                { typeof(List<object>), "ArrayList" },
                { typeof(Dictionary<object, object>), "Hashtable" },
                { typeof(HashSet<object>), "HashSet" },
                { typeof(KeyValuePair<object, object>), "Pair" },
                { typeof(IEnumerable<object>), "Enumerable" },
                { typeof(IOrderedEnumerable<object>), "Enumerable.Ordered" },
                { typeof(IGrouping<object, object>), "Enumerable.Grouping" },
                { typeof(ILookup<object, object>), "Enumerable.Lookup" },
                { typeof(Stream), "Stream" },
                { typeof(SeekOrigin), "Stream.SeekOrigin" },
                { typeof(EncodingType), "Encoding" },
                { typeof(Stopwatch), "Stopwatch" },
                { typeof(DateTime), "DateTime" },
                { typeof(TimeSpan), "TimeSpan" },
                { typeof(System.DayOfWeek), "DateTime.DayOfWeek" }
            };
        }

        public override IEnumerable<KeyValuePair<Type, string>> EnumerateAlias()
        {
            return dictionary;
        }
    }
}
