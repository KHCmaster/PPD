namespace PPDFramework
{
    public class DataBaseTransaction : DisposableComponent
    {
        public DataBaseTransaction()
        {
            PPDDatabase.DB.Begin();
        }

        protected override void DisposeResource()
        {
            PPDDatabase.DB.Commit();
        }
    }
}
