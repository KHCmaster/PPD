using PPDFrameworkCore;

namespace PPDMultiCommon.Model
{
    public class Message : IPropertyChanged
    {
        UserInfo user;
        string text;
        bool isPrivate;

        public UserInfo User
        {
            get
            {
                return user;
            }
            set
            {
                if (user != value)
                {
                    user = value;
                    OnPropertyChanged("User");
                }
            }
        }

        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                if (text != value)
                {
                    text = value;
                    OnPropertyChanged("Text");
                }
            }
        }

        public bool IsPrivate
        {
            get
            {
                return isPrivate;
            }
            set
            {
                if (isPrivate != value)
                {
                    isPrivate = value;
                    OnPropertyChanged("IsPrivate");
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
