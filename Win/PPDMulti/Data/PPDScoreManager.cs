using PPDFrameworkCore;
using PPDMultiCommon.Web;
using System.Linq;

namespace PPDMulti.Data
{
    class PPDScoreManager
    {
        private static PPDScoreManager manager = new PPDScoreManager();
        private PPDScoreInfo[] scores = new PPDScoreInfo[0];

        public static PPDScoreManager Manager
        {
            get
            {
                return manager;
            }
        }

        private PPDScoreManager()
        {
        }

        public void Initialize()
        {
            ThreadManager.Instance.GetThread(() =>
            {
                scores = WebManager.GetScores();
            }).Start();
        }

        public PPDScoreInfo GetScoreByHash(byte[] difficutyHash)
        {
            return scores.FirstOrDefault(score =>
            {
                return Utility.IsSameArray(difficutyHash, score.Easy) ||
                       Utility.IsSameArray(difficutyHash, score.Normal) ||
                       Utility.IsSameArray(difficutyHash, score.Hard) ||
                       Utility.IsSameArray(difficutyHash, score.Extreme);
            });
        }
    }
}
