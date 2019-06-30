using System;
using System.Collections.Generic;
using System.Text;
using PPDFramework;

namespace PPD
{
    public class GameResultManager
    {
        const int MIN = 0;
        const int MAX = 1023;
        int combo;
        int[] evacount = new int[5];
        int currentlife = MAX / 2;
        public GameResultManager()
        {
            CurrentLife = MAX / 2;
        }
        public void Retry()
        {
            MaxCombo = 0;
            CurrentCombo = 0;
            CurrentLife = MAX / 2;
            CurrentScore = 0;
            for (int i = 0; i < evacount.Length; i++)
            {
                evacount[i] = 0;
            }
        }
        public int MaxCombo
        {
            get;
            private set;
        }
        public int CurrentCombo
        {
            get { return combo; }
            set
            {
                combo = value;
                if (combo > MaxCombo) MaxCombo = combo;
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
                if (currentlife >= MAX) currentlife = MAX;
                else if (currentlife <= MIN) currentlife = MIN;
            }
        }

        public float CurrentLifeAsFloat
        {
            get
            {
                return (float)currentlife / MAX * 100;
            }
        }

        public int CurrentScore
        {
            get;
            private set;
        }
        public int[] Evacount
        {
            get
            {
                return evacount;
            }
        }
        private float ComboScale
        {
            get
            {
                float ret = 1;
                if (combo > 20)
                {
                    ret = 1.5f;
                }
                else if (combo > 50)
                {
                    ret = 2f;
                }
                else if (combo > 100)
                {
                    ret = 2.5f;
                }
                else if (combo > 500)
                {
                    ret = 3f;
                }
                return ret;
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

        public void GainScore(MarkEvaluateType type, bool isMissPress)
        {
            int basescore = 0;
            switch (type)
            {
                case MarkEvaluateType.Cool:
                    basescore = 500;
                    break;
                case MarkEvaluateType.Fine:
                    basescore = 300;
                    break;
                case MarkEvaluateType.Sad:
                    basescore = 100;
                    break;
                case MarkEvaluateType.Safe:
                    basescore = 50;
                    break;
                case MarkEvaluateType.Worst:
                    basescore = 0;
                    break;
            }
            CurrentScore += (isMissPress ? (int)Math.Round(basescore / 20.0) * 10 : basescore) + ComboBonus;
        }
        public void GainScore(int gain)
        {
            CurrentScore += gain;
        }
        public bool IfDeath
        {
            get
            {
                return CurrentLife <= 0;
            }
        }
    }
}
