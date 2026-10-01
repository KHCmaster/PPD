using Effect2D;


namespace Effect2DEditor.Command
{
    public abstract class CommandBase(EffectManager manager, string exp)
    {
        public string Explaination { get; private set; } = exp;
        protected EffectManager manager = manager;

        public abstract void Execute();
        public abstract void Undo();
    }
}
