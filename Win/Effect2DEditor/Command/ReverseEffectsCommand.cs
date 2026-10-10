using Effect2D;

namespace Effect2DEditor.Command
{
    class ReverseEffectsCommand(EffectManager manager, string exp) : CommandBase(manager, exp)
    {
        public override void Undo()
        {
            manager.Effects.Reverse();
        }

        public override void Execute()
        {
            manager.Effects.Reverse();
        }
    }
}
