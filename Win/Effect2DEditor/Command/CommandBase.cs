using Effect2D;


namespace Effect2DEditor.Command
{
    public abstract class CommandBase
    {
        public string Explaination { get; private set; }
        protected EffectManager manager;
        protected CommandBase(EffectManager manager, string exp)
        {
            this.manager = manager;
            this.Explaination = exp;
        }
        public abstract void Execute();
        public abstract void Undo();
    }
}
