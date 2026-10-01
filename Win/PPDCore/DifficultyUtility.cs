using PPDFrameworkCore;
namespace PPDCore
{
    public static class DifficultyUtility
    {
        public static string ConvertDifficulty(Difficulty difficulty)
        {
            return difficulty switch
            {
                Difficulty.Easy => "easy",
                Difficulty.Normal => "normal",
                Difficulty.Hard => "hard",
                Difficulty.Extreme => "extreme",
                _ => "base",
            };
        }
    }
}
