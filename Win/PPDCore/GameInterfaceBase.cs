using PPDFramework;
using System.Numerics;
using PPDMath;

namespace PPDCore
{
    /// <summary>
    /// ゲームのインタフェースクラスです
    /// 必ずpublic実装する必要があります
    /// </summary>
    /// <remarks>
    /// コンストラクタです。
    /// </remarks>
    /// <param name="device"></param>
    public abstract class GameInterfaceBase(PPDDevice device) : GameComponent(device)
    {

        /// <summary>
        /// 最小動画位置
        /// </summary>
        public const float MinMoviePostion = 0;

        /// <summary>
        /// 最大動画位置
        /// </summary>
        public const float MaxMoviePosition = 100;

        /// <summary>
        /// サウンド
        /// </summary>
        public ISound Sound { get; set; }

        /// <summary>
        /// PPDGameUtility
        /// </summary>
        public PPDGameUtility PPDGameUtility { get; set; }

        /// <summary>
        /// ロードする
        /// </summary>
        public abstract void Load();

        /// <summary>
        /// リトライされた
        /// </summary>
        public abstract void Retry();

        /// <summary>
        /// 歌詞が変更された
        /// </summary>
        /// <param name="kasi"></param>
        public abstract void ChangeKasi(string kasi);

        /// <summary>
        /// 歌詞で使われる文字を、先に用意します（歌詞が切り替わったフレームが重くならないように、読み込みのときに呼ばれます）。
        /// </summary>
        /// <param name="characters">歌詞で使われる文字。</param>
        public virtual void PrefetchKasi(string characters)
        {
        }

        /// <summary>
        /// コンボが変更された
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="currentCombo"></param>
        public abstract void ChangeCombo(Vector2 pos, int currentCombo);

        /// <summary>
        /// 評価が変更された
        /// </summary>
        /// <param name="type">評価タイプ</param>
        /// <param name="isMissPress">押し間違いか</param>
        /// <param name="currentLife">現在のライフ</param>
        /// <param name="currentEvaluateRatio">現在の結果評価比</param>
        /// <param name="currentResultType">現在の結果評価</param>
        public abstract void ChangeEvaluate(MarkEvaluateType type, bool isMissPress, float currentLife, float currentEvaluateRatio, ResultEvaluateType currentResultType);

        /// <summary>
        /// スコアが変更された
        /// </summary>
        /// <param name="currentScore">現在のスコア</param>
        public abstract void ChangeScore(int currentScore);

        /// <summary>
        /// 現在の動画位置
        /// </summary>
        /// <param name="position"></param>
        public abstract void ChangeMoviePosition(float position);

        /// <summary>
        /// リソースマネージャー
        /// </summary>
        public PPDFramework.Resource.ResourceManager ResourceManager { get; set; }
    }
}
