using Effect2D;

namespace Effect2DEditor.Command
{
    class ClearEffectCommand(EffectManager manager, string exp) : CommandBase(manager, exp)
    {
        IEffect[] array;

        public override void Undo()
        {
            manager.Effects.AddRange(array);
        }

        public override void Execute()
        {
            array = new IEffect[manager.Effects.Count];
            manager.Effects.CopyTo(array);
            manager.Effects.Clear();
        }
    }
}
