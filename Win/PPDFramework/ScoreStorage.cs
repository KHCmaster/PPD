using System.Collections.Generic;
using System.Xml.Linq;

namespace PPDFramework
{
    /// <summary>
    /// 譜面の永続ストレージです。
    /// </summary>
    public class ScoreStorage
    {
        Dictionary<string, string> storage;

        /// <summary>
        /// 値を取得します。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string this[string key]
        {
            get
            {
                if (!storage.TryGetValue(GetKey(key), out string ret))
                {
                    return null;
                }
                return ret;
            }
            set { storage[GetKey(key)] = value; }
        }

        /// <summary>
        /// IDを取得します。
        /// </summary>
        public int ID
        {
            get;
            private set;
        }

        /// <summary>
        /// 譜面のIDを取得します。
        /// </summary>
        public int SongInformationID
        {
            get;
            private set;
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="songInformationId"></param>
        public ScoreStorage(int songInformationId)
        {
            ID = -1;
            SongInformationID = songInformationId;
            storage = new Dictionary<string, string>();
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="id"></param>
        /// <param name="songInformationId"></param>
        /// <param name="data"></param>
        public ScoreStorage(int id, int songInformationId, string data)
        {
            ID = id;
            SongInformationID = songInformationId;
            storage = new Dictionary<string, string>();
            Load(data);
        }

        /// <summary>
        /// 譜面からストレージを取得します。
        /// </summary>
        /// <param name="songInfo"></param>
        /// <returns></returns>
        public static ScoreStorage GetStorageFromSongInformation(SongInformation songInfo)
        {
            return PPDDatabase.DB.GetScoreStorage(songInfo.ID);
        }

        /// <summary>
        /// リロードします。
        /// </summary>
        public void Reload()
        {
            var tempStorage = PPDDatabase.DB.GetScoreStorage(SongInformationID);
            storage = tempStorage.storage;
        }

        private string GetKey(string key)
        {
            if (key == null)
            {
                return "";
            }
            return key;
        }

        private void Load(string text)
        {
            var document = XDocument.Parse(text);
            foreach (var elem in document.Root.Elements("Pair"))
            {
                this[elem.Attribute("Key").Value] = elem.Attribute("Value").Value;
            }
        }

        /// <summary>
        /// 保存します。
        /// </summary>
        public void Save()
        {
            PPDDatabase.DB.SaveScoreStorage(this);
        }

        internal string GetString()
        {
            var document = new XDocument(new XElement("Root"));
            foreach (var pair in storage)
            {
                if (pair.Value == null)
                {
                    continue;
                }
                var elem = new XElement("Pair", new XAttribute("Key", pair.Key), new XAttribute("Value", pair.Value));
                document.Root.Add(elem);
            }
            return document.ToString();
        }

        /// <summary>
        /// キーを含むかどうか返します。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(string key)
        {
            return storage.ContainsKey(GetKey(key));
        }

        /// <summary>
        /// IDを更新します。
        /// </summary>
        /// <param name="id"></param>
        internal void UpdateId(int id)
        {
            ID = id;
        }
    }
}
