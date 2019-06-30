using System;
using System.Collections.Generic;
using System.Text;
using SlimDX.Direct3D9;

namespace PPDFramework
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
        /// デバイス
        /// </summary>
        Device Device { get; set; }

        /// <summary>
        /// スプライト
        /// </summary>
        Sprite Sprite { get; set; }

        /// <summary>
        /// サウンド
        /// </summary>
        ISound Sound { get; set; }

        /// <summary>
        /// ゲームホスト
        /// </summary>
        IGameHost GameHost { get; set; }

        /// <summary>
        /// 前のシーンからのパラメーター
        /// </summary>
        Dictionary<string, object> Param { get; set; }

        /// <summary>
        /// 前の自身のシーンからのパラメーター
        /// </summary>
        Dictionary<string, object> PreviousParam { get; set; }

        /// <summary>
        /// ロードする(非同期実行されます)
        /// </summary>
        void Load();

        /// <summary>
        /// 更新する
        /// </summary>
        /// <param name="presscount">ボタンを押したカウント</param>
        /// <param name="released">ボタンを離したか</param>
        void Update(int[] presscount, bool[] released);

        /// <summary>
        /// 描画する
        /// </summary>
        void Draw();

        /// <summary>
        /// 処分する
        /// </summary>
        void Dispose();
    }
}
