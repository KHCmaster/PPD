using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace PPD
{
    class GameList
    {
        const string innerGameListPath = "skins\\GameList.xml";
        List<GameLoader> games;

        public GameList()
        {
            games = new List<GameLoader>();
            if (File.Exists(innerGameListPath))
            {
                var settings = new XmlReaderSettings();
                var reader = XmlReader.Create(innerGameListPath, settings);
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        switch (reader.LocalName)
                        {
                            case "GameList":
                                ParseGameList(reader.ReadSubtree());
                                break;
                        }
                    }
                }
                reader.Close();
            }
        }

        private void ParseGameList(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.IsStartElement())
                {
                    switch (reader.LocalName)
                    {
                        case "Game":
                            var sl = new GameLoader(Path.Combine("skins", reader.GetAttribute("Name")));
                            sl.Load();
                            games.Add(sl);
                            break;
                    }
                }
            }
            reader.Close();
        }

        public GameLoader[] List
        {
            get
            {
                return games.ToArray();
            }
        }
    }
}
