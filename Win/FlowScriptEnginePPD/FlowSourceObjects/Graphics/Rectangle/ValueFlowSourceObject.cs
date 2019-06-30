using PPDCoreModel;
using PPDFramework;
using SharpDX;
using System.Collections.Generic;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.Rectangle
{
    [ToolTipText("Graphics_Rectangle_Value_Summary")]
    public partial class ValueFlowSourceObject : ExecutableFlowSourceObject
    {
        private ScriptResourceManager resourceManager;

        public override string Name
        {
            get { return "PPD.Graphics.Rectangle.Value"; }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (this.Manager.Items.ContainsKey("ResourceManager"))
            {
                resourceManager = this.Manager.Items["ResourceManager"] as ScriptResourceManager;
            }
        }

        [ToolTipText("Graphics_Rectangle_Value_Width")]
        public float Width
        {
            private get;
            set;
        }

        [ToolTipText("Graphics_Rectangle_Value_Height")]
        public float Height
        {
            private get;
            set;
        }

        [ToolTipText("Graphics_Rectangle_Value_Color")]
        public Color4 Color
        {
            private get;
            set;
        }

        [ToolTipText("Graphics_Rectangle_Value_Object")]
        public RectangleComponent Object
        {
            get;
            private set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            if (resourceManager != null)
            {
                SetValue(nameof(Width));
                SetValue(nameof(Height));
                SetValue(nameof(Color));
                Object = (RectangleComponent)resourceManager.GetResource("", PPDCoreModel.Data.ResourceKind.Rectangle, new Dictionary<string, object>
                {
                    {"Color",Color}                });
                Object.RectangleHeight = Height;
                Object.RectangleWidth = Width;
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
