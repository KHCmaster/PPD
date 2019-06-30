using FlowScriptEngine;
using PPDCoreModel;
using PPDFramework;
using SharpDX;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.Text
{
    [ToolTipText("Text_Value_Summary")]
    public partial class ValueFlowSourceObject : ExecutableFlowSourceObject
    {
        private ScriptResourceManager resourceManager;

        public override string Name
        {
            get { return "PPD.Graphics.Text.Value"; }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (this.Manager.Items.ContainsKey("ResourceManager"))
            {
                resourceManager = this.Manager.Items["ResourceManager"] as ScriptResourceManager;
            }
        }

        [ToolTipText("Text_Value_Object")]
        public TextureString Object
        {
            get;
            private set;
        }

        [ToolTipText("Text_Value_Height")]
        public int Height
        {
            private get;
            set;
        }

        [ToolTipText("Text_Value_Position")]
        public Vector2 Position
        {
            private get;
            set;
        }

        [ToolTipText("Text_Value_Color")]
        public Color4 Color
        {
            private get;
            set;
        }

        [ToolTipText("Text_Value_Text")]
        public string Text
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Height));
            SetValue(nameof(Position));
            SetValue(nameof(Text));
            SetValue(nameof(Color));

            if (Text == null)
            {
                Text = "";
            }

            if (resourceManager != null)
            {
                Object = new TextureString(resourceManager.Device, Text, Height, Color)
                {
                    Position = Position
                };
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
