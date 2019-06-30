using FlowScriptEngine;
using PPDCoreModel;
using PPDFramework;
using System.Collections.Generic;

namespace FlowScriptEnginePPD.FlowSourceObjects.Mark
{
    public abstract class CreateImageFlowSourceObjectBase : FlowSourceObjectBase, ICreateMark
    {
        [Ignore]
        public abstract int Priority { get; }

        [ToolTipText("Mark_CreateImage_Out")]
        public event FlowEventHandler Out;
        private event FlowEventHandler OutEnd;

        IMarkInfo markInfo;

        public override string Name
        {
            get { return "PPD.Mark.CreateImage"; }
        }

        protected CreateImageFlowSourceObjectBase()
        {
            OutEnd += CalculateMarkPosFlowSourceObjectBase_OutEnd;
        }

        void CalculateMarkPosFlowSourceObjectBase_OutEnd(FlowEventArgs e)
        {
            SetValue(nameof(Handled));
            if (Handled)
            {
                SetValue(nameof(MarkImage));
                SetValue(nameof(ColorMarkImage));
                SetValue(nameof(AxisImage));
                SetValue(nameof(SlideMarkImage));
                SetValue(nameof(SlideColorMarkImage));
                SetValue(nameof(TraceImage));
            }

            EvaluateHandled = Handled;
            CreatedMark = MarkImage;
            CreatedColorMark = ColorMarkImage;
            CreatedAxis = AxisImage;
            CreatedSlideMark = SlideMarkImage;
            CreatedSlideColorMark = SlideColorMarkImage;
            CreatedTrace = TraceImage;
        }

        [ToolTipText("Mark_CreateImage_Position")]
        public SharpDX.Vector2 Position
        {
            get { return markInfo != null ? markInfo.Position : SharpDX.Vector2.Zero; }
        }

        [ToolTipText("Mark_CreateImage_Rotation")]
        public float Rotation
        {
            get { return markInfo != null ? markInfo.Angle : 0; }
        }

        [ToolTipText("Mark_CreateImage_Time")]
        public float Time
        {
            get { return markInfo != null ? markInfo.Time : 0; }
        }

        [ToolTipText("Mark_CreateImage_ReleaseTime")]
        public float ReleaseTime
        {
            get { return markInfo != null ? markInfo.ReleaseTime : 0; }
        }

        [ToolTipText("Mark_CreateImage_MarkID")]
        public int MarkID
        {
            get { return markInfo != null ? (int)markInfo.ID : 0; }
        }

        [ToolTipText("Mark_CreateImage_Type")]
        public PPDCoreModel.Data.MarkType Type
        {
            get { return markInfo != null ? markInfo.Type : 0; }
        }

        [ToolTipText("Mark_CreateImage_IsLong")]
        public bool IsLong
        {
            get { return markInfo != null && markInfo.IsLong; }
        }

        [ToolTipText("Mark_CreateImage_IsAC")]
        public bool IsAC
        {
            get { return markInfo != null && markInfo.IsAC; }
        }

        [ToolTipText("Mark_CreateImage_IsACFT")]
        public bool IsACFT
        {
            get { return markInfo != null && markInfo.IsACFT; }
        }

        [ToolTipText("Mark_CreateImage_IsScratch")]
        public bool IsScratch
        {
            get { return markInfo != null && markInfo.IsScratch; }
        }

        [ToolTipText("Mark_CreateImage_IsRight")]
        public bool IsRight
        {
            get { return markInfo != null && markInfo.IsRight; }
        }

        [ToolTipText("Mark_CreateImage_Parameters")]
        public Dictionary<object, object> Parameters
        {
            get { return markInfo?.Parameters; }
        }

        [ToolTipText("Mark_CreateImage_SlideScale")]
        public float SlideScale
        {
            get { return markInfo != null ? markInfo.SlideScale : 0; }
        }

        [ToolTipText("Mark_CreateImage_HasSameTimingMark")]
        public bool HasSameTimingMark
        {
            get { return markInfo != null && markInfo.HasSameTimingMark; }
        }

        [ToolTipText("Mark_CreateImage_SameTimingMarks")]
        public int SameTimingMarks
        {
            get { return markInfo != null ? markInfo.SameTimingMarks : 0; }
        }

        [ToolTipText("Mark_CreateImage_MarkImage")]
        public GameComponent MarkImage
        {
            protected get;
            set;
        }

        [ToolTipText("Mark_CreateImage_ColorMarkImage")]
        public GameComponent ColorMarkImage
        {
            protected get;
            set;
        }

        [ToolTipText("Mark_CreateImage_AxisImage")]
        public GameComponent AxisImage
        {
            protected get;
            set;
        }

        [ToolTipText("Mark_CreateImage_SlideMarkImage")]
        public GameComponent SlideMarkImage
        {
            protected get;
            set;
        }

        [ToolTipText("Mark_CreateImage_SlideColorMarkImage")]
        public GameComponent SlideColorMarkImage
        {
            protected get;
            set;
        }

        [ToolTipText("Mark_CreateImage_TraceImage")]
        public PictureObject TraceImage
        {
            protected get;
            set;
        }

        [ToolTipText("Mark_CreateImage_Handled")]
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
        public GameComponent CreatedMark
        {
            get;
            protected set;
        }

        [Ignore]
        public GameComponent CreatedColorMark
        {
            get;
            protected set;
        }

        [Ignore]
        public GameComponent CreatedAxis
        {
            get;
            protected set;
        }

        [Ignore]
        public GameComponent CreatedSlideMark
        {
            get;
            protected set;
        }

        [Ignore]
        public GameComponent CreatedSlideColorMark
        {
            get;
            protected set;
        }

        [Ignore]
        public PictureObject CreatedTrace
        {
            get;
            protected set;
        }

        [ToolTipText("Mark_CreateImage_EvaluateRequired")]
        public bool EvaluateRequired
        {
            private get;
            set;
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (this.Manager.Items.TryGetValue("CreateMarkManager", out object obj))
            {
                var createManager = obj as CreateMarkManager;
                createManager.Add(this);
            }
        }

        public virtual bool IsEvaluateRequired()
        {
            SetValue(nameof(EvaluateRequired));
            return EvaluateRequired;
        }

        public virtual void Create(IMarkInfo markInfo)
        {
            this.markInfo = markInfo;

            FireEvent(Out, true);
            FireEvent(OutEnd, true);
        }
    }
}
