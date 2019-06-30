using PPDEditorCommon;

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Mark.PosAndRotation
{
    [ToolTipText("Mark_PosAndRotation_Load_Summary")]
    public partial class LoadFlowSourceObject : ExecutableFlowSourceObject
    {
        private IPosAndAngleLoader loader;

        public override string Name
        {
            get { return "PPDEditor.Mark.PosAndRotation.Load"; }
        }

        protected override void OnInitialize()
        {
            if (Manager.Items.ContainsKey("PosAndAngleLoader"))
            {
                loader = (IPosAndAngleLoader)Manager.Items["PosAndAngleLoader"];
            }
        }

        [ToolTipText("Mark_PosAndRotation_Load_FileName")]
        public string FileName
        {
            private get;
            set;
        }

        [ToolTipText("Mark_PosAndRotation_Load_Values")]
        public object[] Values
        {
            get;
            private set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            if (loader != null)
            {
                SetValue(nameof(FileName));
                Values = loader.Load(FileName);
                if (Values == null)
                {
                    OnFailed();
                }
                else
                {
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
