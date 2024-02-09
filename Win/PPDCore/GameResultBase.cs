using PPDCoreModel;
using PPDFramework;
using PPDFramework.Web;
using PPDFrameworkCore;
using System;
using System.Collections.Generic;

namespace PPDCore
{
    /// <summary>
    /// ゲームの結果クラスです
    /// 必ずpublic実装する必要があります
    /// </summary>
    public abstract class GameResultBase : GameComponent
    {
        protected GameResultBase(PPDDevice device) : base(device)
        {

        }

        /// <summary>
        /// 結果がセットされた
        /// </summary>
        protected event EventHandler ResultSet;

        /// <summary>
        /// リターン
        /// </summary>
        public abstract event EventHandler Returned;

        /// <summary>
        /// リトライ
        /// </summary>
        public abstract event EventHandler Retryed;

        /// <summary>
        /// リプレイ
        /// </summary>
        public abstract event EventHandler Replayed;

        /// <summary>
        /// スコアが送信されたときのイベント
        /// </summary>
        public event Action<ErrorReason, Dictionary<string, string>> ScoreSent;

        /// <summary>
        /// スコアが送信されるときのイベント
        /// </summary>
        public event Action ScoreSending;

        /// <summary>
        /// ゲームホスト
        /// </summary>
        public IGameHost GameHost { get; set; }

        /// <summary>
        /// サウンド
        /// </summary>
        public ISound Sound { get; set; }

        /// <summary>
        /// PPDGameUtility
        /// </summary>
        public PPDGameUtility PPDGameUtility { get; set; }

        /// <summary>
        /// 結果評価
        /// </summary>
        public ResultEvaluateType Result { get; private set; }

        /// <summary>
        /// ハイスコアか
        /// </summary>
        protected bool HighScore { get; private set; }

        /// <summary>
        /// スコア
        /// </summary>
        protected int Score { get; private set; }

        /// <summary>
        /// 最大コンボ
        /// </summary>
        protected int MaxCombo { get; private set; }

        /// <summary>
        /// それぞれの評価の数
        /// </summary>
        protected int[] MarkEvals { get; private set; }

        /// <summary>
        /// 終了時間
        /// </summary>
        protected float FinishTime { get; private set; }

        /// <summary>
        /// リプレイ可能かどうかを取得します
        /// </summary>
        public bool CanReplay { get; internal set; }

        /// <summary>
        /// パーフェクトトライアルに成功したかどうかを取得します
        /// </summary>
        public bool PerfectTrialSucceess { get; private set; }

        /// <summary>
        /// ロードする
        /// </summary>
        public abstract void Load();

        /// <summary>
        /// 更新する
        /// </summary>
        /// <param name="inputInfo"></param>
        public abstract void Update(InputInfoBase inputInfo);

        /// <summary>
        /// リトライされた
        /// </summary>
        public abstract void Retry();

        /// <summary>
        /// リソースマネージャー
        /// </summary>
        public PPDFramework.Resource.ResourceManager ResourceManager { get; set; }

        /// <summary>
        /// ツイートマネージャー
        /// </summary>
        public IBlueSkyManager BlueSkyManager { get; set; }

        public IReviewManager ReviewManager { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="gameResultManager">ゲームリザルト</param>
        /// <param name="evatype">結果評価</param>
        /// <param name="finishtime">終了時間</param>
        /// <param name="util">PPDGameUtility</param>
        /// <param name="replaying">リプレイかどうか</param>
        /// <param name="inputs">入力データ</param>
        /// <param name="perfectTrialToken">パーフェクトトライアルのトークン</param>
        public void SetResult(GameResultManager gameResultManager, ResultEvaluateType evatype, float finishtime, PPDGameUtility util, byte[] inputs, bool replaying,
            string perfectTrialToken)
        {
            Score = gameResultManager.CurrentScore;
            MaxCombo = gameResultManager.MaxCombo;
            MarkEvals = gameResultManager.Evalutes;
            FinishTime = finishtime;
            if (evatype != ResultEvaluateType.Mistake)
            {
                Result = gameResultManager.ResultEvaluateType;
            }
            else
            {
                Result = ResultEvaluateType.Mistake;
            }
            UpdateScore(util, replaying);
            SendScore(util, inputs, replaying, perfectTrialToken);
            if (ResultSet != null) ResultSet.Invoke(this, EventArgs.Empty);
        }

        private void UpdateScore(PPDGameUtility util, bool replaying)
        {
            HighScore = false;
            if (util.IsRegular && !replaying)
            {
                HighScore = ScoreReaderWriter.WriteScore(util.SongInformation.ID, PPDGameUtility.Difficulty, MarkEvals, MaxCombo, Score, Result, FinishTime, PPDGameUtility.SongInformation);
            }
        }

        private void SendScore(PPDGameUtility util, byte[] inputs, bool replaying, string perfectTrialToken)
        {
            if (util.IsRegular && Result != ResultEvaluateType.Mistake && WebManager.Instance.IsLogined && !replaying)
            {
                var hash = util.SongInformation.GetScoreHash(util.Difficulty);
                if (hash != null)
                {
                    ScoreSending?.Invoke();
                    ThreadManager.Instance.GetThread(() =>
                    {
                        var reason = WebManager.Instance.PlayResult(hash, Score, MarkEvals[0], MarkEvals[1], MarkEvals[2], MarkEvals[3], MarkEvals[4], MaxCombo,
                            PPDGameUtility.SongInformation.StartTime, FinishTime, inputs, 3, perfectTrialToken, out Dictionary<string, string> data);

                        if (data.TryGetValue("PerfectTrialResult", out string result))
                        {
                            PerfectTrialSucceess |= result == "1";
                        }
                        ScoreSent?.Invoke(reason, data);
                    }).Start();
                }
            }
        }
    }
}
