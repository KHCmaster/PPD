namespace FlowScriptDrawControl.Command
{
    public abstract class CommandBase
    {
        public abstract void Execute();
        public abstract void Undo();
    }
}
