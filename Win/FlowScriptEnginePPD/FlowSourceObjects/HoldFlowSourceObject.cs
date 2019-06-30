using FlowScriptEngine;
using PPDCoreModel;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEnginePPD.FlowSourceObjects
{
    [ToolTipText("Hold_Summary")]
    public partial class HoldFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("Hold_Start")]
        public event FlowEventHandler Start;
        [ToolTipText("Hold_Change")]
        public event FlowEventHandler Change;
        [ToolTipText("Hold_End")]
        public event FlowEventHandler End;
        [ToolTipText("Hold_MaxHold")]
        public event FlowEventHandler MaxHold;

        public override string Name
        {
            get { return "PPD.Hold"; }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            Manager.RegisterCallBack(HoldInfo.HoldStart, HoldStart);
            Manager.RegisterCallBack(HoldInfo.HoldChange, HoldChange);
            Manager.RegisterCallBack(HoldInfo.HoldEnd, HoldEnd);
            Manager.RegisterCallBack(HoldInfo.MaxHold, HoldMaxHold);
        }

        [ToolTipText("Hold_CurrentHoldScore")]
        public int CurrentHoldScore
        {
            get;
            private set;
        }

        [ToolTipText("Hold_MaxHoldBonus")]
        public int MaxHoldBonus
        {
            get;
            private set;
        }

        [ToolTipText("Hold_HoldMarks")]
        public IEnumerable<object> HoldMarks
        {
            get;
            private set;
        }

        private void UpdateInfo(HoldInfo holdInfo)
        {
            CurrentHoldScore = holdInfo.CurrentHoldScore;
            MaxHoldBonus = holdInfo.MaxHoldBonus;
            HoldMarks = holdInfo.HoldMarks.Cast<object>();
        }

        private void HoldStart(object[] args)
        {
            var holdInfo = args[0] as HoldInfo;
            UpdateInfo(holdInfo);
            FireEvent(Start);
        }

        private void HoldChange(object[] args)
        {
            var holdInfo = args[0] as HoldInfo;
            UpdateInfo(holdInfo);
            FireEvent(Change);
        }

        private void HoldEnd(object[] args)
        {
            var holdInfo = args[0] as HoldInfo;
            UpdateInfo(holdInfo);
            FireEvent(End);
        }

        private void HoldMaxHold(object[] args)
        {
            var holdInfo = args[0] as HoldInfo;
            UpdateInfo(holdInfo);
            FireEvent(MaxHold);
        }
    }
}
