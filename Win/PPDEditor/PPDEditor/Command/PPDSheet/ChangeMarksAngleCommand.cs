namespace PPDEditor.Command.PPDSheet
{
    class ChangeMarksAngleCommand : Command
    {
        Mark[] marks;
        float[] previousAngles;
        float angle;
        public ChangeMarksAngleCommand(Mark[] marks, float angle)
        {
            this.marks = marks;
            this.angle = angle;
            previousAngles = new float[marks.Length];
            for (int i = 0; i < previousAngles.Length; i++)
            {
                previousAngles[i] = marks[i].Rotation;
            }
        }

        public Mark[] Marks
        {
            get
            {
                return marks;
            }
        }

        public float Angle
        {
            get { return angle; }
            set { angle = value; }
        }

        public override void Execute()
        {
            for (int i = 0; i < marks.Length; i++)
            {
                marks[i].Rotation = angle;
            }
        }

        public override void Undo()
        {
            for (int i = 0; i < marks.Length; i++)
            {
                marks[i].Rotation = previousAngles[i];
            }
        }

        public override CommandType CommandType
        {
            get { return CommandType.Pos; }
        }
    }
}
