using FlowScriptEngine;
using PPDFramework;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.Polygon
{
    [ToolTipText("Polygon_SetPrimitiveCount_Summary")]
    public partial class SetPrimitiveCountFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPD.Graphics.Polygon.SetPrimitiveCount"; }
        }

        [ToolTipText("Polygon_SetPrimitiveCount_Object")]
        public PolygonObject Object
        {
            private get;
            set;
        }

        [ToolTipText("Polygon_SetPrimitiveCount_PrimitiveCount")]
        public int PrimitiveCount
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Object));
            SetValue(nameof(PrimitiveCount));
            if (Object != null)
            {
                Object.PrimitiveCount = PrimitiveCount;
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
