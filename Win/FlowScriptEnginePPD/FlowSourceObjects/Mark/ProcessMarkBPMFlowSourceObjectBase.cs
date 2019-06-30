using FlowScriptEngine;
using PPDCoreModel;
using System.Collections.Generic;

namespace FlowScriptEnginePPD.FlowSourceObjects.Mark
{
    public abstract class ProcessMarkBPMFlowSourceObjectBase : FlowSourceObjectBase, IProcessMarkBPM
    {
        [Ignore]
        public abstract int Priority { get; }

        [ToolTipText("Mark_ProcessMarkBPM_Out")]
        public event FlowEventHandler Out;
        private event FlowEventHandler OutEnd;

        IMarkInfo markInfo;

        public override string Name
        {
            get { return "PPD.Mark.ProcessMarkBPM"; }
        }

        protected ProcessMarkBPMFlowSourceObjectBase()
        {
            OutEnd += ProcessMarkBPMFlowSourceObjectBase_OutEnd;
        }

        void ProcessMarkBPMFlowSourceObjectBase_OutEnd(FlowEventArgs e)
        {
            SetValue(nameof(Handled));
            if (Handled)
            {
                SetValue(nameof(BPM));
            }

            EvaluateHandled = Handled;
            ProcessBPM = BPM;
        }

        [ToolTipText("Mark_ProcessMarkBPM_Position")]
        public SharpDX.Vector2 Position
        {
            get { return markInfo != null ? markInfo.Position : SharpDX.Vector2.Zero; }
        }

        [ToolTipText("Mark_ProcessMarkBPM_Rotation")]
        public float Rotation
        {
            get { return markInfo != null ? markInfo.Angle : 0; }
        }

        [ToolTipText("Mark_ProcessMarkBPM_Time")]
        public float Time
        {
            get { return markInfo != null ? markInfo.Time : 0; }
        }

        [ToolTipText("Mark_ProcessMarkBPM_ReleaseTime")]
        public float ReleaseTime
        {
            get { return markInfo != null ? markInfo.ReleaseTime : 0; }
        }

        [ToolTipText("Mark_ProcessMarkBPM_MarkID")]
        public int MarkID
        {
            get { return markInfo != null ? (int)markInfo.ID : 0; }
        }

        [ToolTipText("Mark_ProcessMarkBPM_Type")]
        public PPDCoreModel.Data.MarkType Type
        {
            get { return markInfo != null ? markInfo.Type : 0; }
        }

        [ToolTipText("Mark_ProcessMarkBPM_IsLong")]
        public bool IsLong
        {
            get { return markInfo != null && markInfo.IsLong; }
        }

        [ToolTipText("Mark_ProcessMarkBPM_IsAC")]
        public bool IsAC
        {
            get { return markInfo != null && markInfo.IsAC; }
        }

        [ToolTipText("Mark_ProcessMarkBPM_HasSameTimingMark")]
        public bool HasSameTimingMark
        {
            get { return markInfo != null && markInfo.HasSameTimingMark; }
        }

        [ToolTipText("Mark_ProcessMarkBPM_IsACFT")]
        public bool IsACFT
        {
            get { return markInfo != null && markInfo.IsACFT; }
        }

        [ToolTipText("Mark_ProcessMarkBPM_IsScratch")]
        public bool IsScratch
        {
            get { return markInfo != null && markInfo.IsScratch; }
        }

        [ToolTipText("Mark_ProcessMarkBPM_IsRight")]
        public bool IsRight
        {
            get { return markInfo != null && markInfo.IsRight; }
        }

        [ToolTipText("Mark_ProcessMarkBPM_Parameters")]
        public Dictionary<object, object> Parameters
        {
            get { return markInfo?.Parameters; }
        }

        [ToolTipText("Mark_ProcessMarkBPM_SlideScale")]
        public float SlideScale
        {
            get { return markInfo != null ? markInfo.SlideScale : 0; }
        }

        [ToolTipText("Mark_ProcessMarkBPM_SameTimingMarks")]
        public int SameTimingMarks
        {
            get { return markInfo != null ? markInfo.SameTimingMarks : 0; }
        }

        [ToolTipText("Mark_ProcessMarkBPM_BPM")]
        public float BPM
        {
            protected get;
            set;
        }

        [ToolTipText("Mark_ProcessMarkBPM_Handled")]
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

        [Ignore]
        public float ProcessBPM
        {
            get;
            private set;
        }

        [ToolTipText("Mark_ProcessMarkBPM_EvaluateRequired")]
        public bool EvaluateRequired
        {
            private get;
            set;
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (this.Manager.Items.TryGetValue("ProcessMarkBPMManager", out object obj))
            {
                var processManager = obj as ProcessMarkBPMManager;
                processManager.Add(this);
            }
        }

        public virtual bool IsEvaluateRequired()
        {
            SetValue(nameof(EvaluateRequired));
            return EvaluateRequired;
        }

        public virtual void Process(IMarkInfo markInfo)
        {
            this.markInfo = markInfo;

            FireEvent(Out, true);
            FireEvent(OutEnd, true);
        }
    }
}
