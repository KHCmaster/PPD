using FlowScriptEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

namespace FlowScriptEngineBasic
{
    public class TypeColors : TypeColorBase
    {
        Dictionary<Type, Color> dictionary;
        public TypeColors()
        {
            dictionary = new Dictionary<Type, Color>
            {
                { typeof(object[]), Color.FromArgb(241, 100, 100) },
                { typeof(List<object>), Color.FromArgb(241, 135, 135) },
                { typeof(IEnumerable<object>), Color.FromArgb(241, 170, 170) },
                { typeof(IOrderedEnumerable<object>), Color.FromArgb(221, 170, 170) },
                { typeof(IGrouping<object, object>), Color.FromArgb(201, 170, 170) },
                { typeof(ILookup<object, object>), Color.FromArgb(180, 170, 170) },
                { typeof(bool), Color.FromArgb(255, 0, 255) },
                { typeof(double), Color.FromArgb(0, 162, 238) },
                { typeof(float), Color.FromArgb(0, 121, 177) },
                { typeof(Dictionary<object, object>), Color.FromArgb(241, 192, 135) },
                { typeof(HashSet<Object>), Color.FromArgb(241, 224, 167) },
                { typeof(KeyValuePair<object, object>), Color.FromArgb(255, 229, 198) },
                { typeof(int), Color.FromArgb(0, 70, 102) },
                { typeof(string), Color.FromArgb(255, 255, 0) },
                { typeof(object), Color.FromArgb(255, 255, 255) },
                { typeof(Stream), Color.FromArgb(154, 164, 37) },
                { typeof(SeekOrigin), Color.FromArgb(0, 70, 102) },
                { typeof(EncodingType), Color.FromArgb(0, 70, 102) },
                { typeof(Stopwatch), Color.FromArgb(136, 157, 20) },
                { typeof(DateTime), Color.FromArgb(255, 0, 0) },
                { typeof(TimeSpan), Color.FromArgb(180, 0, 0) },
                { typeof(System.DayOfWeek), Color.FromArgb(0, 70, 102) }
            };
        }

        public override IEnumerable<KeyValuePair<Type, Color>> EnumerateColors()
        {
            return dictionary;
        }
    }
}
