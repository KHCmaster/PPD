using FlowScriptEngine;
using PPDCoreModel;
using PPDFramework;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEnginePPD.FlowSourceObjects.Mark
{
    [ModifyData]
    [ModifyDataWarning]
    public abstract class ProcessAllowedButtonsFlowSourceObjectBase : FlowSourceObjectBase, IProcessAllowedButtons
    {
        protected ProcessAllowedButtonsManager procManager;

        [ToolTipText("Mark_ProcessAllowedButtons_Out")]
        public event FlowEventHandler Out;
        private event FlowEventHandler OutEnd;

        IMarkInfo markInfo;

        public override string Name
        {
            get { return "PPD.Mark.ProcessAllowedButtons"; }
        }

        protected ProcessAllowedButtonsFlowSourceObjectBase()
        {
            OutEnd += ProcessAllowedButtonsFlowSourceObjectBase_OutEnd;
        }

        private void ProcessAllowedButtonsFlowSourceObjectBase_OutEnd(FlowEventArgs e)
        {
            SetValue(nameof(Handled));
            if (Handled)
            {
                SetValue(nameof(AllowedButtons));
            }

            EvaluateHandled = Handled;
            ProcessAllowedButtons = AllowedButtons?.Where(b => b != null && b is PPDCoreModel.Data.MarkType).Cast<ButtonType>().ToArray();
        }

        [Ignore]
        public abstract int Priority { get; }

        [ToolTipText("Mark_ProcessAllowedButtons_Position")]
        public SharpDX.Vector2 Position
        {
            get { return markInfo != null ? markInfo.Position : SharpDX.Vector2.Zero; }
        }

        [ToolTipText("Mark_ProcessAllowedButtons_Rotation")]
        public float Rotation
        {
            get { return markInfo != null ? markInfo.Angle : 0; }
        }

        [ToolTipText("Mark_ProcessAllowedButtons_Time")]
        public float Time
        {
            get { return markInfo != null ? markInfo.Time : 0; }
        }

        [ToolTipText("Mark_ProcessAllowedButtons_ReleaseTime")]
        public float ReleaseTime
        {
            get { return markInfo != null ? markInfo.ReleaseTime : 0; }
        }

        [ToolTipText("Mark_ProcessAllowedButtons_MarkID")]
        public int MarkID
        {
            get { return markInfo != null ? (int)markInfo.ID : 0; }
        }

        [ToolTipText("Mark_ProcessAllowedButtons_Type")]
        public PPDCoreModel.Data.MarkType Type
        {
            get { return markInfo != null ? markInfo.Type : 0; }
        }

        [ToolTipText("Mark_ProcessAllowedButtons_IsLong")]
        public bool IsLong
        {
            get { return markInfo != null && markInfo.IsLong; }
        }

        [ToolTipText("Mark_ProcessAllowedButtons_IsAC")]
        public bool IsAC
        {
            get { return markInfo != null && markInfo.IsAC; }
        }

        [ToolTipText("Mark_ProcessAllowedButtons_HasSameTimingMark")]
        public bool HasSameTimingMark
        {
            get { return markInfo != null && markInfo.HasSameTimingMark; }
        }

        [ToolTipText("Mark_ProcessAllowedButtons_IsACFT")]
        public bool IsACFT
        {
            get { return markInfo != null && markInfo.IsACFT; }
        }

        [ToolTipText("Mark_ProcessAllowedButtons_IsScratch")]
        public bool IsScratch
        {
            get { return markInfo != null && markInfo.IsScratch; }
        }

        [ToolTipText("Mark_ProcessAllowedButtons_IsRight")]
        public bool IsRight
        {
            get { return markInfo != null && markInfo.IsRight; }
        }

        [ToolTipText("Mark_ProcessAllowedButtons_Parameters")]
        public Dictionary<object, object> Parameters
        {
            get { return markInfo?.Parameters; }
        }

        [ToolTipText("Mark_ProcessAllowedButtons_SlideScale")]
        public float SlideScale
        {
            get { return markInfo != null ? markInfo.SlideScale : 0; }
        }

        [ToolTipText("Mark_ProcessAllowedButtons_SameTimingMarks")]
        public int SameTimingMarks
        {
            get { return markInfo != null ? markInfo.SameTimingMarks : 0; }
        }

        [ToolTipText("Mark_ProcessAllowedButtons_Handled")]
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

        [ToolTipText("Mark_ProcessAllowedButtons_EvaluateRequired")]
        public bool EvaluateRequired
        {
            private get;
            set;
        }

        [Ignore]
        public ButtonType[] ProcessAllowedButtons
        {
            get;
            protected set;
        }

        [ToolTipText("Mark_ProcessAllowedButtons_AllowedButtons")]
        public IEnumerable<object> AllowedButtons
        {
            protected get;
            set;
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (this.Manager.Items.TryGetValue("ProcessAllowedButtonsManager", out object obj))
            {
                procManager = obj as ProcessAllowedButtonsManager;
                procManager.Add(this);
            }
        }

        #region IProcessAllowedButtons メンバ

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

        #endregion
    }
}
