using PPDFramework;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace PPDEditor
{
    public class KeyConfigManager
    {
        List<KeyConfig> keyConfigs;
        int currentConfigIndex;
        public KeyConfigManager()
        {
            keyConfigs = new List<KeyConfig>();
            currentConfigIndex = 0;
        }

        public void Load(string fileName)
        {
            if (File.Exists(fileName))
            {
                try
                {
                    ReadKeySettingFromXml(fileName);
                }
                catch
                {
                    ReadKeySettingFromIni(fileName);
                }
            }
            else
            {
                keyConfigs.Add(new KeyConfig());
            }
        }

        public KeyConfig CurrentConfig
        {
            get
            {
                return keyConfigs[currentConfigIndex];
            }
        }

        public KeyConfig[] Configs
        {
            get
            {
                return keyConfigs.ToArray();
            }
        }

        public KeyConfig this[int index]
        {
            get
            {
                return keyConfigs[index];
            }
        }

        public int CurrentConfigIndex
        {
            get
            {
                return currentConfigIndex;
            }
            set
            {
                if (value >= 0 && value < keyConfigs.Count)
                {
                    currentConfigIndex = value;
                }
            }
        }

        private void ReadKeySettingFromXml(string fileName)
        {
            var reader = XmlReader.Create(fileName, new XmlReaderSettings());
            while (reader.Read())
            {
                if (reader.IsStartElement())
                {
                    switch (reader.LocalName)
                    {
                        case "KeyConfigs":
                            keyConfigs.Clear();
                            break;
                        case "KeyConfig":
                            var keyConfig = new KeyConfig
                            {
                                Name = reader.GetAttribute("Name")
                            };
                            keyConfigs.Add(keyConfig);
                            ReadKeySetting(reader.ReadSubtree(), keyConfig);
                            break;
                    }
                }
            }
            reader.Close();

            if (keyConfigs.Count == 0)
            {
                keyConfigs.Add(new KeyConfig());
            }
        }

        private void ReadKeySetting(XmlReader reader, KeyConfig keyConfig)
        {
            while (reader.Read())
            {
                if (reader.IsStartElement())
                {
                    switch (reader.LocalName)
                    {
                        case "Config":
                            var key = (Key)int.Parse(reader.GetAttribute("Key"));
                            int button = int.Parse(reader.GetAttribute("Button")), index = FindIndex(reader.GetAttribute("Type"));
                            if (index >= 0)
                            {
                                keyConfig.SetKeyMap(ButtonUtility.Array[index], key);
                                keyConfig.SetButtonMap(ButtonUtility.Array[index], button);
                            }
                            break;
                    }
                }
            }
            reader.Close();
        }

        private int FindIndex(string value)
        {
            for (int i = 0; i < ButtonUtility.Array.Length; i++)
            {
                if (ButtonUtility.Array[i].ToString() == value)
                {
                    return i;
                }
            }
            return -1;
        }

        private void ReadKeySettingFromIni(string fileName)
        {
            var keyConfig = new KeyConfig();
            var sr = new StreamReader(fileName);
            var s = sr.ReadToEnd().Replace("\r\n", "\n").Replace("\r", "\n");
            sr.Close();
            var sp = s.Split('\n');
            for (int i = 0; i < Math.Min(sp.Length, ButtonUtility.Array.Length); i++)
            {
                var secondsp = sp[i].Split(':');
                if (secondsp.Length >= 2)
                {
                    keyConfig.SetKeyMap(ButtonUtility.Array[i], (Key)int.Parse(secondsp[0]));
                    keyConfig.SetButtonMap(ButtonUtility.Array[i], int.Parse(secondsp[1]));
                }
            }
            keyConfigs.Add(keyConfig);
        }

        public void Write(string fileName)
        {
            var writer = XmlWriter.Create(fileName, new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "   ",
                NewLineChars = System.Environment.NewLine
            });

            writer.WriteStartElement("root");
            writer.WriteStartElement("KeyConfigs");
            foreach (KeyConfig keyConfig in keyConfigs)
            {
                writer.WriteStartElement("KeyConfig");
                writer.WriteAttributeString("Name", keyConfig.Name);
                WriteSetting(writer, keyConfig);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndElement();

            writer.Close();
        }

        private void WriteSetting(XmlWriter writer, KeyConfig keyConfig)
        {
            foreach (ButtonType buttonType in ButtonUtility.Array)
            {
                writer.WriteStartElement("Config");
                writer.WriteAttributeString("Type", buttonType.ToString());
                writer.WriteAttributeString("Key", ((int)keyConfig.GetKeyMap(buttonType)).ToString());
                writer.WriteAttributeString("Button", keyConfig.GetButtonMap(buttonType).ToString());
                writer.WriteEndElement();
            }
        }

        public void ChangeSettingFromName(string name)
        {
            for (int i = 0; i < keyConfigs.Count; i++)
            {
                if (keyConfigs[i].Name == name)
                {
                    currentConfigIndex = i;
                    break;
                }
            }
        }
    }

    public class KeyConfig
    {
        Dictionary<ButtonType, Key> keyMap;
        Dictionary<ButtonType, int> buttonMap;

        Key[] keys;
        int[] buttons;

        bool keyChanged;
        bool buttonChanged;

        public KeyConfig()
        {
            buttonMap = new Dictionary<ButtonType, int>();
            keyMap = new Dictionary<ButtonType, Key>();
            foreach (ButtonType type in ButtonUtility.Array)
            {
                buttonMap.Add(type, -1);
                keyMap.Add(type, Key.Unknown);
            }

            Name = "default";
        }

        public string Name
        {
            get;
            set;
        }

        public void SetKeyMap(ButtonType buttonType, Key value)
        {
            keyMap[buttonType] = value;
            keyChanged = true;
        }

        public void SetButtonMap(ButtonType buttonType, int value)
        {
            buttonMap[buttonType] = value;
            buttonChanged = true;
        }

        public Key GetKeyMap(ButtonType buttonType)
        {
            return keyMap[buttonType];
        }

        public int GetButtonMap(ButtonType buttonType)
        {
            return buttonMap[buttonType];
        }

        public Key[] Keys
        {
            get
            {
                if (keyChanged || keys == null)
                {
                    keys = new Key[ButtonUtility.Array.Length];
                    for (int i = 0; i < ButtonUtility.Array.Length; i++)
                    {
                        keys[i] = keyMap[(ButtonType)i];
                    }
                    keyChanged = false;
                }
                return keys;
            }
        }

        public int[] Buttons
        {
            get
            {
                if (buttonChanged || buttons == null)
                {
                    buttons = new int[ButtonUtility.Array.Length];
                    for (int i = 0; i < ButtonUtility.Array.Length; i++)
                    {
                        buttons[i] = buttonMap[(ButtonType)i];
                    }
                    buttonChanged = false;
                }
                return buttons;
            }
        }
    }
}
