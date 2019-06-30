using FlowScriptEngine;
using PPDCoreModel;
using SharpDX;
using System.Collections.Generic;

namespace FlowScriptEnginePPD.FlowSourceObjects.Mark
{
    public abstract class CalculatePosFlowSourceObjectBase : FlowSourceObjectBase, ICalculatePosition
    {
        [Ignore]
        public abstract int Priority { get; }

        [ToolTipText("Mark_CalculatePos_Out")]
        public event FlowEventHandler Out;
        private event FlowEventHandler OutEnd;

        IMarkInfo markInfo;

        public override string Name
        {
            get { return "PPD.Mark.CalculatePos"; }
        }

        protected CalculatePosFlowSourceObjectBase()
        {
            OutEnd += CalculateMarkPosFlowSourceObjectBase_OutEnd;
        }

        void CalculateMarkPosFlowSourceObjectBase_OutEnd(FlowEventArgs e)
        {
            SetValue(nameof(Handled));
            if (Handled)
            {
                SetValue(nameof(ColorPosition));
            }

            EvaluateHandled = Handled;
            CalculatePosition = ColorPosition;
        }

        [ToolTipText("Mark_CalculatePos_Position")]
        public SharpDX.Vector2 Position
        {
            get { return markInfo != null ? markInfo.Position : Vector2.Zero; }
        }

        [ToolTipText("Mark_CalculatePos_Rotation")]
        public float Rotation
        {
            get { return markInfo != null ? markInfo.Angle : 0; }
        }

        [ToolTipText("Mark_CalculatePos_Time")]
        public float Time
        {
            get { return markInfo != null ? markInfo.Time : 0; }
        }

        [ToolTipText("Mark_CalculatePos_ReleaseTime")]
        public float ReleaseTime
        {
            get { return markInfo != null ? markInfo.ReleaseTime : 0; }
        }

        [ToolTipText("Mark_CalculatePos_MarkID")]
        public int MarkID
        {
            get { return markInfo != null ? (int)markInfo.ID : 0; }
        }

        [ToolTipText("Mark_CalculatePos_Type")]
        public PPDCoreModel.Data.MarkType Type
        {
            get { return markInfo != null ? markInfo.Type : 0; }
        }

        [ToolTipText("Mark_CalculatePos_IsLong")]
        public bool IsLong
        {
            get { return markInfo != null && markInfo.IsLong; }
        }

        [ToolTipText("Mark_CalculatePos_IsAC")]
        public bool IsAC
        {
            get { return markInfo != null && markInfo.IsAC; }
        }

        [ToolTipText("Mark_CalculatePos_CurrentTime")]
        public float CurrentTime
        {
            get;
            protected set;
        }

        [ToolTipText("Mark_CalculatePos_HasSameTimingMark")]
        public bool HasSameTimingMark
        {
            get { return markInfo != null && markInfo.HasSameTimingMark; }
        }

        [ToolTipText("Mark_CalculatePos_IsACFT")]
        public bool IsACFT
        {
            get { return markInfo != null && markInfo.IsACFT; }
        }

        [ToolTipText("Mark_CalculatePos_IsScratch")]
        public bool IsScratch
        {
            get { return markInfo != null && markInfo.IsScratch; }
        }

        [ToolTipText("Mark_CalculatePos_IsRight")]
        public bool IsRight
        {
            get { return markInfo != null && markInfo.IsRight; }
        }

        [ToolTipText("Mark_CalculatePos_Parameters")]
        public Dictionary<object, object> Parameters
        {
            get { return markInfo?.Parameters; }
        }

        [ToolTipText("Mark_CalculatePos_SlideScale")]
        public float SlideScale
        {
            get { return markInfo != null ? markInfo.SlideScale : 0; }
        }

        [ToolTipText("Mark_CalculatePos_SameTimingMarks")]
        public int SameTimingMarks
        {
            get { return markInfo != null ? markInfo.SameTimingMarks : 0; }
        }

        [ToolTipText("Mark_CalculatePos_ColorPosition")]
        public SharpDX.Vector2 ColorPosition
        {
            protected get;
            set;
        }

        [ToolTipText("Mark_CalculatePos_Handled")]
        public bool Handled
        {
            protected get;
            set;
        }

        [ToolTipText("Mark_CalculatePos_BPM")]
        public float BPM
        {
            get;
            protected set;
        }

        [Ignore]
        public bool EvaluateHandled
        {
            get;
            protected set;
        }

        [Ignore]
        public Vector2 CalculatePosition
        {
            get;
            private set;
        }

        [ToolTipText("Mark_CalculatePos_EvaluateRequired")]
        public bool EvaluateRequired
        {
            private get;
            set;
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (this.Manager.Items.TryGetValue("CalculatePositionManager", out object obj))
            {
                var calcManager = obj as CalculatePositionManager;
                calcManager.Add(this);
            }
        }

        public virtual bool IsEvaluateRequired()
        {
            SetValue(nameof(EvaluateRequired));
            return EvaluateRequired;
        }

        public virtual void Calculate(IMarkInfo markInfo, float currentTime, float bpm)
        {
            this.markInfo = markInfo;
            CurrentTime = currentTime;
            BPM = bpm;

            FireEvent(Out, true);
            FireEvent(OutEnd, true);
        }
    }
}
