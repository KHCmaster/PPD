using System.Collections.Generic;

namespace PPDFramework.Scene
{
    /// <summary>
    /// シーンインターフェース
    /// </summary>
    public interface ISceneBase
    {
        /// <summary>
        /// シーンマネージャー
        /// </summary>
        SceneManager SceneManager { get; set; }

        /// <summary>
        /// サウンド
        /// </summary>
        ISound Sound { get; set; }

        /// <summary>
        /// ゲームホスト
        /// </summary>
        IGameHost GameHost { get; set; }

        /// <summary>
        /// リソースマネージャー
        /// </summary>
        PPDFramework.Resource.ResourceManager ResourceManager { get; set; }

        /// <summary>
        /// 前のシーンからのパラメーター
        /// </summary>
        Dictionary<string, object> Param { get; set; }

        /// <summary>
        /// 前の自身のシーンからのパラメーター
        /// </summary>
        Dictionary<string, object> PreviousParam { get; set; }

        /// <summary>
        /// シーンスタック上で動作しているかどうか
        /// </summary>
        bool IsInSceneStack { get; set; }

        /// <summary>
        /// ロードを終わるべきかどうか
        /// </summary>
        bool ShouldFinishLoad { get; set; }

        /// <summary>
        /// スプライトのディレクトリ
        /// </summary>
        string SpriteDir { get; }

        /// <summary>
        /// ロードする(非同期実行されます)
        /// </summary>
        bool Load();

        /// <summary>
        /// 更新する
        /// </summary>
        /// <param name="inputInfo">入力の情報</param>
        /// <param name="mouseInfo">マウスの情報</param>
        void Update(InputInfoBase inputInfo, MouseInfo mouseInfo);

        /// <summary>
        /// 描画する
        /// </summary>
        void Draw();

        /// <summary>
        /// 処分する
        /// </summary>
        void Dispose();

        /// <summary>
        /// シーンスタックがポップされ現在のシーンになった時によばれます
        /// </summary>
        void SceneStackPoped(Dictionary<string, object> param);
    }
}
