using FlowScriptEngine;
using PPDFramework;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics
{
    [ToolTipText("Graphics_GameInterface_Summary")]
    public partial class GameInterfaceFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Graphics.GameInterface"; }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (this.Manager.Items.TryGetValue("GameInterface", out object obj))
            {
                Value = obj as GameComponent;
            }
            if (this.Manager.Items.TryGetValue("HoldLayer", out obj))
            {
                HoldLayer = obj as GameComponent;
            }
            if (this.Manager.Items.TryGetValue("MarkEffectLayer", out obj))
            {
                MarkEffectLayer = obj as GameComponent;
            }
            if (this.Manager.Items.TryGetValue("ConnectLayer", out obj))
            {
                ConnectLayer = obj as GameComponent;
            }
            if (this.Manager.Items.TryGetValue("MarkLayer", out obj))
            {
                MarkLayer = obj as GameComponent;
            }
            if (this.Manager.Items.TryGetValue("ComboLayer", out obj))
            {
                ComboLayer = obj as GameComponent;
            }
            if (this.Manager.Items.TryGetValue("SlideLayer", out obj))
            {
                SlideLayer = obj as GameComponent;
            }
        }

        [ToolTipText("Graphics_GameInterface_Value")]
        public GameComponent Value
        {
            get;
            private set;
        }

        [ToolTipText("Graphics_GameInterface_HoldLayer")]
        public GameComponent HoldLayer
        {
            get;
            private set;
        }

        [ToolTipText("Graphics_GameInterface_MarkEffectLayer")]
        public GameComponent MarkEffectLayer
        {
            get;
            private set;
        }

        [ToolTipText("Graphics_GameInterface_ConnectLayer")]
        public GameComponent ConnectLayer
        {
            get;
            private set;
        }

        [ToolTipText("Graphics_GameInterface_MarkLayer")]
        public GameComponent MarkLayer
        {
            get;
            private set;
        }

        [ToolTipText("Graphics_GameInterface_ComboLayer")]
        public GameComponent ComboLayer
        {
            get;
            private set;
        }

        [ToolTipText("Graphics_GameInterface_SlideLayer")]
        public GameComponent SlideLayer
        {
            get;
            private set;
        }
    }
}
