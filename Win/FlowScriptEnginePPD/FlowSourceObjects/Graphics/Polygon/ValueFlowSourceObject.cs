using FlowScriptEngine;
using PPDCoreModel;
using PPDFramework;
using PPDFramework.Vertex;
using System.Collections.Generic;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.Polygon
{
    [ToolTipText("Polygon_Value_Summary")]
    public partial class ValueFlowSourceObject : ExecutableFlowSourceObject
    {
        private ScriptResourceManager resourceManager;

        public override string Name
        {
            get { return "PPD.Graphics.Polygon.Value"; }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (this.Manager.Items.ContainsKey("ResourceManager"))
            {
                resourceManager = this.Manager.Items["ResourceManager"] as ScriptResourceManager;
            }
        }

        [ToolTipText("Polygon_Value_Object")]
        public PolygonObject Object
        {
            get;
            private set;
        }

        [ToolTipText("Polygon_Value_Path")]
        public string Path
        {
            private get;
            set;
        }

        [ToolTipText("Polygon_Value_VertexBuffer")]
        public VertexInfo VertexBuffer
        {
            private get;
            set;
        }

        [ToolTipText("Polygon_Value_PrimitiveType")]
        public PPDFramework.PrimitiveType PrimitiveType
        {
            private get;
            set;
        }

        [ToolTipText("Polygon_Value_PrimitiveCount")]
        public int PrimitiveCount
        {
            private get;
            set;
        }

        [ToolTipText("Polygon_Value_StartIndex")]
        public int StartIndex
        {
            private get;
            set;
        }

        [ToolTipText("Polygon_Value_VertexCount")]
        public int VertexCount
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Path));
            SetValue(nameof(VertexBuffer));

            if (resourceManager != null && VertexBuffer != null)
            {
                SetValue(nameof(PrimitiveType));
                SetValue(nameof(PrimitiveCount));
                SetValue(nameof(StartIndex));
                SetValue(nameof(VertexCount));
                Object = (PolygonObject)resourceManager.GetResource(Path, PPDCoreModel.Data.ResourceKind.Polygon, new Dictionary<string, object>
                {
                    { "Vertex", VertexBuffer },
                    { "PrimitiveType", PrimitiveType },
                    { "PrimitiveCount", PrimitiveCount },
                    { "StartIndex", StartIndex },
                    { "VertexCount", VertexCount },
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
