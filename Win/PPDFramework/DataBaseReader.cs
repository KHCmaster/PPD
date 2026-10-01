using System.Data.SQLite;
using System.Threading;

namespace PPDFramework
{
    /// <summary>
    /// 読み取りの SQL の結果です。破棄するまでデータベースのロックを保持します。
    /// </summary>
    /// <remarks>
    /// コンストラクタです。
    /// </remarks>
    /// <param name="reader">結果のリーダー。</param>
    /// <param name="lockObject">データベースのロックに使うオブジェクト。</param>
    /// <param name="lockTaken">ロックを取得できたかどうか。</param>
    public class DataBaseReader(SQLiteDataReader reader, object lockObject, bool lockTaken) : DisposableComponent
    {
        private object lockObject = lockObject;
        private readonly bool lockTaken = lockTaken;

        /// <summary>
        /// 結果を読み取るリーダーを取得します。
        /// </summary>
        public SQLiteDataReader Reader
        {
            get;
            private set;
        } = reader;

        /// <summary>
        /// ロックを解放し、リーダーを破棄します。
        /// </summary>
        protected override void DisposeResource()
        {
            base.DisposeResource();
            if (lockObject != null)
            {
                if (lockTaken)
                {
                    Monitor.Exit(lockObject);
                }
                lockObject = null;
            }

            Reader?.Dispose();
            Reader = null;
        }
    }
}
