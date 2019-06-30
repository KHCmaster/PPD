using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace PPDEditorCommon
{
    public class ParameterPreset
    {
        public string FilePath
        {
            get;
            private set;
        }

        public string PresetName
        {
            get;
            private set;
        }

        public KeyValuePair<string, string>[] Parameters
        {
            get;
            private set;
        }

        public ParameterPreset(string filePath, string presetName)
        {
            FilePath = filePath;
            PresetName = presetName;
            Load();
        }

        public void Load()
        {
            var document = XDocument.Load(FilePath);
            var parameters = new Dictionary<string, string>();
            foreach (var elem in document.Root.Elements("Parameter"))
            {
                parameters.Add(elem.Attribute("Key").Value, elem.Attribute("Value").Value);
            }
            Parameters = parameters.ToArray();
        }
    }
}
