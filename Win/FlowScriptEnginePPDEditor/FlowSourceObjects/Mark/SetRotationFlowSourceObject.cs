using PPDEditorCommon;

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Mark
{
    [ToolTipText("Mark_SetRotation_Summary")]
    public partial class SetRotationFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPDEditor.Mark.SetRotation"; }
        }

        [ToolTipText("Mark_SetRotation_Mark")]
        public IEditorMarkInfo Mark
        {
            private get;
            set;
        }

        [ToolTipText("Mark_SetRotation_Rotation")]
        public float Rotation
        {
            private get;
            set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetValue(nameof(Mark));
            if (Mark != null)
            {
                SetValue(nameof(Rotation));
                Mark.Layer.ChangeMarkPropertyManager.ChangeMarkAngle(Mark, Rotation);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
