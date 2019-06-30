using PPDEditorCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;

namespace PPDEditor.FlowScript
{
    class Storage : IStorage
    {
        private const string StoragePath = "PPDEditor_Storage.xml";
        Dictionary<Tuple<string, string>, string> storage;

        public bool IsChanged
        {
            get;
            private set;
        }

        public Storage()
        {
            storage = new Dictionary<Tuple<string, string>, string>();
            Load();
        }

        private void Load()
        {
            if (!File.Exists(StoragePath))
            {
                return;
            }

            try
            {
                var document = XDocument.Load(StoragePath);
                foreach (var elem in document.Root.Elements("Storage"))
                {
                    storage[Tuple.Create(elem.Attribute("StorageKey").Value, elem.Attribute("Key").Value)] =
                        elem.Attribute("Value").Value;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error in loading PPDEditor_Storage.xml" + "\n" + e);
            }
        }

        public void Save()
        {
            if (!IsChanged)
            {
                return;
            }

            try
            {
                var document = new XDocument(new XElement("Root"));
                foreach (var s in storage)
                {
                    var elem = new XElement("Storage", new XAttribute("StorageKey", s.Key.Item1),
                        new XAttribute("Key", s.Key.Item2), new XAttribute("Value", s.Value));
                    document.Root.Add(elem);
                }
                document.Save(StoragePath);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error in writing PPDEditor_Storage.xml" + "\n" + e);
            }

            IsChanged = false;
        }

        #region IStorage メンバー

        public string GetValue(string storageKey, string key)
        {
            if (storage.TryGetValue(Tuple.Create(storageKey, key), out string ret))
            {
                return ret;
            }
            return null;
        }

        public void SetValue(string storageKey, string key, string value)
        {
            if (storage.TryGetValue(Tuple.Create(storageKey, key), out string temp))
            {
                if (temp == value)
                {
                    return;
                }
            }
            storage[Tuple.Create(storageKey, key)] = value;
            IsChanged = true;
        }

        #endregion
    }
}
