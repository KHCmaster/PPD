using PPDCoreModel;
using PPDFramework;
using SharpDX;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.Effect.Pool
{
    [ToolTipText("Effect_Pool_Use_Summary")]
    public partial class UseFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPD.Graphics.Effect.Pool.Use"; }
        }

        [ToolTipText("Effect_Pool_Use_Position")]
        public Vector2 Position
        {
            private get;
            set;
        }

        [ToolTipText("Effect_Pool_Use_Pool")]
        public EffectPool Pool
        {
            private get;
            set;
        }

        [ToolTipText("Effect_Pool_Use_Object")]
        public EffectObject Object
        {
            get;
            private set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetValue(nameof(Position));
            SetValue(nameof(Pool));
            if (Pool != null)
            {
                Object = Pool.Use(Position);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
