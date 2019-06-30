using System.Data.SQLite;
using System.Threading;

namespace PPDFramework
{
    public class DataBaseReader : DisposableComponent
    {
        private object lockObject;
        private bool lockTaken;

        public SQLiteDataReader Reader
        {
            get;
            private set;
        }

        public DataBaseReader(SQLiteDataReader reader, object lockObject, bool lockTaken)
        {
            this.lockObject = lockObject;
            this.lockTaken = lockTaken;
            Reader = reader;
        }

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
