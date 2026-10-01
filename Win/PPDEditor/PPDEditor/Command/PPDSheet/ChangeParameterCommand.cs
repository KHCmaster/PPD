namespace PPDEditor.Command.PPDSheet
{
    class ChangeParameterCommand : Command
    {
        private readonly Mark mk;
        private readonly string key;
        private readonly string value;
        private readonly string prevValue;
        public ChangeParameterCommand(Mark mk, string key, string value)
        {
            this.mk = mk;
            this.key = key;
            this.value = value;
            prevValue = mk[key];
        }
        public override void Execute()
        {
            mk[key] = value;
        }

        public override void Undo()
        {
            if (prevValue != null)
            {
                mk[key] = value;
            }
            else
            {
                mk.RemoveParameter(key);
            }
        }

        public override CommandType CommandType
        {
            get { return CommandType.None; }
        }
    }
}
