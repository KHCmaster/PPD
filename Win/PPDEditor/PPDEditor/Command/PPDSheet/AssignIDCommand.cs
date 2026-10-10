namespace PPDEditor.Command.PPDSheet
{
    class AssignIDCommand(Mark mark, uint id) : Command
    {
        readonly uint newID = id;
        readonly uint prevID = mark.ID;
        readonly Mark mark = mark;

        public override void Execute()
        {
            mark.ID = newID;
        }

        public override void Undo()
        {
            mark.ID = prevID;
        }

        public override CommandType CommandType
        {
            get { return CommandType.ID; }
        }
    }
}
