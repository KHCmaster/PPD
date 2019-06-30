namespace FlowScriptControl.Classes
{
    public abstract class NodeFilterInfoBase
    {
        public abstract string Name { get; }
        public abstract bool IsHide(FlowSourceDumper dumper);
    }
}
