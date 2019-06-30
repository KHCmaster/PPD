using PPDFramework.Web;
using PPDFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace PPDSingle
{
    class PerfectTrialCache
    {
        private static PerfectTrialCache instance = new PerfectTrialCache();
        Dictionary<string, List<PerfectTrialInfo>> trials = new Dictionary<string, List<PerfectTrialInfo>>();

        public static PerfectTrialCache Instance
        {
            get
            {
                return instance;
            }
        }

        private PerfectTrialCache()
        {
        }

        public void AddPerfectTrial(string scoreHash)
        {
            WebSongInformation webSongInfo = WebSongInformationManager.Instance[scoreHash];
            if (webSongInfo == null)
            {
                return;
            }

            if (!trials.TryGetValue(webSongInfo.Hash, out List<PerfectTrialInfo> trialList))
            {
                trialList = new List<PerfectTrialInfo>();
                trials[webSongInfo.Hash] = trialList;
            }
            var diff = webSongInfo.Difficulties.FirstOrDefault(d => d.Hash == scoreHash);
            trialList.Add(new PerfectTrialInfo(-1, webSongInfo.Hash, scoreHash, diff == null ? Difficulty.Easy : diff.Difficulty));
        }

        public void Update()
        {
            var scores = WebManager.Instance.GetScores();
            var trialInfos = WebManager.Instance.GetPerfectTrials();
            foreach (var trialInfo in trialInfos)
            {
                if (!trials.TryGetValue(trialInfo.ScoreLibraryId, out List<PerfectTrialInfo> trialList))
                {
                    trialList = new List<PerfectTrialInfo>();
                    trials[trialInfo.ScoreLibraryId] = trialList;
                }
                trialList.Add(trialInfo);
            }
            WebSongInformationManager.Instance.Update(false);
        }

        public bool IsPerfect(string scoreHash)
        {
            WebSongInformation webSongInfo = WebSongInformationManager.Instance[scoreHash];
            if (webSongInfo == null)
            {
                return false;
            }
            if (trials.TryGetValue(webSongInfo.Hash, out List<PerfectTrialInfo> perfectTrials))
            {
                var found = perfectTrials.FirstOrDefault(p => p.ScoreHash == scoreHash);
                if (found != null)
                {
                    return true;
                }
                Difficulty targetDifficulty = Difficulty.Other;
                foreach (var diff in webSongInfo.Difficulties)
                {
                    if (diff.Hash == scoreHash)
                    {
                        targetDifficulty = diff.Difficulty;
                        break;
                    }
                }
                foreach (var diff in webSongInfo.Difficulties)
                {
                    if (diff.Difficulty == targetDifficulty)
                    {
                        found = perfectTrials.FirstOrDefault(p => p.ScoreHash == diff.Hash);
                        if (found != null)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
