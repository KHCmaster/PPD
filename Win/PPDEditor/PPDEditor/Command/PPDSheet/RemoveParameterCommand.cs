namespace PPDEditor.Command.PPDSheet
{
    class RemoveParameterCommand(Mark mk, string key) : Command
    {
        private readonly Mark mk = mk;
        private readonly string key = key;
        private readonly string prevValue = mk[key];

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
