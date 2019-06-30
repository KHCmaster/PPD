using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_Zip_Summary")]
    public partial class ZipFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("Enumerable_Zip_Select")]
        public event FlowEventHandler Select;

        public override string Name
        {
            get { return "Enumerable.Zip"; }
        }

        [ToolTipText("Enumerable_Zip_First")]
        public IEnumerable<object> First
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_Zip_Second")]
        public IEnumerable<object> Second
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_Zip_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            get
            {
                SetValue(nameof(First));
                SetValue(nameof(Second));
                if (First == null || Second == null)
                {
                    if (Second == null)
                    {
                        return System.Linq.Enumerable.Empty<object>();
                    }
                    else
                    {
                        return Second;
                    }
                }
                else if (Second == null)
                {
                    return First;
                }
                return First.Zip(Second, (obj1, obj2) =>
                {
                    Obj1 = obj1;
                    Obj2 = obj2;
                    FireEvent(Select, true);
                    ProcessChildEvent();
                    SetValue(nameof(Result));
                    return Result;
                });
            }
        }

        [ToolTipText("Enumerable_Zip_Obj1")]
        public object Obj1
        {
            get;
            private set;
        }

        [ToolTipText("Enumerable_Zip_Obj2")]
        public object Obj2
        {
            get;
            private set;
        }

        [ToolTipText("Enumerable_Zip_Result")]
        public object Result
        {
            private get;
            set;
        }
    }
}
