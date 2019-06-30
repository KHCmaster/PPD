using System;

namespace PPDEditor.Command
{
    [Flags]
    public enum CommandType
    {
        None = 0,
        Time = 1,
        Pos = 2,
        ID = 4,
    }
}
