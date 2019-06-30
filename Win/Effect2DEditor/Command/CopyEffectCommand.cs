using Effect2D;

namespace Effect2DEditor.Command
{
    class CopyEffectCommand : CommandBase
    {
        SelectedManager SManager;
        public CopyEffectCommand(EffectManager manager, string exp, SelectedManager SManager)
            : base(manager, exp)
        {
            this.SManager = SManager;
        }

        public override void Undo()
        {
            manager.Effects.RemoveAt(manager.Effects.Count - 1);
        }

        public override void Execute()
        {
            var effect = (IEffect)SManager.Effect.Clone();
            manager.Effects.Add(effect);
        }
    }
}
