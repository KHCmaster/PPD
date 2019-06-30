using PPDFramework;
using System.Linq;

namespace PPDCoreModel
{
    public class GameResultManager
    {
        public const int MINLIFE = 0;
        public const int MAXLIFE = 1023;
        public const int DEFAULTLIFE = MAXLIFE / 2;
        int combo;
        int[] evacount = new int[5];
        int currentlife = MAXLIFE / 2;

        public GameResultManager()
        {
            CurrentLife = DEFAULTLIFE;
        }

        public void Retry(bool isReplaying)
        {
            MaxCombo = 0;
            CurrentCombo = 0;
            CurrentLife = DEFAULTLIFE;
            CurrentScore = 0;
            for (int i = 0; i < evacount.Length; i++)
            {
                evacount[i] = 0;
            }
            HoldBonus = 0;
            SlideBonus = 0;
            SuspendFinish = false;
            IsRetrying = true;
            IsReplaying = isReplaying;
        }

        public int MaxCombo
        {
            get;
            private set;
        }

        public bool SuspendFinish
        {
            get;
            set;
        }

        public int CurrentCombo
        {
            get { return combo; }
            set
            {
                combo = value;
                if (combo < 0)
                {
                    combo = 0;
                }
                if (combo > MaxCombo)
                {
                    MaxCombo = combo;
                }
            }
        }

        public int CurrentLife
        {
            get
            {
                return currentlife;
            }
            set
            {
                currentlife = value;
                if (currentlife > MAXLIFE)
                {
                    currentlife = MAXLIFE;
                }
                else if (currentlife < MINLIFE)
                {
                    currentlife = MINLIFE;
                }
            }
        }

        public float CurrentLifeAsFloat
        {
            get
            {
                return (float)currentlife / MAXLIFE * 100;
            }
        }

        public int CurrentScore
        {
            get;
            private set;
        }

        public int CoolCount
        {
            get
            {
                return evacount[(int)MarkEvaluateType.Cool];
            }
        }

        public int GoodCount
        {
            get
            {
                return evacount[(int)MarkEvaluateType.Fine];
            }
        }

        public int SafeCount
        {
            get
            {
                return evacount[(int)MarkEvaluateType.Safe];
            }
        }

        public int SadCount
        {
            get
            {
                return evacount[(int)MarkEvaluateType.Sad];
            }
        }

        public int WorstCount
        {
            get
            {
                return evacount[(int)MarkEvaluateType.Worst];
            }
        }

        public int EvaluateSum
        {
            get
            {
                return evacount.Sum();
            }
        }

        public int[] Evalutes
        {
            get
            {
                return new int[]{
                    CoolCount,
                    GoodCount,
                    SafeCount,
                    SadCount,
                    WorstCount
                };
            }
        }

        public ResultEvaluateType ResultEvaluateType
        {
            get
            {
                float betterRatio = (float)(CoolCount + GoodCount) / EvaluateSum;
                if (SafeCount == 0 && SadCount == 0 && WorstCount == 0)
                {
                    return ResultEvaluateType.Perfect;
                }
                else if (betterRatio >= 0.97)
                {
                    return ResultEvaluateType.Excellent;
                }
                else if (betterRatio >= 0.94)
                {
                    return ResultEvaluateType.Great;
                }
                else if (betterRatio >= 0.85)
                {
                    return ResultEvaluateType.Standard;
                }
                else
                {
                    return ResultEvaluateType.Cheap;
                }
            }
        }

        private int ComboBonus
        {
            get
            {
                if (combo >= 50)
                    return 250;
                else if (combo >= 40)
                    return 200;
                else if (combo >= 30)
                    return 150;
                else if (combo >= 20)
                    return 100;
                else if (combo >= 10)
                    return 50;
                else
                    return 0;
            }
        }

        public bool IfDeath
        {
            get
            {
                return CurrentLife <= 0;
            }
        }

        public int HoldBonus
        {
            get;
            private set;
        }

        public int SlideBonus
        {
            get;
            private set;
        }

        public int ExpectedTotalSlideBonus
        {
            get;
            set;
        }

        public bool IsRetrying
        {
            get;
            private set;
        }

        public bool IsReplaying
        {
            get;
            private set;
        }

        public void GainScore(MarkEvaluateType type, bool isMissPress, float scoreScale)
        {
            int basescore = 0;
            switch (type)
            {
                case MarkEvaluateType.Cool:
                    basescore = isMissPress ? 250 : 500;
                    break;
                case MarkEvaluateType.Fine:
                    basescore = isMissPress ? 150 : 300;
                    break;
                case MarkEvaluateType.Safe:
                    basescore = isMissPress ? 50 : 100;
                    break;
                case MarkEvaluateType.Sad:
                    basescore = isMissPress ? 30 : 50;
                    break;
                case MarkEvaluateType.Worst:
                    basescore = 0;
                    break;
            }
            CurrentScore += (int)(basescore * scoreScale) + ComboBonus;
        }

        public void GainScore(int gain)
        {
            CurrentScore += gain;
        }

        public void AddHoldBonus(int gain)
        {
            CurrentScore += gain;
            HoldBonus += gain;
        }

        public void AddSlideBonus(int gain)
        {
            CurrentScore += gain;
            SlideBonus += gain;
        }

        public void GainEvaluate(MarkEvaluateType evaluateType, int gain)
        {
            evacount[(int)evaluateType] += gain;
            if (evacount[(int)evaluateType] < 0)
            {
                evacount[(int)evaluateType] = 0;
            }
        }
    }
}
