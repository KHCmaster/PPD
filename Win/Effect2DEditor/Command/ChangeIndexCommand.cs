using Effect2D;

namespace Effect2DEditor.Command
{
    class ChangeIndexCommand : CommandBase
    {
        int beforeindex;
        int index;
        public ChangeIndexCommand(EffectManager manager, string exp, int index, int beforeindex)
            : base(manager, exp)
        {
            this.index = index;
            this.beforeindex = beforeindex;
        }

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
