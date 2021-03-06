﻿using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.ArrayList
{
    [ToolTipText("ArrayList_BinarySearch_Summary")]
    public partial class BinarySearchWithLimitFlowSourceObject : ArrayListFlowSourceObjectBase
    {
        [ToolTipText("ArrayList_BinarySearch_Compare")]
        public event FlowEventHandler Compare;

        public override string Name
        {
            get { return "ArrayList.BinarySearch"; }
        }

        [ToolTipText("ArrayList_BinarySearch_Value")]
        public object Value
        {
            private get;
            set;
        }

        [ToolTipText("ArrayList_BinarySearch_StartIndex")]
        public int StartIndex
        {
            private get;
            set;
        }

        [ToolTipText("ArrayList_BinarySearch_Count")]
        public int Count
        {
            private get;
            set;
        }

        [ToolTipText("ArrayList_BinarySearch_X")]
        public object X
        {
            get;
            private set;
        }

        [ToolTipText("ArrayList_BinarySearch_Y")]
        public object Y
        {
            get;
            private set;
        }

        [ToolTipText("ArrayList_BinarySearch_Result")]
        public int Result
        {
            private get;
            set;
        }

        [ToolTipText("ArrayList_BinarySearch_FoundIndex")]
        public int FoundIndex
        {
            get;
            private set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetArrayList();
            if (ArrayList != null)
            {
                SetValue(nameof(Value));
                SetValue(nameof(StartIndex));
                SetValue(nameof(Count));
                var comparer = new CallbackComparer((x, y) =>
                {
                    X = x;
                    Y = y;
                    FireEvent(Compare, true);
                    ProcessChildEvent();
                    SetValue(nameof(Result));
                    return Result;
                });
                FoundIndex = ArrayList.BinarySearch(StartIndex, Count, Value, comparer);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
