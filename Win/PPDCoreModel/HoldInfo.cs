using PPDCoreModel.Data;

namespace PPDCoreModel
{
    public class HoldInfo
    {
        public const string HoldStart = "PPDHoldStart";
        public const string HoldChange = "PPDHoldChange";
        public const string HoldEnd = "PPDHoldEnd";
        public const string MaxHold = "PPDMaxHold";

        public int CurrentHoldScore
        {
            get;
            private set;
        }

        public int MaxHoldBonus
        {
            get;
            private set;
        }

        public MarkType[] HoldMarks
        {
            get;
            private set;
        }

        public HoldInfo(int currentHoldScore, int maxHoldBonus, MarkType[] holdMarks)
        {
            CurrentHoldScore = currentHoldScore;
            MaxHoldBonus = maxHoldBonus;
            HoldMarks = holdMarks;
        }
    }
}
