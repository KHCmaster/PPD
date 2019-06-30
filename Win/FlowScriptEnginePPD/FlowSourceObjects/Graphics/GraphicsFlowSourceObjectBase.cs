using PPDFramework;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics
{
    public abstract class GraphicsFlowSourceObjectBase : ExecutableFlowSourceObject
    {
        [ToolTipText("Graphics_Object")]
        public GameComponent Object
        {
            protected get;
            set;
        }
    }
}
