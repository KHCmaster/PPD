using PPDFramework;
using System.Collections.Generic;

namespace KeyConfiger
{
    public class KeyConfig
    {
        Dictionary<ButtonType, int> keyMap;
        Dictionary<ButtonType, int> buttonMap;

        public KeyConfig()
        {
            buttonMap = new Dictionary<ButtonType, int>();
            keyMap = new Dictionary<ButtonType, int>();
            foreach (ButtonType type in ButtonUtility.Array)
            {
                buttonMap.Add(type, 0);
                keyMap.Add(type, 0);
            }

            Name = "default";
        }

        public string Name
        {
            get;
            set;
        }

        public void SetKeyMap(ButtonType buttonType, int value)
        {
            keyMap[buttonType] = value;
        }

        public void SetButtonMap(ButtonType buttonType, int value)
        {
            buttonMap[buttonType] = value;
        }

        public int GetKeyMap(ButtonType buttonType)
        {
            return keyMap[buttonType];
        }

        public int GetButtonMap(ButtonType buttonType)
        {
            return buttonMap[buttonType];
        }

        public KeyConfig Clone()
        {
            var ret = new KeyConfig();
            foreach (ButtonType type in ButtonUtility.Array)
            {
                ret.SetKeyMap(type, GetKeyMap(type));
                ret.SetButtonMap(type, GetButtonMap(type));
            }
            return ret;
        }
    }
}
