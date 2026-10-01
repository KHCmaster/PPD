using System;
using System.Collections.Generic;

namespace FlowScriptEngineBasic
{
    class CallbackComparer(Comparison<object> comparison) : Comparer<object>
    {
        private readonly Comparison<object> comparison = comparison;

        public override int Compare(object x, object y)
        {
            return comparison(x, y);
        }
    }
}
