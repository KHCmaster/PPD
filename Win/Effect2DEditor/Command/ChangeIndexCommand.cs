using Effect2D;

namespace Effect2DEditor.Command
{
    class ChangeIndexCommand(EffectManager manager, string exp, int index, int beforeindex) : CommandBase(manager, exp)
    {
        readonly int beforeindex = beforeindex;
        readonly int index = index;

        public override void Undo()
        {
            IEffect temp = manager.Effects[index < beforeindex ? index + 1 : index];
            manager.Effects.RemoveAt(index < beforeindex ? index + 1 : index);
            manager.Effects.Insert(beforeindex, temp);
        }

        public override void Execute()
        {
            IEffect temp = manager.Effects[beforeindex];
            manager.Effects.RemoveAt(beforeindex);
            manager.Effects.Insert(index < beforeindex ? index + 1 : index, temp);
        }
    }
}
