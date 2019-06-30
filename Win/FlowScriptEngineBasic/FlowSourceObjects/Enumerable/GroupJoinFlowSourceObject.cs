using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_GroupJoin_Summary")]
    public partial class GroupJoinFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("Enumerable_GroupJoin_OuterSelect")]
        public event FlowEventHandler OuterSelect;
        [ToolTipText("Enumerable_GroupJoin_InnerSelect")]
        public event FlowEventHandler InnerSelect;
        [ToolTipText("Enumerable_GroupJoin_ResultSelect")]
        public event FlowEventHandler ResultSelect;

        private IEnumerable<object> enumerable;

        public override string Name
        {
            get { return "Enumerable.GroupJoin"; }
        }

        [ToolTipText("Enumerable_GroupJoin_InnerEnumerable")]
        public IEnumerable<object> InnerEnumerable
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_GroupJoin_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            get
            {
                SetValue(nameof(Enumerable));
                SetValue(nameof(InnerEnumerable));
                if (enumerable == null || InnerEnumerable == null)
                {
                    return System.Linq.Enumerable.Empty<object>();
                }
                return enumerable.GroupJoin(InnerEnumerable, obj =>
                {
                    OuterSelectValue = obj;
                    FireEvent(OuterSelect, true);
                    ProcessChildEvent();
                    SetValue(nameof(OuterSelectResult));
                    return OuterSelectResult;
                },
                obj =>
                {
                    InnerSelectValue = obj;
                    FireEvent(InnerSelect, true);
                    ProcessChildEvent();
                    SetValue(nameof(InnerSelectResult));
                    return InnerSelectResult;
                },
                (obj, list) =>
                {
                    ResultSelectValue = obj;
                    ResultSelectValues = list;
                    FireEvent(ResultSelect, true);
                    ProcessChildEvent();
                    SetValue(nameof(ResultSelectResult));
                    return ResultSelectResult;
                });
            }
            set
            {
                enumerable = value;
            }
        }

        [ToolTipText("Enumerable_GroupJoin_InnerSelectValue")]
        public object InnerSelectValue
        {
            get;
            private set;
        }

        [ToolTipText("Enumerable_GroupJoin_InnerSelectResult")]
        public object InnerSelectResult
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_GroupJoin_OuterSelectValue")]
        public object OuterSelectValue
        {
            get;
            private set;
        }

        [ToolTipText("Enumerable_GroupJoin_OuterSelectResult")]
        public object OuterSelectResult
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_GroupJoin_ResultSelectValue")]
        public object ResultSelectValue
        {
            get;
            private set;
        }

        [ToolTipText("Enumerable_GroupJoin_ResultSelectValues")]
        public object ResultSelectValues
        {
            get;
            private set;
        }

        [ToolTipText("Enumerable_GroupJoin_ResultSelectResult")]
        public object ResultSelectResult
        {
            private get;
            set;
        }
    }
}
