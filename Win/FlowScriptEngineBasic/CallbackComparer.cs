using System;
using System.Collections.Generic;

namespace FlowScriptEngineBasic
{
    class CallbackComparer : Comparer<object>
    {
        private Comparison<object> comparison;

        public CallbackComparer(Comparison<object> comparison)
        {
            this.comparison = comparison;
        }

        public override int Compare(object x, object y)
        {
            return comparison(x, y);
        }
    }
}
