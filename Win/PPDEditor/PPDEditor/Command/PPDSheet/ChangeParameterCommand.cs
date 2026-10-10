namespace PPDEditor.Command.PPDSheet
{
    class ChangeParameterCommand(Mark mk, string key, string value) : Command
    {
        private readonly Mark mk = mk;
        private readonly string key = key;
        private readonly string value = value;
        private readonly string prevValue = mk[key];

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
