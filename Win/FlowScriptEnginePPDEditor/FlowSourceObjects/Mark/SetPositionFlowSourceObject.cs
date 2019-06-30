using PPDEditorCommon;
using SharpDX;

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Mark
{
    [ToolTipText("Mark_SetPosition_Summary")]
    public partial class SetPositionFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPDEditor.Mark.SetPosition"; }
        }

        [ToolTipText("Mark_SetPosition_Mark")]
        public IEditorMarkInfo Mark
        {
            private get;
            set;
        }

        [ToolTipText("Mark_SetPosition_Position")]
        public Vector2 Position
        {
            private get;
            set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetValue(nameof(Mark));
            if (Mark != null)
            {
                SetValue(nameof(Position));
                Mark.Layer.ChangeMarkPropertyManager.ChangeMarkPosition(Mark, Position);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
