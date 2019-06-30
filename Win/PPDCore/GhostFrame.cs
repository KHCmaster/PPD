using PPDFramework;

namespace PPDCore
{
    public class GhostFrame
    {
        public float Time
        {
            get;
            private set;
        }

        public int? Score
        {
            get;
            private set;
        }

        public int? Life
        {
            get;
            private set;
        }

        public MarkEvaluateType? MarkEvaluateType
        {
            get;
            private set;
        }

        public bool IsMissPress
        {
            get;
            private set;
        }

        public GhostFrame(float time, int score, int life)
        {
            Time = time;
            Score = score;
            Life = life;
        }

        public GhostFrame(float time, MarkEvaluateType markEvaluateType, bool isMissPress)
        {
            Time = time;
            MarkEvaluateType = markEvaluateType;
            IsMissPress = isMissPress;
        }
    }
}
