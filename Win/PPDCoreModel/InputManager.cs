using PPDCoreModel.Data;
using System;

namespace PPDCoreModel
{
    public class InputManager
    {
        bool forceEnabled;
        bool[] disabled;

        public bool Modified
        {
            get;
            private set;
        }

        public InputManager()
        {
            disabled = new bool[Enum.GetValues(typeof(MarkType)).Length];
            Initialize();
        }

        public void Initialize()
        {
            forceEnabled = false;
            Array.Clear(disabled, 0, disabled.Length);
            Modified = false;
        }

        public void Disable(MarkType button, bool isDisabled)
        {
            if (disabled[(int)button] != isDisabled)
            {
                disabled[(int)button] = isDisabled;
                Modified = true;
            }
        }

        public void ForceEnable()
        {
            forceEnabled = true;
        }

        public bool IsDisabled(MarkType markType)
        {
            if (forceEnabled)
            {
                return false;
            }
            return disabled[(int)markType];
        }
    }
}
