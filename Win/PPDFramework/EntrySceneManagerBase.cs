using PPDFramework.Scene;
using System.Collections.Generic;

namespace PPDFramework
{
    /// <summary>
    /// エントリーシーンマネージャーのクラスです
    /// 必ずpublic実装する必要があります
    /// </summary>
    public abstract class EntrySceneManagerBase
    {
        /// <summary>
        /// スプライトのディレクトリを取得します。
        /// </summary>
        public abstract string SpriteDir { get; }

        /// <summary>
        /// 起動引数つきのシーンを取得します
        /// </summary>
        /// <param name="device"></param>
        /// <param name="args"></param>
        /// <param name="dic"></param>
        /// <returns></returns>
        public abstract ISceneBase GetSceneWithArgs(PPDDevice device, PPDExecuteArg args, out Dictionary<string, object> dic);
    }
}
