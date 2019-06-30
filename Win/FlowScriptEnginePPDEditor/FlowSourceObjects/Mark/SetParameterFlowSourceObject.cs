using PPDEditorCommon;

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Mark
{
    [ToolTipText("Mark_SetParameter_Summary")]
    public partial class SetParameterFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPDEditor.Mark.SetParameter"; }
        }

        [ToolTipText("Mark_SetParameter_Mark")]
        public IEditorMarkInfo Mark
        {
            private get;
            set;
        }

        [ToolTipText("Mark_SetParameter_Key")]
        public string Key
        {
            private get;
            set;
        }

        [ToolTipText("Mark_SetParameter_Value")]
        public string Value
        {
            private get;
            set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetValue(nameof(Mark));
            if (Mark != null)
            {
                SetValue(nameof(Key));
                SetValue(nameof(Value));
                Mark.Layer.ChangeMarkPropertyManager.ChangeParameter(Mark, Key, Value);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
