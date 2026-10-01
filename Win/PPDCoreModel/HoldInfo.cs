using PPDCoreModel.Data;

namespace PPDCoreModel
{
    public class HoldInfo(int currentHoldScore, int maxHoldBonus, MarkType[] holdMarks)
    {
        public const string HoldStart = "PPDHoldStart";
        public const string HoldChange = "PPDHoldChange";
        public const string HoldEnd = "PPDHoldEnd";
        public const string MaxHold = "PPDMaxHold";

        public int CurrentHoldScore
        {
            get;
            private set;
        } = currentHoldScore;

        public int MaxHoldBonus
        {
            get;
            private set;
        } = maxHoldBonus;

        public MarkType[] HoldMarks
        {
            get;
            private set;
        } = holdMarks;
    }
}
