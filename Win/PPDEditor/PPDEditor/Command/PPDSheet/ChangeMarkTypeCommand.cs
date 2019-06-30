using PPDFramework;
using System.Collections.Generic;

namespace PPDEditor.Command.PPDSheet
{
    class ChangeMarkTypeCommand : Command
    {
        SortedList<float, Mark>[] data;
        Mark mk;
        ButtonType newType;
        ButtonType prevType;
        public ChangeMarkTypeCommand(SortedList<float, Mark>[] data, Mark mk, ButtonType newType)
        {
            this.data = data;
            this.mk = mk;
            this.newType = newType;

            prevType = mk.Type;
        }

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
