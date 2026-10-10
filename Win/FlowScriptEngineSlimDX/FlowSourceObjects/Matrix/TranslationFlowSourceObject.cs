using PPDMath;
using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Matrix
{
    [ToolTipText("Matrix_Translation_Summary")]
    public partial class TranslationFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Matrix.Translation"; }
        }

        [ToolTipText("Matrix_Translation_Translation_Summary")]
        public System.Numerics.Vector3 Translation
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_Translation_Value_Summary")]
        public System.Numerics.Matrix4x4 Value
        {
            get
            {
                SetValue(nameof(Translation));
                return MathCompat.Translation(Translation);
            }
        }
    }
}
