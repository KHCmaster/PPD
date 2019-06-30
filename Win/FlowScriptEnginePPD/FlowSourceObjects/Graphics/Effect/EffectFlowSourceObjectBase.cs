using PPDFramework;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.Effect
{
    public abstract class EffectFlowSourceObjectBase : ExecutableFlowSourceObject
    {
        [ToolTipText("Effect_Object")]
        public EffectObject Object
        {
            protected get;
            set;
        }
    }
}
