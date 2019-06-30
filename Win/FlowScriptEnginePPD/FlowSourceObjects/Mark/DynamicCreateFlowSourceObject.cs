using PPDCoreModel;
using PPDFramework;
using SharpDX;

namespace FlowScriptEnginePPD.FlowSourceObjects.Mark
{
    [ToolTipText("Mark_DynamicCreate_Summary")]
    [ModifyData]
    [ModifyDataWarning]
    public partial class DynamicCreateFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPD.Mark.DynamicCreate"; }
        }

        private IMarkManager markManager;

        protected override void OnInitialize()
        {
            base.OnInitialize();


            if (this.Manager.Items.ContainsKey("MarkManager"))
            {
                markManager = this.Manager.Items["MarkManager"] as IMarkManager;
            }
        }

        [ToolTipText("Mark_DynamicCreate_Position")]
        public Vector2 Position
        {
            private get;
            set;
        }

        [ToolTipText("Mark_DynamicCreate_Angle")]
        public float Angle
        {
            private get;
            set;
        }

        [ToolTipText("Mark_DynamicCreate_MarkType")]
        public PPDCoreModel.Data.MarkType MarkType
        {
            private get;
            set;
        }

        [ToolTipText("Mark_DynamicCreate_Time")]
        public float Time
        {
            private get;
            set;
        }

        [ToolTipText("Mark_DynamicCreate_EndTime")]
        public float EndTime
        {
            private get;
            set;
        }

        [ToolTipText("Mark_DynamicCreate_IsLong")]
        public bool IsLong
        {
            private get;
            set;
        }

        [ToolTipText("Mark_DynamicCreate_MarkID")]
        public int MarkID
        {
            private get;
            set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetValue(nameof(Position));
            SetValue(nameof(Angle));
            SetValue(nameof(MarkType));
            SetValue(nameof(Time));
            SetValue(nameof(EndTime));
            SetValue(nameof(IsLong));
            SetValue(nameof(MarkID));

            if (markManager != null)
            {
                if (IsLong)
                {
                    if (Time < EndTime)
                    {
                        markManager.AddLongMark(Position, Angle, Time, EndTime, MarkType, MarkID);
                        OnSuccess();
                    }
                    else
                    {
                        OnFailed();
                    }
                }
                else
                {
                    markManager.AddMark(Position, Angle, Time, MarkType, MarkID);
                    OnSuccess();
                }
            }
            else
            {
                OnFailed();
            }
        }
    }
}
