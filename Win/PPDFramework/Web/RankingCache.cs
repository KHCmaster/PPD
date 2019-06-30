using System;

namespace PPDFramework.Web
{
    class RankingCache
    {
        private static RankingCache cache = new RankingCache();

        public static RankingCache Cache
        {
            get
            {
                return cache;
            }
        }

        ArrayTree<byte, RankingCacheInfo> tree;
        ArrayTree<byte, RankingCacheInfo> rivalTree;

        private RankingCache()
        {
            tree = new ArrayTree<byte, RankingCacheInfo>();
            rivalTree = new ArrayTree<byte, RankingCacheInfo>();
        }

        public bool IsRankingAvailable(byte[] primaryHash)
        {
            lock (tree)
            {
                return tree.Find(primaryHash) != null;
            }
        }

        public Ranking GetRanking(byte[] primaryHash, bool forceUpdate = false)
        {
            lock (tree)
            {
                var temp = tree.Find(primaryHash);
                if (temp != null)
                {
                    if (DateTime.Now - temp.CreatedTime < TimeSpan.FromMinutes(1) && !forceUpdate)
                    {
                        return temp.Ranking;
                    }
                    else
                    {
                        tree.Remove(primaryHash);
                    }
                }
            }

            var scoreId = WebManager.Instance.GetScoreId(primaryHash);
            if (scoreId == "")
            {
                return null;
            }
            lock (tree)
            {
                var ranking = WebManager.Instance.GetRanking(scoreId);
                tree.Add(primaryHash, new RankingCacheInfo(ranking, primaryHash));
                return ranking;
            }
        }

        public Ranking GetRivalRanking(byte[] primaryHash, bool forceUpdate = false)
        {
            lock (rivalTree)
            {
                var temp = rivalTree.Find(primaryHash);
                if (temp != null)
                {
                    if (DateTime.Now - temp.CreatedTime < TimeSpan.FromMinutes(1) && !forceUpdate)
                    {
                        return temp.Ranking;
                    }
                    else
                    {
                        rivalTree.Remove(primaryHash);
                    }
                }
            }

            var scoreId = WebManager.Instance.GetScoreId(primaryHash);
            if (scoreId == "")
            {
                return null;
            }
            lock (rivalTree)
            {
                var ranking = WebManager.Instance.GetRivalRanking(scoreId);
                rivalTree.Add(primaryHash, new RankingCacheInfo(ranking, primaryHash));
                return ranking;
            }
        }

        class RankingCacheInfo
        {
            public RankingCacheInfo(Ranking ranking, byte[] hash)
            {
                Ranking = ranking;
                Hash = hash;
                CreatedTime = DateTime.Now;
            }

            public Ranking Ranking
            {
                get;
                private set;
            }

            public Byte[] Hash
            {
                get;
                private set;
            }

            public DateTime CreatedTime
            {
                get;
                private set;
            }
        }
    }
}
