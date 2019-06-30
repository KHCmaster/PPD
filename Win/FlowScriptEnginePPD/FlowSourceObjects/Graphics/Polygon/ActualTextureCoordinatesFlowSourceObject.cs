using FlowScriptEngine;
using PPDFramework;
using SharpDX;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.Polygon
{
    [ToolTipText("Polygon_ActualTextureCoordinates_Summary")]
    public partial class ActualTextureCoordinatesFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Graphics.Polygon.ActualTextureCoordinates"; }
        }

        [ToolTipText("Polygon_ActualTextureCoordinates_Object")]
        public PolygonObject Object
        {
            private get;
            set;
        }

        [ToolTipText("Polygon_ActualTextureCoordinates_TextureCoordinates")]
        public Vector2 TextureCoordinates
        {
            private get;
            set;
        }

        [ToolTipText("Polygon_ActualTextureCoordinates_Value")]
        public Vector2 Value
        {
            get
            {
                SetValue(nameof(Object));
                SetValue(nameof(TextureCoordinates));
                if (Object == null)
                {
                    return TextureCoordinates;
                }
                return Object.GetActualUV(TextureCoordinates);
            }
        }
    }
}
