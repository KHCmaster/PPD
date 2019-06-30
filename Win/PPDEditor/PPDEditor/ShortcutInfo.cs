using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PPDEditor
{
    public class ShortcutInfo
    {
        public Keys Key
        {
            get;
            private set;
        }

        public bool Shift
        {
            get;
            private set;
        }

        public bool Control
        {
            get;
            private set;
        }

        public bool Alt
        {
            get;
            private set;
        }

        public ShortcutType ShortcutType
        {
            get;
            private set;
        }

        public string ScriptPath
        {
            get;
            private set;
        }

        public string ScriptPathWithExtension
        {
            get
            {
                return Path.ChangeExtension(ScriptPath, "fsml");
            }
        }

        public ShortcutInfo(Keys key, bool shift, bool control, bool alt, ShortcutType shortcutType)
        {
            Key = key;
            Shift = shift;
            Control = control;
            Alt = alt;
            ShortcutType = shortcutType;
        }

        public ShortcutInfo(Keys key, bool shift, bool control, bool alt, string scriptFilePath)
        {
            Key = key;
            Shift = shift;
            Control = control;
            Alt = alt;
            ShortcutType = ShortcutType.Custom;
            ScriptPath = scriptFilePath;
        }

        private string ConvertBoolean(bool b)
        {
            return b ? "1" : "0";
        }

        public string Serialize()
        {
            if (ShortcutType == ShortcutType.Custom)
            {
                return String.Format("Key:{0}, Control:{1}, Shift:{2}, Alt:{3}, Path:{4}",
                    Key, ConvertBoolean(Control), ConvertBoolean(Shift), ConvertBoolean(Alt), ScriptPath);
            }
            else
            {
                return String.Format("Key:{0}, Control:{1}, Shift:{2}, Alt:{3}, Type:{4}",
                    Key, ConvertBoolean(Control), ConvertBoolean(Shift), ConvertBoolean(Alt), ShortcutType);
            }
        }

        public static ShortcutInfo Deserialize(string str)
        {
            var regex = new Regex(@"Key:(\w+), Control:(\w+), Shift:(\w+), Alt:(\w+), Type:(\w+)");
            var scriptRegex = new Regex(@"Key:(\w+), Control:(\w+), Shift:(\w+), Alt:(\w+), Path:(.+)");
            var m = regex.Match(str);
            if (m.Success)
            {
                var key = (Keys)Enum.Parse(typeof(Keys), m.Groups[1].Value);
                bool control = m.Groups[2].Value == "1",
                    shift = m.Groups[3].Value == "1",
                    alt = m.Groups[4].Value == "1";
                var type = (ShortcutType)Enum.Parse(typeof(ShortcutType), m.Groups[5].Value);
                return new ShortcutInfo(key, shift, control, alt, type);
            }
            else
            {
                m = scriptRegex.Match(str);
                if (m.Success)
                {
                    var key = (Keys)Enum.Parse(typeof(Keys), m.Groups[1].Value);
                    bool control = m.Groups[2].Value == "1",
                        shift = m.Groups[3].Value == "1",
                        alt = m.Groups[4].Value == "1";
                    return new ShortcutInfo(key, shift, control, alt, m.Groups[5].Value);
                }
            }
            return null;
        }

        public string GetCommandText()
        {
            var temp = new List<string>();
            if (Control)
            {
                temp.Add("Ctrl");
            }
            if (Shift)
            {
                temp.Add("Shift");
            }
            if (Alt)
            {
                temp.Add("Alt");
            }
            temp.Add(Key.ToString());
            return String.Join("+", temp.ToArray());
        }
    }
}
