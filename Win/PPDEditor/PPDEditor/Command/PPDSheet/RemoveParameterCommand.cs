namespace PPDEditor.Command.PPDSheet
{
    class RemoveParameterCommand : Command
    {
        private Mark mk;
        private string key;
        private string prevValue;
        public RemoveParameterCommand(Mark mk, string key)
        {
            this.mk = mk;
            this.key = key;
            prevValue = mk[key];
        }
        public override void Execute()
        {
            if (prevValue != null)
            {
                mk.RemoveParameter(key);
            }
        }

        public override void Undo()
        {
            if (prevValue != null)
            {
                mk[key] = prevValue;
            }
        }

        public override CommandType CommandType
        {
            get { return CommandType.None; }
        }
    }
}
