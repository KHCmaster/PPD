using FlowScriptEngine;
using PPDFramework;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.Polygon
{
    [ToolTipText("Polygon_SetPrimitiveType_Summary")]
    public partial class SetPrimitiveTypeFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPD.Graphics.Polygon.SetPrimitiveType"; }
        }

        [ToolTipText("Polygon_SetPrimitiveType_Object")]
        public PolygonObject Object
        {
            private get;
            set;
        }

        [ToolTipText("Polygon_SetPrimitiveType_PrimitiveType")]
        public PPDFramework.PrimitiveType PrimitiveType
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Object));
            SetValue(nameof(PrimitiveType));
            if (Object != null)
            {
                Object.PrimitiveType = PrimitiveType;
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
