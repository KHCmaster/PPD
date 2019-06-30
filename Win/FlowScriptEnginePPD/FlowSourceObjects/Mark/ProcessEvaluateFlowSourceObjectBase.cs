using FlowScriptEngine;
using PPDCoreModel;
using SharpDX;
using System.Collections.Generic;

namespace FlowScriptEnginePPD.FlowSourceObjects.Mark
{
    public abstract class ProcessEvaluateFlowSourceObjectBase : FlowSourceObjectBase, IProcessEvaluate
    {
        protected ProcessEvaluateManager procManager;

        IMarkInfo markInfo;

        [ToolTipText("Mark_ProcessEvalute_Out")]
        public event FlowEventHandler Out;
        private event FlowEventHandler OutEnd;

        public override string Name
        {
            get { return "PPD.Mark.ProcessEvaluate"; }
        }

        protected ProcessEvaluateFlowSourceObjectBase()
        {
            OutEnd += ProcessMarkEvaluateFlowSourceObjectBase_OutEnd;
        }

        void ProcessMarkEvaluateFlowSourceObjectBase_OutEnd(FlowEventArgs e)
        {
            SetValue(nameof(Handled));

            EvaluateHandled = Handled;
        }

        [Ignore]
        public abstract int Priority { get; }

        [ToolTipText("Mark_ProcessEvalute_Position")]
        public SharpDX.Vector2 Position
        {
            get;
            private set;
        }

        [ToolTipText("Mark_ProcessEvalute_Rotation")]
        public float Rotation
        {
            get { return markInfo != null ? markInfo.Angle : 0; }
        }

        [ToolTipText("Mark_ProcessEvalute_Time")]
        public float Time
        {
            get { return markInfo != null ? markInfo.Time : 0; }
        }

        [ToolTipText("Mark_ProcessEvaluate_ReleaseTime")]
        public float ReleaseTime
        {
            get { return markInfo != null ? markInfo.ReleaseTime : 0; }
        }

        [ToolTipText("Mark_ProcessEvalute_MarkID")]
        public int MarkID
        {
            get { return markInfo != null ? (int)markInfo.ID : 0; }
        }

        [ToolTipText("Mark_ProcessEvalute_Type")]
        public PPDCoreModel.Data.MarkType Type
        {
            get { return markInfo != null ? markInfo.Type : 0; }
        }

        [ToolTipText("Mark_ProcessEvalute_IsLong")]
        public bool IsLong
        {
            get { return markInfo != null && markInfo.IsLong; }
        }

        [ToolTipText("Mark_ProcessEvalute_IsAC")]
        public bool IsAC
        {
            get { return markInfo != null && markInfo.IsAC; }
        }

        [ToolTipText("Mark_ProcessEvalute_HasSameTimingMark")]
        public bool HasSameTimingMark
        {
            get { return markInfo != null && markInfo.HasSameTimingMark; }
        }

        [ToolTipText("Mark_ProcessEvalute_IsACFT")]
        public bool IsACFT
        {
            get { return markInfo != null && markInfo.IsACFT; }
        }

        [ToolTipText("Mark_ProcessEvalute_IsScratch")]
        public bool IsScratch
        {
            get { return markInfo != null && markInfo.IsScratch; }
        }

        [ToolTipText("Mark_ProcessEvalute_IsRight")]
        public bool IsRight
        {
            get { return markInfo != null && markInfo.IsRight; }
        }

        [ToolTipText("Mark_ProcessEvalute_Parameters")]
        public Dictionary<object, object> Parameters
        {
            get { return markInfo?.Parameters; }
        }

        [ToolTipText("Mark_ProcessEvalute_SlideScale")]
        public float SlideScale
        {
            get { return markInfo != null ? markInfo.SlideScale : 0; }
        }

        [ToolTipText("Mark_ProcessEvalute_SameTimingMarks")]
        public int SameTimingMarks
        {
            get { return markInfo != null ? markInfo.SameTimingMarks : 0; }
        }

        [ToolTipText("Mark_ProcessEvalute_IsMissPress")]
        public bool IsMissPress
        {
            get;
            protected set;
        }

        [ToolTipText("Mark_ProcessEvalute_IsRelease")]
        public bool IsRelease
        {
            get;
            protected set;
        }

        [ToolTipText("Mark_ProcessEvalute_Handled")]
        public bool Handled
        {
            protected get;
            set;
        }

        [ToolTipText("Mark_ProcessEvalute_Evaluate")]
        public PPDCoreModel.Data.EffectType EffectType
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

        [ToolTipText("Mark_ProcessEvalute_EvaluateRequired")]
        public bool EvaluateRequired
        {
            private get;
            set;
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (this.Manager.Items.TryGetValue("ProcessEvaluateManager", out object obj))
            {
                procManager = obj as ProcessEvaluateManager;
                procManager.Add(this);
            }
        }

        #region IProcessEvalute メンバ

        public virtual bool IsEvaluateRequired()
        {
            SetValue(nameof(EvaluateRequired));
            return EvaluateRequired;
        }

        public virtual void ProcessEvaluate(IMarkInfo markInfo, PPDCoreModel.Data.EffectType effectType, bool missPress, bool release, Vector2 position)
        {
            this.markInfo = markInfo;
            IsMissPress = missPress;
            IsRelease = release;
            EffectType = effectType;
            Position = position;

            FireEvent(Out, true);
            FireEvent(OutEnd, true);
        }

        #endregion
    }
}
