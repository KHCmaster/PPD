using FlowScriptEngine;
using PPDCoreModel;
using PPDFramework.Vertex;
using System.Collections.Generic;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.VertexBuffer
{
    [ToolTipText("VertexBuffer_Value_Summary")]
    public partial class ValueFlowSourceObject : ExecutableFlowSourceObject
    {
        private ScriptResourceManager resourceManager;

        public override string Name
        {
            get { return "PPD.Graphics.VertexBuffer.Value"; }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (this.Manager.Items.ContainsKey("ResourceManager"))
            {
                resourceManager = this.Manager.Items["ResourceManager"] as ScriptResourceManager;
            }
        }

        [ToolTipText("VertexBuffer_Value_VertexCount")]
        public int VertexCount
        {
            private get;
            set;
        }

        [ToolTipText("VertexBuffer_Value_Object")]
        public VertexInfo Object
        {
            get;
            private set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(VertexCount));
            if (VertexCount > 0)
            {
                Object = (VertexInfo)resourceManager.GetResource("", PPDCoreModel.Data.ResourceKind.VertexBuffer, new Dictionary<string, object>(){
                    { nameof(VertexCount), VertexCount }
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
