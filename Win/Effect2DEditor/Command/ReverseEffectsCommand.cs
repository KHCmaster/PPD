using Effect2D;

namespace Effect2DEditor.Command
{
    class ReverseEffectsCommand : CommandBase
    {
        public ReverseEffectsCommand(EffectManager manager, string exp)
            : base(manager, exp)
        {

        }

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
