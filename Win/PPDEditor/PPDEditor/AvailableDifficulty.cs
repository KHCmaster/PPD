using System;

namespace PPDEditor
{
    [Flags]
    public enum AvailableDifficulty
    {
        None = 0,
        Base = 1,
        Easy = 2,
        Normal = 4,
        Hard = 8,
        Extreme = 16
    }
}
