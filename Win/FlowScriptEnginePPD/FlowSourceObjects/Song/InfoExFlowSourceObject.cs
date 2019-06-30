using FlowScriptEngine;
using PPDFramework;
using System;

namespace FlowScriptEnginePPD.FlowSourceObjects.Song
{
    [ToolTipText("Song_InfoEx_Summary")]
    public partial class InfoExFlowSourceObject : FlowSourceObjectBase
    {
        private int clearCount;
        private int allClearCount;
        private int playCount;
        private int allPlayCount;
        private int bestScore;
        private bool isFirstPlay;
        private bool isAllFirstPlay;
        private float bestPerformance;
        private int bestPerformanceCoolCount;
        private int bestPerformanceGoodCount;
        private int bestPerformanceSafeCount;
        private int bestPerformanceSadCount;
        private int bestPerformanceWorstCount;
        private int bestPerformanceScore;
        private ResultInfo[] results;

        public override string Name
        {
            get { return "PPD.Song.InfoEx"; }
        }

        static InfoExFlowSourceObject()
        {
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (this.Manager.Items.ContainsKey("PPDGameUtility"))
            {
                UpdateData(this.Manager.Items["PPDGameUtility"] as PPDGameUtility);
            }
        }

        protected override void OnReset()
        {
            if (this.Manager.Items.ContainsKey("PPDGameUtility"))
            {
                UpdateData(this.Manager.Items["PPDGameUtility"] as PPDGameUtility);
            }
        }

        private void UpdateData(PPDGameUtility gameUtility)
        {
            var str = gameUtility.SongInformation.GetScore(gameUtility.Difficulty);
            if (!int.TryParse(str, out int val))
            {
                val = 0;
            }
            bestScore = val;
            clearCount = allClearCount = playCount = allPlayCount = 0;
            bestPerformance = 0;

            ResultInfo bestPerformanceResult = null;
            results = ResultInfo.GetInfoFromSongInformation(gameUtility.SongInformation);
            foreach (ResultInfo resultInfo in results)
            {
                allPlayCount++;
                if (resultInfo.ResultEvaluate != ResultEvaluateType.Mistake)
                {
                    allClearCount++;
                }
                if (resultInfo.Difficulty != gameUtility.Difficulty)
                {
                    continue;
                }
                playCount++;
                if (resultInfo.ResultEvaluate == ResultEvaluateType.Mistake)
                {
                    continue;
                }
                clearCount++;

                if (IsBestPerformance(bestPerformanceResult, resultInfo))
                {
                    bestPerformanceResult = resultInfo;
                }
            }
            if (bestPerformanceResult != null)
            {
                bestPerformance = GetRatio(bestPerformanceResult);
                bestPerformanceCoolCount = bestPerformanceResult.CoolCount;
                bestPerformanceGoodCount = bestPerformanceResult.GoodCount;
                bestPerformanceSafeCount = bestPerformanceResult.SafeCount;
                bestPerformanceSadCount = bestPerformanceResult.SadCount;
                bestPerformanceWorstCount = bestPerformanceResult.WorstCount;
                bestPerformanceScore = bestPerformanceResult.Score;
            }
            isAllFirstPlay = allPlayCount == 0;
            isFirstPlay = playCount == 0;
        }

        private bool IsBestPerformance(ResultInfo bestPerformance, ResultInfo result)
        {
            return CompareResult(bestPerformance, result) < 0;
        }

        private int CompareResult(ResultInfo result1, ResultInfo result2)
        {
            if (result1 == null && result2 == null)
            {
                return 0;
            }
            if (result1 == null)
            {
                return -1;
            }
            if (result2 == null)
            {
                return 1;
            }
            var ratio1 = GetRatio(result1);
            var ratio2 = GetRatio(result2);
            if (ratio1 != ratio2)
            {
                return Math.Sign(ratio1 - ratio2);
            }
            if (result1.Score != result2.Score)
            {
                return result1.Score - result2.Score;
            }
            if (result1.CoolCount != result2.CoolCount)
            {
                return result1.CoolCount - result2.CoolCount;
            }
            if (result1.GoodCount != result2.GoodCount)
            {
                return result1.GoodCount - result2.GoodCount;
            }
            if (result1.SafeCount != result2.SafeCount)
            {
                return result1.SafeCount - result2.SafeCount;
            }
            if (result1.SadCount != result2.SadCount)
            {
                return result1.SadCount - result2.SadCount;
            }
            if (result1.WorstCount != result2.WorstCount)
            {
                return result1.WorstCount - result2.WorstCount;
            }
            if (result1.MaxCombo != result2.MaxCombo)
            {
                return result1.MaxCombo - result2.MaxCombo;
            }

            return 0;
        }

        private float GetRatio(ResultInfo result)
        {
            return (float)(result.CoolCount + result.GoodCount) /
                (result.CoolCount + result.GoodCount + result.SafeCount + result.SadCount + result.WorstCount);
        }

        [ToolTipText("Song_InfoEx_ClearCount")]
        public int ClearCount
        {
            get { return clearCount; }
        }

        [ToolTipText("Song_InfoEx_AllClearCount")]
        public int AllClearCount
        {
            get { return allClearCount; }
        }

        [ToolTipText("Song_InfoEx_PlayCount")]
        public int PlayCount
        {
            get { return playCount; }
        }

        [ToolTipText("Song_InfoEx_AllPlayCount")]
        public int AllPlayCount
        {
            get { return allPlayCount; }
        }

        [ToolTipText("Song_InfoEx_BestScore")]
        public int BestScore
        {
            get { return bestScore; }
        }

        [ToolTipText("Song_InfoEx_IsFirstPlay")]
        public bool IsFirstPlay
        {
            get { return isFirstPlay; }
        }

        [ToolTipText("Song_InfoEx_IsAllFirstPlay")]
        public bool IsAllFirstPlay
        {
            get { return isAllFirstPlay; }
        }

        [ToolTipText("Song_InfoEx_BestPerformance")]
        public float BestPerformance
        {
            get { return bestPerformance; }
        }

        [ToolTipText("Song_InfoEx_BestPerformanceCoolCount")]
        public int BestPerformanceCoolCount
        {
            get { return bestPerformanceCoolCount; }
        }

        [ToolTipText("Song_InfoEx_BestPerformanceGoodCount")]
        public int BestPerformanceGoodCount
        {
            get { return bestPerformanceGoodCount; }
        }

        [ToolTipText("Song_InfoEx_BestPerformanceSafeCount")]
        public int BestPerformanceSafeCount
        {
            get { return bestPerformanceSafeCount; }
        }

        [ToolTipText("Song_InfoEx_BestPerformanceSadCount")]
        public int BestPerformanceSadCount
        {
            get { return bestPerformanceSadCount; }
        }

        [ToolTipText("Song_InfoEx_BestPerformanceWorstCount")]
        public int BestPerformanceWorstCount
        {
            get { return bestPerformanceWorstCount; }
        }

        [ToolTipText("Song_InfoEx_BestPerformanceScore")]
        public int BestPerformanceScore
        {
            get { return bestPerformanceScore; }
        }

        [ToolTipText("Song_InfoEx_Results")]
        public object[] Results
        {
            get { return results; }
        }
    }
}
