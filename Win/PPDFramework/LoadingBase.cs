using PPDFramework.Scene;
using System.Collections.Generic;

namespace PPDFramework
{
    /// <summary>
    /// ローディング時のシーンです
    /// 必ずpublic実装する必要があります
    /// </summary>
    public abstract class LoadingBase : SceneBase
    {
        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="device"></param>
        protected LoadingBase(PPDDevice device) : base(device)
        {

        }

        /// <summary>
        /// ローディング開始時に呼ばれます。
        /// </summary>
        public abstract void EnterLoading();

        /// <summary>
        /// 情報を送信します。
        /// </summary>
        /// <param name="parameters">パラメーター。</param>
        public abstract void SendToLoading(Dictionary<string, object> parameters);
    }
}
