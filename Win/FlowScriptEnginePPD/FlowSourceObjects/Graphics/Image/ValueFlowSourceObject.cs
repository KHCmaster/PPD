using FlowScriptEngine;
using PPDCoreModel;
using PPDFramework;
using SharpDX;
using System.Collections.Generic;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.Image
{
    [ToolTipText("Image_Value_Summary")]
    public partial class ValueFlowSourceObject : ExecutableFlowSourceObject
    {
        private ScriptResourceManager resourceManager;

        public override string Name
        {
            get { return "PPD.Graphics.Image.Value"; }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (this.Manager.Items.ContainsKey("ResourceManager"))
            {
                resourceManager = this.Manager.Items["ResourceManager"] as ScriptResourceManager;
            }
        }

        [ToolTipText("Image_Value_Object")]
        public PictureObject Object
        {
            get;
            private set;
        }

        [ToolTipText("Image_Value_Path")]
        public string Path
        {
            private get;
            set;
        }

        [ToolTipText("Image_Value_Position")]
        public Vector2 Position
        {
            private get;
            set;
        }

        [ToolTipText("Image_Value_IsCenter")]
        public bool IsCenter
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Path));
            SetValue(nameof(Position));
            SetValue(nameof(IsCenter));

            if (resourceManager != null)
            {
                Object = (PictureObject)resourceManager.GetResource(Path, PPDCoreModel.Data.ResourceKind.Image, new Dictionary<string, object>
                {
                    {"Position",Position},                    {"IsCenter",IsCenter}                });
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
