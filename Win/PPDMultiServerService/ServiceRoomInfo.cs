using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PPDMultiServerService
{
    class ServiceRoomInfo
    {
        public int Port
        {
            get;
            private set;
        }

        public bool RegisterToPPD
        {
            get;
            private set;
        }

        public string UserName
        {
            get;
            private set;
        }

        public string Password
        {
            get;
            private set;
        }

        public string RoomName
        {
            get;
            private set;
        }

        public string[] AllowedModIds
        {
            get;
            private set;
        }

        public static ServiceRoomInfo[] Parse(string filePath)
        {
            XDocument doc = XDocument.Load(filePath);
            return doc.Element("Rooms").Elements("Room").Select(room =>
                new ServiceRoomInfo
                {
                    Port = int.Parse(room.Element("Port").Value),
                    RegisterToPPD = room.Element("RegisterToPPD").Value == "True",
                    UserName = room.Element("UserName").Value,
                    Password = room.Element("Password").Value,
                    RoomName = room.Element("RoomName").Value,
                    AllowedModIds = room.Element("AllowedMods").Elements("Mod").Select(mod =>
                        mod.Element("ID").Value
                    ).Where(id => !String.IsNullOrEmpty(id)).ToArray(),
                }
            ).ToArray();
        }
    }
}
