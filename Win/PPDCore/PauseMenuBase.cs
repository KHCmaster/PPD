using PPDFramework;
using System;

namespace PPDCore
{
    /// <summary>
    /// latency変更用デリゲート
    /// </summary>
    /// <param name="sign">+か-</param>
    /// <returns>現在のlantency</returns>
    public delegate float ChangeLatencyEventHandler(int sign);

    /// <summary>
    /// ポーズのクラスです
    /// 必ずpublic実装する必要があります
    /// </summary>
    public abstract class PauseMenuBase : GameComponent
    {
        protected PauseMenuBase(PPDDevice device) : base(device)
        {

        }

        /// <summary>
        /// 再開
        /// </summary>
        public abstract event EventHandler Resumed;

        /// <summary>
        /// リトライ
        /// </summary>
        public abstract event EventHandler Retryed;

        /// <summary>
        /// リプレイ
        /// </summary>
        public abstract event EventHandler Replayed;

        /// <summary>
        /// リターン
        /// </summary>
        public abstract event EventHandler Returned;

        /// <summary>
        /// Latencyの調整
        /// </summary>
        public abstract event ChangeLatencyEventHandler LatencyChanged;

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
        /// 更新する
        /// </summary>
        /// <param name="inputInfo"></param>
        public abstract void Update(InputInfoBase inputInfo);

        /// <summary>
        /// リトライされた
        /// </summary>
        /// <param name="canReplay"></param>
        public abstract void Retry(bool canReplay);

        /// <summary>
        /// リソースマネージャー
        /// </summary>
        public PPDFramework.Resource.ResourceManager ResourceManager { get; set; }
    }
}
