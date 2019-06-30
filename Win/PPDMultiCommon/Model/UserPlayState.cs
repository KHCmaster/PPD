using MessagePack;
using PPDFrameworkCore;
using PPDMultiCommon.Data;

namespace PPDMultiCommon.Model
{
    public class UserPlayState : IPropertyChanged
    {
        const int MIN = 0;
        const int MAX = 1023;
        [IgnoreMember]
        int score;
        [IgnoreMember]
        int life = MAX / 2;
        [IgnoreMember]
        MarkEvaluateTypeEx evaluate = (MarkEvaluateTypeEx)(-1);
        [IgnoreMember]
        bool isStealth;
        [IgnoreMember]
        bool loaded;

        public UserPlayState()
        {
            Rank = 1;
        }

        public bool Loaded
        {
            get { return loaded; }
            set
            {
                if (loaded != value)
                {
                    loaded = value;
                    OnPropertyChanged("Loaded");
                }
            }
        }

        public UserInfo User
        {
            get;
            set;
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

        public MarkEvaluateTypeEx Evaluate
        {
            get { return evaluate; }
            set
            {
                evaluate = value;
                OnPropertyChanged("Evaluate");
            }
        }

        public bool IsStealth
        {
            get { return isStealth; }
            set
            {
                isStealth = value;
                OnPropertyChanged("IsStealth");
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
