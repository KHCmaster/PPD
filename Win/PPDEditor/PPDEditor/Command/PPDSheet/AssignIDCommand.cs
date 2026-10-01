namespace PPDEditor.Command.PPDSheet
{
    class AssignIDCommand : Command
    {
        readonly uint newID;
        readonly uint prevID;
        readonly Mark mark;

        public AssignIDCommand(Mark mark, uint id)
        {
            newID = id;
            this.mark = mark;
            prevID = mark.ID;
        }

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
