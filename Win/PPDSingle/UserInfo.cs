using PPDFrameworkCore;

namespace PPDSingle
{
    public class UserInfo : IPropertyChanged
    {
        protected string name;
        protected string accountId;
        protected string imagePath;

        public bool IsSelf
        {
            get;
            set;
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
