using FlowScriptEngine;
using PPDCoreModel;
using PPDFramework;
using System.Collections.Generic;

namespace FlowScriptEnginePPD.FlowSourceObjects.Mark
{
    [ModifyData]
    [ModifyDataWarning]
    public abstract class ProcessMissPressFlowSourceObjectBase : FlowSourceObjectBase, IProcessMissPress
    {
        protected ProcessMissPressManager procManager;

        [ToolTipText("Mark_ProcessMissPress_Out")]
        public event FlowEventHandler Out;
        private event FlowEventHandler OutEnd;

        IMarkInfo markInfo;

        public override string Name
        {
            get { return "PPD.Mark.ProcessMissPress"; }
        }

        protected ProcessMissPressFlowSourceObjectBase()
        {
            OutEnd += ProcessMissPressFlowSourceObjectBase_OutEnd;
        }

        void ProcessMissPressFlowSourceObjectBase_OutEnd(FlowEventArgs e)
        {
            SetValue(nameof(Handled));
            if (Handled)
            {
                SetValue(nameof(IsMissPress));
            }

            EvaluateHandled = Handled;
            ProcessMissPress = IsMissPress;
        }

        [Ignore]
        public abstract int Priority { get; }

        [ToolTipText("Mark_ProcessMissPress_Position")]
        public SharpDX.Vector2 Position
        {
            get { return markInfo != null ? markInfo.Position : SharpDX.Vector2.Zero; }
        }

        [ToolTipText("Mark_ProcessMissPress_Rotation")]
        public float Rotation
        {
            get { return markInfo != null ? markInfo.Angle : 0; }
        }

        [ToolTipText("Mark_ProcessMissPress_Time")]
        public float Time
        {
            get { return markInfo != null ? markInfo.Time : 0; }
        }

        [ToolTipText("Mark_ProcessMissPress_ReleaseTime")]
        public float ReleaseTime
        {
            get { return markInfo != null ? markInfo.ReleaseTime : 0; }
        }

        [ToolTipText("Mark_ProcessMissPress_MarkID")]
        public int MarkID
        {
            get { return markInfo != null ? (int)markInfo.ID : 0; }
        }

        [ToolTipText("Mark_ProcessMissPress_Type")]
        public PPDCoreModel.Data.MarkType Type
        {
            get { return markInfo != null ? markInfo.Type : 0; }
        }

        [ToolTipText("Mark_ProcessMissPress_IsLong")]
        public bool IsLong
        {
            get { return markInfo != null && markInfo.IsLong; }
        }

        [ToolTipText("Mark_ProcessMissPress_IsAC")]
        public bool IsAC
        {
            get { return markInfo != null && markInfo.IsAC; }
        }

        [ToolTipText("Mark_ProcessMissPress_HasSameTimingMark")]
        public bool HasSameTimingMark
        {
            get { return markInfo != null && markInfo.HasSameTimingMark; }
        }

        [ToolTipText("Mark_ProcessMissPress_IsACFT")]
        public bool IsACFT
        {
            get { return markInfo != null && markInfo.IsACFT; }
        }

        [ToolTipText("Mark_ProcessMissPress_IsScratch")]
        public bool IsScratch
        {
            get { return markInfo != null && markInfo.IsScratch; }
        }

        [ToolTipText("Mark_ProcessMissPress_IsRight")]
        public bool IsRight
        {
            get { return markInfo != null && markInfo.IsRight; }
        }

        [ToolTipText("Mark_ProcessMissPress_Parameters")]
        public Dictionary<object, object> Parameters
        {
            get { return markInfo?.Parameters; }
        }

        [ToolTipText("Mark_ProcessMissPress_SlideScale")]
        public float SlideScale
        {
            get { return markInfo != null ? markInfo.SlideScale : 0; }
        }

        [ToolTipText("Mark_ProcessMissPress_SameTimingMarks")]
        public int SameTimingMarks
        {
            get { return markInfo != null ? markInfo.SameTimingMarks : 0; }
        }

        [ToolTipText("Mark_ProcessMissPress_Handled")]
        public bool Handled
        {
            protected get;
            set;
        }

        [Ignore]
        public bool EvaluateHandled
        {
            get;
            protected set;
        }

        [ToolTipText("Mark_ProcessMissPress_EvaluateRequired")]
        public bool EvaluateRequired
        {
            private get;
            set;
        }

        [Ignore]
        public bool ProcessMissPress
        {
            get;
            protected set;
        }

        [ToolTipText("Mark_ProcessMissPress_PressedButton")]
        public PPDCoreModel.Data.MarkType PressedButton
        {
            get;
            protected set;
        }

        [ToolTipText("Mark_ProcessMissPress_IsMissPress")]
        public bool IsMissPress
        {
            protected get;
            set;
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (this.Manager.Items.TryGetValue("ProcessMissPressManager", out object obj))
            {
                procManager = obj as ProcessMissPressManager;
                procManager.Add(this);
            }
        }

        #region IProcessMissPress メンバ

        public virtual bool IsEvaluateRequired()
        {
            SetValue(nameof(EvaluateRequired));
            return EvaluateRequired;
        }

        public virtual void Process(IMarkInfo markInfo, PPDCoreModel.Data.MarkType buttonType)
        {
            this.markInfo = markInfo;
            PressedButton = buttonType;

            FireEvent(Out, true);
            FireEvent(OutEnd, true);
        }

        #endregion
    }
}
