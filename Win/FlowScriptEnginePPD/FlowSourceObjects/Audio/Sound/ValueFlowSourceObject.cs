using FlowScriptEngine;
using PPDCoreModel;
using PPDSound;
using System.Collections.Generic;

namespace FlowScriptEnginePPD.FlowSourceObjects.Audio.Sound
{
    [ToolTipText("Sound_Value_Summary")]
    public partial class ValueFlowSourceObject : ExecutableFlowSourceObject
    {
        private ScriptResourceManager resourceManager;

        public override string Name
        {
            get { return "PPD.Audio.Sound.Value"; }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (this.Manager.Items.ContainsKey("ResourceManager"))
            {
                resourceManager = this.Manager.Items["ResourceManager"] as ScriptResourceManager;
            }
        }

        [ToolTipText("Sound_Value_Object")]
        public SoundResource Object
        {
            get;
            private set;
        }

        [ToolTipText("Sound_Value_Path")]
        public string Path
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Path));

            if (resourceManager != null)
            {
                Object = (SoundResource)resourceManager.GetResource(Path, PPDCoreModel.Data.ResourceKind.Sound, new Dictionary<string, object>());
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
