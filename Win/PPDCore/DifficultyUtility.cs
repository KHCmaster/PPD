using PPDFrameworkCore;
namespace PPDCore
{
    public static class DifficultyUtility
    {
        public static string ConvertDifficulty(Difficulty difficulty)
        {
            switch (difficulty)
            {
                case Difficulty.Easy:
                    return "easy";
                case Difficulty.Normal:
                    return "normal";
                case Difficulty.Hard:
                    return "hard";
                case Difficulty.Extreme:
                    return "extreme";
                default:
                    return "base";
            }
        }
    }
}
