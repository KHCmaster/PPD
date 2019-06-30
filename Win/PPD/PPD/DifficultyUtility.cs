using System;
using System.Collections.Generic;
using System.Text;
using PPDFramework;
namespace PPD
{
    public static class DifficultyUtility
    {
        public static string ConvertDifficulty(Difficulty difficulty)
        {
            if (difficulty == Difficulty.Easy) return "easy";
            else if (difficulty == Difficulty.Normal) return "normal";
            else if (difficulty == Difficulty.Hard) return "hard";
            else if (difficulty == Difficulty.Extreme) return "extreme";
            else return "base";
        }
    }
}
