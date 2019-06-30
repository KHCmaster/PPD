using PPDCoreModel;
using System.IO;

namespace FlowScriptEnginePPD.FlowSourceObjects.Resource
{
    [ToolTipText("Resource_Value")]
    public partial class ValueFlowSourceObject : ExecutableFlowSourceObject
    {
        private ScriptResourceManager resourceManager;

        public override string Name
        {
            get { return "PPD.Resource.Value"; }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (this.Manager.Items.ContainsKey("ResourceManager"))
            {
                resourceManager = this.Manager.Items["ResourceManager"] as ScriptResourceManager;
            }
        }

        [ToolTipText("Resource_Value_Path")]
        public string Path
        {
            private get;
            set;
        }

        [ToolTipText("Resource_Value_Value")]
        public Stream Value
        {
            get;
            private set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetValue(nameof(Path));
            if (resourceManager != null)
            {
                Value = resourceManager.GetResource(Path, PPDCoreModel.Data.ResourceKind.Others, null) as Stream;
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
