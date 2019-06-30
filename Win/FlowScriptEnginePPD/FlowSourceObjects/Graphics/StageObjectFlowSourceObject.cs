using FlowScriptEngine;
using PPDCoreModel;
using PPDCoreModel.Data;
using PPDFramework;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics
{
    [ToolTipText("StageObject_Summary")]
    public partial class StageObjectFlowSourceObject : FlowSourceObjectBase
    {
        private StageManager stageManager;

        public override string Name
        {
            get { return "PPD.Graphics.StageObject"; }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            if (this.Manager.Items.ContainsKey("StageManager"))
            {
                stageManager = this.Manager.Items["StageManager"] as StageManager;
            }
        }

        [ToolTipText("StageObject_Layer")]
        public LayerType Layer
        {
            private get;
            set;
        }

        [ToolTipText("StageObject_Value")]
        public GameComponent Value
        {
            get
            {
                SetValue(nameof(Layer));
                if (stageManager != null)
                {
                    return stageManager[Layer];
                }
                return null;
            }
        }
    }
}
