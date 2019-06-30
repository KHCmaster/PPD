using FlowScriptEngine;
using PPDCoreModel;
using PPDFramework;
using SharpDX;
using System.Collections.Generic;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.Effect
{
    [ToolTipText("Effect_Value_Summary")]
    public partial class ValueFlowSourceObject : ExecutableFlowSourceObject
    {
        private ScriptResourceManager resourceManager;

        public override string Name
        {
            get { return "PPD.Graphics.Effect.Value"; }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (this.Manager.Items.ContainsKey("ResourceManager"))
            {
                resourceManager = this.Manager.Items["ResourceManager"] as ScriptResourceManager;
            }
        }

        [ToolTipText("Effect_Value_Object")]
        public EffectObject Object
        {
            get;
            set;
        }

        [ToolTipText("Effect_Value_Path")]
        public string Path
        {
            private get;
            set;
        }

        [ToolTipText("Effect_Value_Position")]
        public Vector2 Position
        {
            private get;
            set;
        }

        [ToolTipText("Effect_Value_PlayState")]
        public Effect2D.EffectManager.PlayState PlayState
        {
            get
            {
                SetValue(nameof(Object));
                return Object == null ? Effect2D.EffectManager.PlayState.Stop : Object.PlayState;
            }
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Path));
            SetValue(nameof(Position));

            if (resourceManager != null)
            {
                Object = (EffectObject)resourceManager.GetResource(Path, PPDCoreModel.Data.ResourceKind.Effect, new Dictionary<string, object>
                {
                    {"Position",Position}
                });
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
