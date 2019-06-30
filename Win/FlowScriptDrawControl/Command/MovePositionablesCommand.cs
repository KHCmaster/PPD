using FlowScriptDrawControl.Model;
using System.Linq;
using System.Windows;

namespace FlowScriptDrawControl.Command
{
    class MovePositionablesCommand : CommandBase
    {
        Positionable[] positionables;
        Point[] oldPoses;

        public Point[] NewPoses
        {
            get;
            set;
        }

        public MovePositionablesCommand(Positionable[] positionables, Point[] newPoses)
        {
            this.positionables = positionables;
            NewPoses = newPoses;
            oldPoses = positionables.Select(s => s.Position).ToArray();
        }

        public override void Execute()
        {
            for (int i = 0; i < positionables.Length; i++)
            {
                positionables[i].Position = NewPoses[i];
            }
        }

        public override void Undo()
        {
            for (int i = 0; i < positionables.Length; i++)
            {
                positionables[i].Position = oldPoses[i];
            }
        }
    }
}
