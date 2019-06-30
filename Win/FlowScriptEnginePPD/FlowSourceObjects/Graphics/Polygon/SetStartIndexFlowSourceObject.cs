using FlowScriptEngine;
using PPDFramework;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.Polygon
{
    [ToolTipText("Polygon_SetStartIndex_Summary")]
    public partial class SetStartIndexFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPD.Graphics.Polygon.SetStartIndex"; }
        }

        [ToolTipText("Polygon_SetStartIndex_Object")]
        public PolygonObject Object
        {
            private get;
            set;
        }

        [ToolTipText("Polygon_SetStartIndex_StartIndex")]
        public int StartIndex
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Object));
            SetValue(nameof(StartIndex));
            if (Object != null)
            {
                Object.StartIndex = StartIndex;
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
