using Effect2D;

namespace Effect2DEditor.Command
{
    class CopyEffectCommand(EffectManager manager, string exp, SelectedManager SManager) : CommandBase(manager, exp)
    {
        readonly SelectedManager SManager = SManager;

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
