using FlowScriptEngine;
using PPDFramework;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.Polygon
{
    [ToolTipText("Polygon_SetVertexCount_Summary")]
    public partial class SetVertexCountFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPD.Graphics.Polygon.SetVertexCount"; }
        }

        [ToolTipText("Polygon_SetVertexCount_Object")]
        public PolygonObject Object
        {
            private get;
            set;
        }

        [ToolTipText("Polygon_SetVertexCount_VertexCount")]
        public int VertexCount
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Object));
            SetValue(nameof(VertexCount));
            if (Object != null)
            {
                Object.VertexCount = VertexCount;
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
