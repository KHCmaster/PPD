using PPDCoreModel;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.Effect.Pool
{
    [ToolTipText("Effect_Pool_Value_Summary")]
    public partial class ValueFlowSourceObject : ExecutableFlowSourceObject
    {
        private ScriptResourceManager resourceManager;

        public override string Name
        {
            get { return "PPD.Graphics.Effect.Pool.Value"; }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            if (this.Manager.Items.ContainsKey("ResourceManager"))
            {
                resourceManager = this.Manager.Items["ResourceManager"] as ScriptResourceManager;
            }
        }

        [ToolTipText("Effect_Pool_Value_Pool")]
        public EffectPool Pool
        {
            get;
            private set;
        }

        [ToolTipText("Effect_Pool_Value_Path")]
        public string Path
        {
            private get;
            set;
        }

        [ToolTipText("Effect_Pool_Value_Count")]
        public int Count
        {
            private get;
            set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetValue(nameof(Path));
            SetValue(nameof(Count));

            if (resourceManager != null)
            {
                Pool = new EffectPool(resourceManager.Device, Path, Count, resourceManager);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
