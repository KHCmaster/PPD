using PPDEditorCommon;

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Mark
{
    [ToolTipText("Mark_SetTime_Summary")]
    public partial class SetTimeFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPDEditor.Mark.SetTime"; }
        }

        [ToolTipText("Mark_SetTime_Mark")]
        public IEditorMarkInfo Mark
        {
            private get;
            set;
        }

        [ToolTipText("Mark_SetTime_Time")]
        public float Time
        {
            private get;
            set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetValue(nameof(Mark));
            if (Mark != null)
            {
                SetValue(nameof(Time));
                Mark.Layer.ChangeMarkPropertyManager.ChangeMarkTime(Mark, Time);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
