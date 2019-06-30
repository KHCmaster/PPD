using PPDFramework.Resource;
using System.Collections.Generic;

namespace PPDFramework.Scene
{
    /// <summary>
    /// 基底シーンクラス
    /// </summary>
    public abstract class SceneBase : GameComponent, ISceneBase
    {

        #region ISceneBase メンバ
        /// <summary>
        /// リソースマネージャー
        /// </summary>
        public ResourceManager ResourceManager
        {
            get;
            set;
        }

        /// <summary>
        /// シーンスタック上で動作しているかどうか
        /// </summary>
        public bool IsInSceneStack
        {
            get;
            set;
        }

        /// <summary>
        /// シーンマネージャー
        /// </summary>
        public SceneManager SceneManager
        {
            get;
            set;
        }

        /// <summary>
        /// サウンド
        /// </summary>
        public ISound Sound
        {
            get;
            set;
        }

        /// <summary>
        /// ゲームホスト
        /// </summary>
        public IGameHost GameHost
        {
            get;
            set;
        }

        /// <summary>
        /// 現在のパラメーター
        /// </summary>
        public Dictionary<string, object> Param
        {
            get;
            set;
        }

        /// <summary>
        /// 前のパラメーター
        /// </summary>
        public Dictionary<string, object> PreviousParam
        {
            get;
            set;
        }

        /// <summary>
        /// ロードを終わるべきかどうか
        /// </summary>
        public bool ShouldFinishLoad
        {
            get;
            set;
        }

        /// <summary>
        /// スプライトのディレクトリ
        /// </summary>
        public virtual string SpriteDir
        {
            get;
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="device"></param>
        protected SceneBase(PPDDevice device) : base(device)
        {

        }

        /// <summary>
        /// ロードします
        /// </summary>
        public virtual bool Load()
        {
            return true;
        }

        /// <summary>
        /// シーンスタックがポップされ現在のシーンになった時によばれます
        /// </summary>
        public virtual void SceneStackPoped(Dictionary<string, object> param)
        {
        }

        /// <summary>
        /// 更新します
        /// </summary>
        /// <param name="inputInfo"></param>
        /// <param name="mouseInfo"></param>
        public virtual void Update(InputInfoBase inputInfo, MouseInfo mouseInfo)
        {
        }
        #endregion
    }
}
