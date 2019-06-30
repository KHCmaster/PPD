using PPDFrameworkCore;

namespace PPDFramework.Web
{
    /// <summary>
    /// Web上のランキングを表すクラスです
    /// </summary>
    public class Ranking
    {
        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="easy">イージーのランキング。</param>
        /// <param name="normal">ノーマルのランキング。</param>
        /// <param name="hard">ハードのランキング。</param>
        /// <param name="extreme">エクストリームのランキング。</param>
        public Ranking(RankingInfo[] easy, RankingInfo[] normal, RankingInfo[] hard, RankingInfo[] extreme)
        {
            Easy = easy;
            Normal = normal;
            Hard = hard;
            Extreme = extreme;
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="info">ランキング。</param>
        /// <param name="difficulty">難易度。</param>
        public Ranking(RankingInfo[] info, Difficulty difficulty)
        {
            switch (difficulty)
            {
                case Difficulty.Easy:
                    Easy = info;
                    break;
                case Difficulty.Normal:
                    Normal = info;
                    break;
                case Difficulty.Hard:
                    Hard = info;
                    break;
                case Difficulty.Extreme:
                    Extreme = info;
                    break;
            }
        }

        /// <summary>
        /// Easyのランキングを取得します
        /// </summary>
        public RankingInfo[] Easy
        {
            get;
            private set;
        }

        /// <summary>
        /// Normalのランキングを取得します
        /// </summary>
        public RankingInfo[] Normal
        {
            get;
            private set;
        }

        /// <summary>
        /// Hardのランキングを取得します
        /// </summary>
        public RankingInfo[] Hard
        {
            get;
            private set;
        }

        /// <summary>
        /// Extremeのランキングを取得します
        /// </summary>
        public RankingInfo[] Extreme
        {
            get;
            private set;
        }

        /// <summary>
        /// 難易度を指定してランキングを取得します
        /// </summary>
        /// <param name="difficulty">難易度</param>
        /// <returns></returns>
        public RankingInfo[] GetInfo(Difficulty difficulty)
        {
            switch (difficulty)
            {
                case Difficulty.Easy:
                    return Easy;
                case Difficulty.Normal:
                    return Normal;
                case Difficulty.Hard:
                    return Hard;
                case Difficulty.Extreme:
                    return Extreme;
            }

            return null;
        }
    }
}
