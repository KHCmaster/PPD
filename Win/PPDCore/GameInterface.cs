using PPDFramework;
using SharpDX;

namespace PPDCore
{
    /// <summary>
    /// メインゲームのUIクラス
    /// </summary>
    public class GameInterface : GameInterfaceBase
    {

        GameInterfaceTop mgt;
        GameInterfaceBottom mgb;
        int dangercount;

        float currentlife = 50;


        public GameInterface(PPDDevice device) : base(device)
        {
        }

        /// <summary>
        /// 読み込み
        /// </summary>
        public override void Load()
        {
            mgt = new GameInterfaceTop(device, ResourceManager, PPDGameUtility);
            mgb = new GameInterfaceBottom(device, ResourceManager, PPDGameUtility);
            this.AddChild(mgt);
            this.AddChild(mgb);
        }

        /// <summary>
        /// リトライ
        /// </summary>
        public override void Retry()
        {
            dangercount = 0;
            currentlife = 50;
            mgb.Retry();
            mgt.Retry();
            SetDefault();
        }

        /// <summary>
        /// 歌詞変更
        /// </summary>
        /// <param name="kasi"></param>
        public override void ChangeKasi(string kasi)
        {
            mgb.Kasi = kasi;
        }

        /// <summary>
        /// 動画の位置変更
        /// </summary>
        /// <param name="position"></param>
        public override void ChangeMoviePosition(float position)
        {
            mgb.MoviePosDrawX = (int)(800 * position / GameInterfaceBase.MaxMoviePosition);
        }

        /// <summary>
        /// マークの評価変更
        /// </summary>
        /// <param name="type">マークの評価タイプ</param>
        /// <param name="isMissPress">押し間違いか</param>
        /// <param name="currentLife">今のライフ</param>
        /// <param name="currentEvaluateRatio">現状の評価割合</param>
        /// <param name="currentResultType">現状の評価</param>
        public override void ChangeEvaluate(MarkEvaluateType type, bool isMissPress, float currentLife, float currentEvaluateRatio, ResultEvaluateType currentResultType)
        {
            mgb.CreateOnpu((4 - (int)type) / 2);
            mgb.CurrentLife = currentLife;
            mgb.ChangeTempEvaluate(currentEvaluateRatio, currentResultType);
            currentlife = currentLife;
        }

        /// <summary>
        /// スコアの変更
        /// </summary>
        /// <param name="currentScore"></param>
        public override void ChangeScore(int currentScore)
        {
            mgt.CurrentScore = currentScore;
        }

        /// <summary>
        /// コンボが起きた
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="currentCombo"></param>
        public override void ChangeCombo(Vector2 pos, int currentCombo)
        {
        }

        protected override void UpdateImpl()
        {
            if (currentlife < 20)
            {
                dangercount++;
            }
            else
            {
                dangercount = 0;
                mgt.Danger = false;
                mgb.Danger = false;
            }
            if (dangercount > 30)
            {
                mgb.Danger = true;
                mgt.Danger = true;
            }
            if (dangercount > 60)
            {
                dangercount = 0;
                mgt.Danger = false;
                mgb.Danger = false;
            }
        }
    }
}
