using System.Data.SQLite;
using System.Threading;

namespace PPDFramework
{
    /// <summary>
    /// 読み取りの SQL の結果です。破棄するまでデータベースのロックを保持します。
    /// </summary>
    public class DataBaseReader : DisposableComponent
    {
        private object lockObject;
        private bool lockTaken;

        /// <summary>
        /// 結果を読み取るリーダーを取得します。
        /// </summary>
        public SQLiteDataReader Reader
        {
            get;
            private set;
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="reader">結果のリーダー。</param>
        /// <param name="lockObject">データベースのロックに使うオブジェクト。</param>
        /// <param name="lockTaken">ロックを取得できたかどうか。</param>
        public DataBaseReader(SQLiteDataReader reader, object lockObject, bool lockTaken)
        {
            this.lockObject = lockObject;
            this.lockTaken = lockTaken;
            Reader = reader;
        }

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
            if (Reader != null)
            {
                Reader.Dispose();
                Reader = null;
            }
        }
    }
}
