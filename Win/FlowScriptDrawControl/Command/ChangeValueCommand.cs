using FlowScriptDrawControl.Model;

namespace FlowScriptDrawControl.Command
{
    class ChangeValueCommand : CommandBase
    {
        private readonly string newValue;
        private string oldValue;
        private readonly Item item;

        public ChangeValueCommand(Item item, string newValue)
        {
            this.item = item;
            this.newValue = newValue;
        }

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
