using PPDMath;
namespace FlowScriptEngineSlimDX.FlowSourceObjects.Matrix
{
    public partial class IsTypeFlowSourceObject : TemplateIsTypeFlowSourceObject<System.Numerics.Matrix4x4>
    {
        public override string Name
        {
            get { return "Matrix.IsType"; }
        }
    }
}
