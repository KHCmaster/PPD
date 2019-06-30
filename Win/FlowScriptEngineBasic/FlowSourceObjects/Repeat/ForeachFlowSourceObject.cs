using FlowScriptEngine;
using System.Collections.Generic;

namespace FlowScriptEngineBasic.FlowSourceObjects.Repeat
{
    [ToolTipText("Repeat_Foreach_Summary")]
    public partial class ForeachFlowSourceObject : FlowSourceObjectBase
    {
        IEnumerator<object> enumerator;

        [ToolTipText("Repeat_Foreach_Loop")]
        public event FlowEventHandler Loop;
        private event FlowEventHandler LoopEnd;
        public ForeachFlowSourceObject()
        {
            LoopEnd += ForeachFlowSourceObject_LoopEnd;
        }

        void ForeachFlowSourceObject_LoopEnd(FlowEventArgs e)
        {
            if (e.IsBreakUsed)
            {
                enumerator.Dispose();
                return;
            }

            Iter++;
            if (enumerator.MoveNext())
            {
                Value = enumerator.Current;
                FireEvent(Loop, true);
                FireEvent(LoopEnd, true, true);
            }
            else
            {
                enumerator.Dispose();
            }
        }

        public override string Name
        {
            get { return "Repeat.Foreach"; }
        }

        [Replaced("ArrayList")]
        [ToolTipText("Repeat_Foreach_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            private get;
            set;
        }

        [ToolTipText("Repeat_Foreach_Value")]
        public object Value
        {
            get;
            private set;
        }

        [ToolTipText("Repeat_Foreach_Iter")]
        public int Iter
        {
            get;
            private set;
        }

        [ToolTipText("Repeat_Foreach_In")]
        public void In(FlowEventArgs e)
        {
            SetValue(nameof(Enumerable));
            if (Enumerable != null)
            {
                enumerator = Enumerable.GetEnumerator();
                if (enumerator.MoveNext())
                {
                    Iter = 0;
                    Value = enumerator.Current;
                    FireEvent(Loop, true);
                    FireEvent(LoopEnd, true, true);
                }
                else
                {
                    enumerator.Dispose();
                }
            }
        }
    }
}
