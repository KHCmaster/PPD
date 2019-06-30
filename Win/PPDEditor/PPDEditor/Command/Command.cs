namespace PPDEditor.Command
{
    public abstract class Command
    {
        public abstract void Execute();
        public abstract void Undo();
        public abstract CommandType CommandType { get; }
    }
}
