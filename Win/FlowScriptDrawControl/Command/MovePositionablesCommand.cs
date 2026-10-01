using FlowScriptDrawControl.Model;
using System.Linq;
using System.Windows;

namespace FlowScriptDrawControl.Command
{
    class MovePositionablesCommand(Positionable[] positionables, Point[] newPoses) : CommandBase
    {
        readonly Positionable[] positionables = positionables;
        readonly Point[] oldPoses = [.. positionables.Select(s => s.Position)];

        public Point[] NewPoses
        {
            get;
            set;
        } = newPoses;

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
