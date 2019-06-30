using FlowScriptEngine;
using PPDCoreModel;
using PPDFramework;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.Sprite
{
    [ToolTipText("Sprite_Value_Summary")]
    public partial class ValueFlowSourceObject : ExecutableFlowSourceObject
    {
        private ScriptResourceManager resourceManager;

        public override string Name
        {
            get { return "PPD.Graphics.Sprite.Value"; }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (this.Manager.Items.ContainsKey("ResourceManager"))
            {
                resourceManager = this.Manager.Items["ResourceManager"] as ScriptResourceManager;
            }
        }

        [ToolTipText("Sprite_Value_Object")]
        public SpriteObject Object
        {
            get;
            private set;
        }

        public override void In(FlowEventArgs e)
        {
            if (resourceManager != null)
            {
                Object = new SpriteObject(resourceManager.Device);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
