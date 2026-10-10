namespace PPDFramework
{
    /// <summary>
    /// データベースのトランザクションです。作成すると開始し、破棄すると確定します。
    /// </summary>
    public class DataBaseTransaction : DisposableComponent
    {
        /// <summary>
        /// トランザクションを開始します。
        /// </summary>
        public DataBaseTransaction()
        {
            PPDDatabase.DB.Begin();
        }

        /// <summary>
        /// トランザクションを確定します。
        /// </summary>
        protected override void DisposeResource()
        {
            PPDDatabase.DB.Commit();
        }
    }
}
