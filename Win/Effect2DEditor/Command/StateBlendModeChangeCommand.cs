using Effect2D;

namespace Effect2DEditor.Command
{
    class StateBlendModeChangeCommand : CommandBase
    {
        SelectedManager SManager;
        BlendMode previousBlend;
        BlendMode blendMode;

        public StateBlendModeChangeCommand(EffectManager manager, string exp, SelectedManager SManager, BlendMode blendMode)
            : base(manager, exp)
        {
            this.SManager = SManager;
            this.blendMode = blendMode;
        }

        public BlendMode BlendMode
        {
            get { return blendMode; }
            set { this.blendMode = value; }
        }

        public override void Undo()
        {
            EffectStateStructure state = SManager.State;
            state.BlendMode = previousBlend;
        }

        public override void Execute()
        {
            EffectStateStructure state = SManager.State;
            previousBlend = state.BlendMode;
            state.BlendMode = blendMode;
        }

        public void SpecialExecute()
        {
            EffectStateStructure state = SManager.State;
            state.BlendMode = blendMode;
        }
    }
}
