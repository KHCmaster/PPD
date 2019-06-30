using SharpDX;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace PPDFramework.Mod
{
    class ModSettingDatabase : SettingDataBase
    {
        private static ModSettingDatabase setting = new ModSettingDatabase();

        private ModSettingDatabase()
        {

        }

        public static ModSettingDatabase Setting
        {
            get
            {
                return setting;
            }
        }

        public override string Name
        {
            get { return "ModSetting.setting"; }
        }

        public Dictionary<string, object> GetSetting(string key)
        {
            string setting = this[key];
            if (String.IsNullOrEmpty(setting))
            {
                return new Dictionary<string, object>();
            }

            return Parse(setting);
        }

        private Dictionary<string, object> Parse(string setting)
        {
            var document = XDocument.Parse(setting);
            var query = from p in document.Root.Elements("Setting")
                        select
                            new
                            {
                                Key = p.Attribute("Key").Value,
#pragma warning disable RECS0069 // Redundant explicit property name
                                Value = p.Attribute("Value").Value,
#pragma warning restore RECS0069 // Redundant explicit property name
                                Type = p.Attribute("Type").Value,
                            };
            var ret = new Dictionary<string, object>();
            foreach (var elem in query)
            {
                object addValue = null;
                switch (elem.Type)
                {
                    case "Int32":
                        int intVal;
                        if (int.TryParse(elem.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out intVal))
                        {
                            addValue = intVal;
                        }
                        break;
                    case "Float":
                        float floatVal;
                        if (float.TryParse(elem.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out floatVal))
                        {
                            addValue = floatVal;
                        }
                        break;
                    case "Double":
                        double doubleVal;
                        if (double.TryParse(elem.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out doubleVal))
                        {
                            addValue = doubleVal;
                        }
                        break;
                    case "String":
                        addValue = elem.Value;
                        break;
                    case "Boolean":
                        bool boolVal;
                        if (bool.TryParse(elem.Value, out boolVal))
                        {
                            addValue = boolVal;
                        }
                        break;
                    case "Color4":
                        if (int.TryParse(elem.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out intVal))
                        {
                            addValue = new Color4(intVal);
                        }
                        break;
                    default:
                        throw new Exception("Invalid Data Type");
                }
                if (addValue != null)
                {
                    ret.Add(elem.Key, addValue);
                }
            }

            return ret;
        }

        public void SetSetting(string key, Dictionary<string, object> dict)
        {
            if (String.IsNullOrEmpty(key))
            {
                return;
            }

            this[key] = GetString(dict);
        }

        private string GetString(Dictionary<string, object> dict)
        {
            var document = new XDocument(new XElement("Root"));
            foreach (KeyValuePair<string, object> pair in dict)
            {
                string type = null;
                string value = null;
                if (pair.Value is Int32)
                {
                    type = "Int32";
                    value = ((int)pair.Value).ToString(CultureInfo.InvariantCulture);
                }
                else if (pair.Value is Single)
                {
                    type = "Float";
                    value = ((float)pair.Value).ToString(CultureInfo.InvariantCulture);
                }
                else if (pair.Value is Double)
                {
                    type = "Double";
                    value = ((double)pair.Value).ToString(CultureInfo.InvariantCulture);
                }
                else if (pair.Value is String)
                {
                    type = "String";
                    value = (string)pair.Value;
                }
                else if (pair.Value is Boolean)
                {
                    type = "Boolean";
                    value = ((bool)pair.Value).ToString(CultureInfo.InvariantCulture);
                }
                else if (pair.Value is Color4)
                {
                    type = "Color4";
                    value = ((Color4)pair.Value).ToRgba().ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    throw new Exception("Invalid Data Type");
                }
                document.Root.Add(new XElement("Setting", new XAttribute("Key", pair.Key),
                    new XAttribute("Value", value), new XAttribute("Type", type)));
            }

            return document.ToString();
        }

        public bool GetIsApplied(string key)
        {
            string setting = this[GetAppliedKey(key)];
            if (String.IsNullOrEmpty(setting))
            {
                return false;
            }
            bool.TryParse(setting, out bool ret);
            return ret;
        }

        public void SetIsApplied(string key, bool value)
        {
            this[GetAppliedKey(key)] = value.ToString();
        }

        private string GetAppliedKey(string key)
        {
            return String.Format("IsApplied::{0}", key);
        }
    }
}
