using FlowScriptEngine;
using PPDEditorCommon;

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Layer
{
    [ToolTipText("Layer_All_Summary")]
    public partial class AllFlowSourceObject : FlowSourceObjectBase
    {
        private ILayer[] all;

        public override string Name
        {
            get { return "PPDEditor.Layer.All"; }
        }

        protected override void OnInitialize()
        {
            if (Manager.Items.ContainsKey("AllLayers"))
            {
                all = (ILayer[])Manager.Items["AllLayers"];
            }
        }

        [ToolTipText("Layer_All_All")]
        public object[] All
        {
            get
            {
                return all;
            }
        }
    }
}
