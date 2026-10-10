using PPDFramework;
using System.Collections.Generic;

namespace PPDEditor.Command.PPDSheet
{
    class ChangeMarkTypeCommand(SortedList<float, Mark>[] data, Mark mk, ButtonType newType) : Command
    {
        readonly SortedList<float, Mark>[] data = data;
        readonly Mark mk = mk;
        readonly ButtonType newType = newType;
        readonly ButtonType prevType = mk.Type;

        public override void Execute()
        {
            data[(int)prevType].Remove(mk.Time);
            mk.Type = newType;
            data[(int)newType].Add(mk.Time, mk);
        }

        public override void Undo()
        {
            data[(int)newType].Remove(mk.Time);
            mk.Type = prevType;
            data[(int)prevType].Add(mk.Time, mk);
        }

        public override CommandType CommandType
        {
            get { return CommandType.Time; }
        }
    }
}
