using FlowScriptDrawControl.Model;

namespace FlowScriptDrawControl.Command
{
    class ChangeValueCommand(Item item, string newValue) : CommandBase
    {
        private readonly string newValue = newValue;
        private string oldValue;
        private readonly Item item = item;

        public override void Execute()
        {
            oldValue = item.Value;
            item.Value = newValue;
        }

        public override void Undo()
        {
            item.Value = oldValue;
        }
    }
}
