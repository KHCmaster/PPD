using System;
using System.Globalization;

namespace PPDMultiCommon.Web
{
    public class RoomInfo
    {
        public RoomInfo(string userName, string roomName, string password, int port)
        {
            if (String.IsNullOrEmpty(userName.Trim()))
            {
                userName = "NONAME";
            }

            if (String.IsNullOrEmpty(roomName.Trim()))
            {
                roomName = "NOROOMNAME";
            }

            UserName = userName;
            RoomName = roomName;

            if (password != "")
            {
                PasswordHash = WebManager.GetPasswordHash(password);
            }
            else
            {
                PasswordHash = "";
            }

            Port = port;
            Language = CultureInfo.CurrentCulture.ThreeLetterISOLanguageName;
        }

        public RoomInfo(string userName, string roomName, string password, int port, string language, string ip, int playerCount)
        {
            UserName = userName;
            RoomName = roomName;
            PasswordHash = password;
            Port = port;
            Language = language;
            IP = ip;
            PlayerCount = playerCount;
        }

        public string UserName
        {
            get;
            private set;
        }

        public string RoomName
        {
            get;
            private set;
        }

        public string PasswordHash
        {
            get;
            private set;
        }

        public int Port
        {
            get;
            private set;
        }

        public string Language
        {
            get;
            private set;
        }

        public string IP
        {
            get;
            private set;
        }

        public int PlayerCount
        {
            get;
            private set;
        }

        public bool HasPassword
        {
            get
            {
                return !String.IsNullOrEmpty(PasswordHash);
            }
        }
    }
}
