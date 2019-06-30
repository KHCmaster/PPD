using PPDFrameworkCore;
using PPDMultiCommon.Data;

namespace PPDMultiCommon.Model
{
    public class UserInfo : IPropertyChanged
    {
        protected string name;
        protected string accountId;
        protected int id;
        protected string imagePath;
        protected bool isLeader;
        protected UserState state;
        protected int ping;
        protected bool hasSong;

        public string Ip
        {
            get;
            set;
        }

        public bool IsSelf
        {
            get;
            set;
        }

        public bool IsHost
        {
            get;
            set;
        }

        public bool IsLeader
        {
            get
            {
                return isLeader;
            }
            set
            {
                if (isLeader != value)
                {
                    isLeader = value;
                    OnPropertyChanged("IsLeader");
                }
            }
        }

        public bool HasSong
        {
            get
            {
                return hasSong;
            }
            set
            {
                if (hasSong != value)
                {
                    hasSong = value;
                    OnPropertyChanged("HasSong");
                }
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public string AccountId
        {
            get
            {
                return accountId;
            }
            set
            {
                if (accountId != value)
                {
                    accountId = value;
                    OnPropertyChanged("AccountId");
                }
            }
        }

        public int ID
        {
            get { return id; }
            set
            {
                if (id != value)
                {
                    id = value;
                    OnPropertyChanged("ID");
                }
            }
        }

        public string ImagePath
        {
            get { return imagePath; }
            set
            {
                if (imagePath != value)
                {
                    imagePath = value;
                    OnPropertyChanged("ImagePath");
                }
            }
        }

        public UserState CurrentState
        {
            get { return state; }
            set
            {
                if (state != value)
                {
                    state = value;
                    OnPropertyChanged("CurrentState");
                }
            }
        }

        public int Ping
        {
            get
            {
                return ping;
            }
            set
            {
                if (ping != value)
                {
                    ping = value;
                    OnPropertyChanged("Ping");
                }
            }
        }

        public SongInfo[] SongInfos
        {
            get;
            set;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(propertyName);
            }
        }

        #region IPropertyChanged メンバ

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
