using PPDFramework;
using PPDFrameworkCore;

namespace PPDSingle
{
    public class UserPlayState : IPropertyChanged
    {
        const int MIN = 0;
        const int MAX = 1023;
        int score;
        int life = MAX / 2;
        MarkEvaluateType evaluate = (MarkEvaluateType)(-1);
        bool isMissPress;

        public UserPlayState(UserInfo user)
        {
            User = user;
            Rank = 1;
        }

        public UserInfo User
        {
            get;
            private set;
        }

        public int Score
        {
            get { return score; }
            set
            {
                if (score != value)
                {
                    score = value;
                    OnPropertyChanged("Score");
                }
            }
        }

        public float LifeAsFloat
        {
            get
            {
                return (float)life / MAX * 100;
            }
        }

        public int Life
        {
            get { return life; }
            set
            {
                if (life != value)
                {
                    life = value;
                    OnPropertyChanged("Life");
                    OnPropertyChanged("LifeAsFloat");
                }
            }
        }

        public MarkEvaluateType Evaluate
        {
            get { return evaluate; }
            set
            {
                evaluate = value;
                OnPropertyChanged("Evaluate");
            }
        }

        public bool IsMissPress
        {
            get { return isMissPress; }
            set
            {
                isMissPress = value;
                OnPropertyChanged("IsMissPress");
            }
        }

        public int Rank
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
