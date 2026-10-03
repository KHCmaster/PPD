using PPDFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace KeyConfiger.Models
{
    /// <summary>
    /// キーの設定（keyconfig.ini）を読み書きします。今の形式は XML で、以前の形式（1 行に「キー:ボタン」）も読めます。
    /// </summary>
    public static class KeyConfigStore
    {
        public static List<KeyConfig> Load(string path)
        {
            var configs = new List<KeyConfig>();
            if (File.Exists(path))
            {
                try
                {
                    LoadFromXml(path, configs);
                }
                catch
                {
                    configs.Clear();
                    configs.Add(LoadFromIni(path));
                }
            }
            if (configs.Count == 0)
            {
                configs.Add(new KeyConfig());
            }
            return configs;
        }

        private static void LoadFromXml(string path, List<KeyConfig> configs)
        {
            using var reader = XmlReader.Create(path, new XmlReaderSettings());
            while (reader.Read())
            {
                if (reader.IsStartElement())
                {
                    switch (reader.LocalName)
                    {
                        case "KeyConfigs":
                            configs.Clear();
                            break;
                        case "KeyConfig":
                            var keyConfig = new KeyConfig
                            {
                                Name = reader.GetAttribute("Name")
                            };
                            configs.Add(keyConfig);
                            ReadConfig(reader.ReadSubtree(), keyConfig);
                            break;
                    }
                }
            }
        }

        private static void ReadConfig(XmlReader reader, KeyConfig keyConfig)
        {
            while (reader.Read())
            {
                if (reader.IsStartElement() && reader.LocalName == "Config")
                {
                    int key = int.Parse(reader.GetAttribute("Key")), button = int.Parse(reader.GetAttribute("Button")),
                        index = Array.FindIndex(ButtonUtility.Array, b => b.ToString() == reader.GetAttribute("Type"));
                    if (index >= 0)
                    {
                        keyConfig.SetKeyMap(ButtonUtility.Array[index], key);
                        keyConfig.SetButtonMap(ButtonUtility.Array[index], button);
                    }
                }
            }
            reader.Close();
        }

        private static KeyConfig LoadFromIni(string path)
        {
            var keyConfig = new KeyConfig();
            var lines = File.ReadAllText(path).Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
            for (int i = 0; i < Math.Min(lines.Length, ButtonUtility.Array.Length); i++)
            {
                var values = lines[i].Split(':');
                if (values.Length >= 2)
                {
                    keyConfig.SetKeyMap(ButtonUtility.Array[i], int.Parse(values[0]));
                    keyConfig.SetButtonMap(ButtonUtility.Array[i], int.Parse(values[1]));
                }
            }
            return keyConfig;
        }

        public static void Save(string path, IEnumerable<KeyConfig> configs)
        {
            using var writer = XmlWriter.Create(path, new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "   ",
                NewLineChars = Environment.NewLine
            });
            writer.WriteStartElement("root");
            writer.WriteStartElement("KeyConfigs");
            foreach (KeyConfig keyConfig in configs)
            {
                writer.WriteStartElement("KeyConfig");
                writer.WriteAttributeString("Name", keyConfig.Name);
                foreach (ButtonType buttonType in ButtonUtility.Array)
                {
                    writer.WriteStartElement("Config");
                    writer.WriteAttributeString("Type", buttonType.ToString());
                    writer.WriteAttributeString("Key", keyConfig.GetKeyMap(buttonType).ToString());
                    writer.WriteAttributeString("Button", keyConfig.GetButtonMap(buttonType).ToString());
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
        }
    }

    /// <summary>
    /// 押されたキーやボタンを調べる入力です（DirectInput）。ウィンドウができてから作るので、ViewModel には後から渡す。
    /// </summary>
    public interface IInputDevice
    {
        string[] JoyStickNames { get; }

        int CurrentJoyStickIndex { get; set; }

        void Load();

        bool TryGetPressedKey(out int key);

        bool TryGetPressedButton(out int button);
    }
}
