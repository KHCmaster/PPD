using System;

namespace PPDFramework.Logger
{
    /// <summary>
    /// ログマネージャーです。
    /// </summary>
    public class LogManager
    {
        private static LogManager instance = new LogManager();

        /// <summary>
        /// インスタンスを取得します。
        /// </summary>
        public static LogManager Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// ログを受信したイベントです。
        /// </summary>
        public event Action<LogInfo> LogReceived;

        /// <summary>
        /// ログを追加します。
        /// </summary>
        /// <param name="logInfo">ログ。</param>
        public void AddLog(LogInfo logInfo)
        {
            OnLogReceived(logInfo);
        }

        private void OnLogReceived(LogInfo logInfo)
        {
            LogReceived?.Invoke(logInfo);
        }
    }
}
